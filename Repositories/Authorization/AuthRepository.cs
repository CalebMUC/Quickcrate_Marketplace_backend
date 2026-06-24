using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.General;
using Minimart_Api.Models;
using Minimart_Api.Services.EmailServices;
using Minimart_Api.Services.SignalR;
using StackExchange.Redis;
using Authentication_and_Authorization_Api.Core;
using Microsoft.AspNetCore.SignalR;

namespace Minimart_Api.Repositories.Authorization
{
    public class AuthRepository : IAuthRepository
    {
        private readonly MinimartDBContext _context;
        private readonly JwtSettings _jwtSettings;
        private readonly IConfiguration _configuration;
        private readonly CoreLibraries _coreLibraries;
        private readonly IHubContext<ActivityHub> _hubContext;
        private readonly ILogger<AuthRepository> _logger;
        private readonly BrevoEmailService _brevoEmailService;
        private readonly IDatabase _redisDatabase;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthRepository(MinimartDBContext dbContext, IOptions<JwtSettings> jwtsettings, CoreLibraries coreLibraries,
            IHubContext<ActivityHub> hubContext, ILogger<AuthRepository> logger, BrevoEmailService brevoEmailService, 
            IDatabase redisDatabase, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _context = dbContext;
            _jwtSettings = jwtsettings.Value;
            _coreLibraries = coreLibraries;
            _hubContext = hubContext;
            _logger = logger;
            _brevoEmailService = brevoEmailService;
            _redisDatabase = redisDatabase;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region Core Interface Methods

        public async Task<RegisterResponse> Register(Register register)
        {
            try
            {
                // Validate role input - only allow specific roles for registration
                var allowedRoles = new[] { "User", "Admin" }; // Note: Merchant registration should use separate endpoint
                var roleToAssign = string.IsNullOrWhiteSpace(register.Role) ? "User" : register.Role.Trim();
                
                if (!allowedRoles.Contains(roleToAssign, StringComparer.OrdinalIgnoreCase))
                {
                    return new RegisterResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = $"Invalid role '{register.Role}'. Only 'User' and 'Admin' roles are allowed for registration."
                    };
                }

                //check if user already exists by email
                var existingemail = await _userManager.FindByEmailAsync(register.Email);
                if (existingemail != null)
                {
                    return new RegisterResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = "User with the Provided Email Already Exists"
                    };
                }

                // Check phone number uniqueness - Use no-tracking query to avoid MerchantID issues
                var existingphone = await _userManager.Users
                    .AsNoTracking()
                    .Select(u => new { u.PhoneNumber, u.Id }) // Only select needed fields
                    .FirstOrDefaultAsync(u => u.PhoneNumber == register.PhoneNumber);
                
                if (existingphone != null)
                {
                    return new RegisterResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = "User with the Provided phonenumber Already Exists"
                    };
                }

                //create an instance of ApplicationUser with MINIMAL properties initially
                var user = new ApplicationUser
                {
                    // Essential Identity properties only
                    UserName = register.Email,
                    Email = register.Email,
                    PhoneNumber = register.PhoneNumber
                    // Don't set custom properties initially to avoid tracking issues
                };

                var result = await _userManager.CreateAsync(user, register.password);
                if (!result.Succeeded)
                {
                    return new RegisterResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                // Now update with custom properties AFTER creation
                user.DisplayName = register.UserName;
                user.FirstName = ExtractFirstName(register.UserName);
                user.LastName = ExtractLastName(register.UserName);
                user.CreatedAt = DateTime.UtcNow;
                user.IsLoggedIn = false;
                user.LastLogin = null;
                user.IsEmailVerified = true;
                user.EmailConfirmed = true;
                user.IsActive = true;
                user.FailedAttempts = 0;
                user.IsTemporaryPassword = false;

                // Update the user with custom properties
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _logger.LogWarning("Failed to update user {UserId} with custom properties. Errors: {Errors}",
                        user.Id, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                }

                // Assign the specified role (normalize case)
                var normalizedRole = char.ToUpperInvariant(roleToAssign[0]) + roleToAssign.Substring(1).ToLowerInvariant();
                await _userManager.AddToRoleAsync(user, normalizedRole);

                // Get the LegacyUserId (which will be 0 for new users since they don't have a legacy ID)
                //var userId = user.LegacyUserId ?? 0;

                // Send different notifications based on role
                var notificationMessage = normalizedRole == "Admin" 
                    ? $"New Admin has Registered: {register.UserName} | UserID: {user.Id} | Email: {register.Email}"
                    : $"New User has Registered: {register.UserName} | UserID: {user.Id} | Email: {register.Email}";

                await _hubContext.Clients.All.SendAsync("ReceiveNewUser", notificationMessage);

                // Log the registration for audit purposes
                _logger.LogInformation("New {Role} registered: {Email} with UserID: {UserId}", 
                    normalizedRole, register.Email, user.Id);

                return new RegisterResponse
                {
                    ResponseCode = 200,
                    ResponseMessage = $"You have successfully created an account as {normalizedRole}. Welcome to Minimart!",
                    Username = register.UserName,
                    Role = normalizedRole,
                    UserId = user.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email {Email} with role {Role}", 
                    register.Email, register.Role);
                return new RegisterResponse
                {
                    ResponseCode = 500,
                    ResponseMessage = "An error occurred during registration. Please try again later.",
                };
            }
        }

        public async Task<LoginResponse> Login(UserLogin userLogin)
        {
            if (string.IsNullOrEmpty(userLogin.EmailorPhone) || string.IsNullOrEmpty(userLogin.Password))
            {
                return new LoginResponse
                {
                    ResponseCode = 400,
                    ResponseMessage = "Email/Phone and Password are required"
                };
            }

            try
            {
                // Validate input format
                var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                var phoneRegex = @"^\+?[0-9]{10,15}$";

                bool isEmail = Regex.IsMatch(userLogin.EmailorPhone, emailRegex);
                bool isPhonenumber = Regex.IsMatch(userLogin.EmailorPhone, phoneRegex);

                if (!isEmail && !isPhonenumber)
                {
                    return new LoginResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = "Invalid email or phone number format"
                    };
                }

                // Fetch user from database
                ApplicationUser? user = null;
                if (isEmail)
                {
                    user = await _userManager.FindByEmailAsync(userLogin.EmailorPhone);
                }
                else
                {
                    // Use no-tracking query for phone number lookup to avoid MerchantID issues
                    var phoneUser = await _userManager.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.PhoneNumber == userLogin.EmailorPhone);
                    
                    if (phoneUser != null)
                    {
                        // Get the full user object for authentication
                        user = await _userManager.FindByIdAsync(phoneUser.Id);
                    }
                }

                if (user == null)
                {
                    return new LoginResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = isEmail
                            ? "User with the provided email doesn't exist"
                            : "User with the provided phone number doesn't exist"
                    };
                }

                // Verify password using Identity
                var result = await _signInManager.CheckPasswordSignInAsync(user, userLogin.Password, false);
                if (!result.Succeeded)
                {
                    return new LoginResponse
                    {
                        ResponseCode = 401,
                        ResponseMessage = "Invalid Password"
                    };
                }

                // Generate Token and Refresh Token
                var token = _coreLibraries.GenerateToken(user);
                var refreshToken = GenerateRefreshToken();

                // Update user details
                user.IsLoggedIn = true;
                user.LastLogin = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Get user role
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "User";

                return new LoginResponse
                {
                    Username = user.DisplayName,
                    UserId= user.Id,
                    UserRole = userRole,
                    Token = token,
                    Refreshtoken = refreshToken,
                    ResponseCode = 200,
                    ResponseMessage = "Login Successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return new LoginResponse
                {
                    ResponseCode = 500,
                    ResponseMessage = "An unexpected error occurred"
                };
            }
        }

        public async Task<Status> SendEmailVerificationCode(string email)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "User with the Provided Email Already Exists"
                    };
                }

                // 🔄 Rate Limiting
                string rateLimitKey = $"EmailRateLimit:{email}";
                var attemptCount = await _redisDatabase.StringGetAsync(rateLimitKey);

                if (attemptCount.HasValue && int.Parse(attemptCount) >= 3)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "Too many Password Resets. Please try again Later"
                    };
                }

                // Generate verification code
                var code = new Random().Next(100000, 999999).ToString();
                var verificationKey = $"EmailVerification:{email}";

                // Set verification code in Redis for 10 minutes
                await _redisDatabase.StringSetAsync(verificationKey, code, TimeSpan.FromMinutes(10));

                // Increment rate limit counter (or set it to 1 if not exists)
                if (!attemptCount.HasValue)
                {
                    await _redisDatabase.StringSetAsync(rateLimitKey, "1", TimeSpan.FromMinutes(10));
                }
                else
                {
                    await _redisDatabase.StringIncrementAsync(rateLimitKey);
                }

                await _brevoEmailService.SendAsync(email, "Email Verification Code", $"Your Email Verification Code is {code}");

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Email Validation Code Sent Successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email verification code to {Email}", email);
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Error Sending Reset Code"
                };
            }
        }

        public async Task<Status> SendResetCode(PasswordResetDto passwordReset)
        {
            try
            {
                //check if user with the provided Email exists
                var existingUser = await _userManager.FindByEmailAsync(passwordReset.Email);
                if (existingUser == null)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "User with the Provided Email doesnt Exist"
                    };
                }

                //Rate Limiting Limit the No of trials to 3 
                string RateLimitKey = $"PasswordResetLimit:{passwordReset.Email} ";
                var attemptCount = await _redisDatabase.StringGetAsync(RateLimitKey);

                if (attemptCount.HasValue && int.Parse(attemptCount) >= 3)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "Too many Password Resets. Please try again Later"
                    };
                }

                //Generate Password Reset Code
                var code = new Random().Next(100000, 999999).ToString();
                var verificationKey = $"ResetCodeVerification:{passwordReset.Email}";

                //set Code in Redis for  10mins
                await _redisDatabase.StringSetAsync(verificationKey, code, TimeSpan.FromMinutes(10));

                //increment Rate Limit Counter
                if (!attemptCount.HasValue)
                {
                    await _redisDatabase.StringSetAsync(RateLimitKey, 1, TimeSpan.FromMinutes(10));
                }
                else
                {
                    await _redisDatabase.StringIncrementAsync(RateLimitKey);
                }

                //send Code Via Email
                await _brevoEmailService.SendAsync(passwordReset.Email, "Password Reset Code", $"Your Password Reset Code is {code}");

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Password Reset Code Sent Successfully"
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending email verification code to {Email}", passwordReset.Email);

                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Error Sending Reset Code"
                };
            }
        }

        public async Task<Status> VerifyResetCode(VerifyResetCodeDto verifyResetCode)
        {
            try
            {
                //Retrieve Code stored in cache
                var codekey = $"ResetCodeVerification:{verifyResetCode.Email}";
                var storedCode = await _redisDatabase.StringGetAsync(codekey);

                //compare stored Code with input Code
                if (storedCode.IsNullOrEmpty || storedCode.ToString().Trim() != verifyResetCode.Code.Trim())
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = $"Invalid/Expired Reset Code for {verifyResetCode.Email} "//preferably a masked Email
                    };
                }

                //mark as verified and delete Code to prevent Reuse
                var verifiedKey = $"ResetCodeVerified:{verifyResetCode.Email}";
                await _redisDatabase.KeyDeleteAsync(codekey);
                //mark key as verified
                await _redisDatabase.StringSetAsync(verifiedKey, "true", TimeSpan.FromMinutes(10));

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = $" Code Verified,Proceed to Reset your Password"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset code for {Email}", verifyResetCode.Email);

                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error during code verification."
                };
            }
        }

        public async Task<Status> VerifyEmailValidationCode(EmailVerificationCodeDTO verificationCodeDTO)
        {
            try
            {
                //Retrieve Code stored in cache
                var codekey = $"EmailVerification:{verificationCodeDTO.Email}";
                var storedCode = await _redisDatabase.StringGetAsync(codekey);

                //compare stored Code with input Code
                if (storedCode.IsNullOrEmpty || storedCode.ToString().Trim() != verificationCodeDTO.Code.Trim())
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = $"Invalid/Expired Reset Code for {verificationCodeDTO.Email} "//preferably a masked Email
                    };
                }

                //mark as verified and delete Code to prevent Reuse
                var verifiedKey = $"EmailVerified:{verificationCodeDTO.Email}";
                await _redisDatabase.KeyDeleteAsync(codekey);
                //mark key as verified
                await _redisDatabase.StringSetAsync(verifiedKey, "true", TimeSpan.FromMinutes(10));

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Email verified successfully. You can now register."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset code for {Email}", verificationCodeDTO.Email);

                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error during code verification."
                };
            }
        }

        public async Task<Status> ResetPassword(ResetPasswordDto resetPassword)
        {
            try
            {
                var verifiedKey = $"ResetCodeVerified:{resetPassword.Email}";
                var isVerified = await _redisDatabase.StringGetAsync(verifiedKey);

                if (isVerified.IsNullOrEmpty || isVerified.ToString() != "true")
                {
                    return new Status
                    {
                        ResponseCode = 403,
                        ResponseMessage = "Unauthorized or expired verification. Please verify the code first."
                    };
                }

                //check if the user Exists
                var existingUser = await _userManager.FindByEmailAsync(resetPassword.Email);
                if (existingUser == null)
                {
                    return new Status
                    {
                        ResponseCode = 404,
                        ResponseMessage = "User not found."
                    };
                }

                // Use Identity's password reset functionality
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                var result = await _userManager.ResetPasswordAsync(existingUser, resetToken, resetPassword.NewPassword);

                if (!result.Succeeded)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                //remove verification key to invalidate Reset Flow
                await _redisDatabase.KeyDeleteAsync(verifiedKey);

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Password reset successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", resetPassword.Email);

                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error while resetting password."
                };
            }
        }

        #endregion

        #region Enhanced Authentication Methods

        public async Task<AuthResponse> LoginAsync(Minimart_Api.DTOS.Authorization.LoginRequest request, string ipAddress, string userAgent)
        {
            try
            {
                // Check if account is locked due to failed attempts
                if (await IsAccountLockedAsync(request.Email))
                {
                    await RecordLoginAttemptAsync(request.Email, ipAddress, userAgent, false);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Account temporarily locked due to multiple failed login attempts. Please try again later.",
                        Errors = new List<string> { "ACCOUNT_LOCKED" }
                    };
                }

                // Find user by email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    await RecordLoginAttemptAsync(request.Email, ipAddress, userAgent, false);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password",
                        Errors = new List<string> { "INVALID_CREDENTIALS" }
                    };
                }

                // Check if temporary password has expired
                if (user.IsTemporaryPassword && user.TemporaryPasswordExpiry.HasValue &&
                    user.TemporaryPasswordExpiry.Value < DateTime.UtcNow)
                {
                    await RecordLoginAttemptAsync(request.Email, ipAddress, userAgent, false);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Temporary password has expired. Please contact support for a new temporary password.",
                        Errors = new List<string> { "TEMPORARY_PASSWORD_EXPIRED" }
                    };
                }

                // Verify password
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    await RecordLoginAttemptAsync(request.Email, ipAddress, userAgent, false);
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid email or password",
                        Errors = new List<string> { "INVALID_CREDENTIALS" }
                    };
                }

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Record successful login
                await RecordLoginAttemptAsync(request.Email, ipAddress, userAgent, true);

                // Generate tokens
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = await GenerateAccessTokenAsync(user, roles);
                var refreshToken = await GenerateRefreshTokenAsync();

                // Get JWT ID from access token
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var jwtId = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;

                // Save refresh token
                await SaveRefreshTokenAsync(user.Id, refreshToken, jwtId);

                // Get merchant information if user is a merchant (using RBAC)
                var (merchantId, businessName) = await GetMerchantInfoAsync(user.Id, roles);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Data = new AuthData
                    {
                        User = new UserResponse
                        {
                            Id = user.Id,
                            Email = user.Email ?? string.Empty,
                            MerchantId = merchantId,
                            BusinessName = businessName,
                            IsTemporaryPassword = user.IsTemporaryPassword,
                            Role = roles.FirstOrDefault() ?? "User",
                            EmailVerified = user.EmailConfirmed
                        },
                        Token = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresIn = _jwtSettings.ExpirationInMinutes * 60, // Convert to seconds
                        RequiresPasswordReset = user.IsTemporaryPassword
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for email {Email}", request.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login",
                    Errors = new List<string> { "INTERNAL_ERROR" }
                };
            }
        }

        public async Task<AuthResponse> ResetPasswordAsync(string userId, Minimart_Api.DTOS.Authorization.ResetPasswordRequest request)
        {
            try
            {
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Passwords do not match",
                        Errors = new List<string> { "PASSWORD_MISMATCH" }
                    };
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "User not found",
                        Errors = new List<string> { "USER_NOT_FOUND" }
                    };
                }

                // Verify current password
                var currentPasswordValid = await _userManager.CheckPasswordAsync(user, request.CurrentPassword);
                if (!currentPasswordValid)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Current password is incorrect",
                        Errors = new List<string> { "INVALID_CURRENT_PASSWORD" }
                    };
                }

                // Change password
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Failed to update password",
                        Errors = result.Errors.Select(e => e.Description).ToList()
                    };
                }

                // Update temporary password flag
                user.IsTemporaryPassword = false;
                user.TemporaryPasswordExpiry = null;
                await _userManager.UpdateAsync(user);

                // Revoke all existing refresh tokens for security
                await RevokeAllUserRefreshTokensAsync(user.Id);

                // Generate new tokens
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = await GenerateAccessTokenAsync(user, roles);
                var refreshToken = await GenerateRefreshTokenAsync();

                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
                var jwtId = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;

                await SaveRefreshTokenAsync(user.Id, refreshToken, jwtId);

                // Get merchant information if user is a merchant (using RBAC)
                var (merchantId, businessName) = await GetMerchantInfoAsync(user.Id, roles);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Password updated successfully",
                    Data = new AuthData
                    {
                        User = new UserResponse
                        {
                            Id = user.Id,
                            Email = user.Email ?? string.Empty,
                            MerchantId = merchantId,
                            BusinessName = businessName,
                            IsTemporaryPassword = false,
                            Role = roles.FirstOrDefault() ?? "User",
                            EmailVerified = user.EmailConfirmed
                        },
                        Token = accessToken,
                        RefreshToken = refreshToken,
                        ExpiresIn = _jwtSettings.ExpirationInMinutes * 60,
                        RequiresPasswordReset = false
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed for user {UserId}", userId);
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while resetting password",
                    Errors = new List<string> { "INTERNAL_ERROR" }
                };
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
        {
            try
            {
                // Validate the expired token
                var principal = GetPrincipalFromExpiredToken(token);
                if (principal == null)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid token",
                        Errors = new List<string> { "INVALID_TOKEN" }
                    };
                }

                // Get stored refresh token
                var storedRefreshToken = await GetStoredRefreshTokenAsync(refreshToken);
                if (storedRefreshToken == null || storedRefreshToken.Used || storedRefreshToken.IsRevoked ||
                    storedRefreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid or expired refresh token",
                        Errors = new List<string> { "INVALID_REFRESH_TOKEN" }
                    };
                }

                // Verify the JWT ID matches
                var jti = principal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value;
                if (storedRefreshToken.JwtId != jti)
                {
                    return new AuthResponse
                    {
                        Success = false,
                        Message = "Token mismatch",
                        Errors = new List<string> { "TOKEN_MISMATCH" }
                    };
                }

                // Mark the refresh token as used
                storedRefreshToken.Used = true;
                await _context.SaveChangesAsync();

                // Generate new tokens
                var user = storedRefreshToken.User;
                var roles = await _userManager.GetRolesAsync(user);
                var newAccessToken = await GenerateAccessTokenAsync(user, roles);
                var newRefreshToken = await GenerateRefreshTokenAsync();

                var newJwtToken = new JwtSecurityTokenHandler().ReadJwtToken(newAccessToken);
                var newJwtId = newJwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;

                await SaveRefreshTokenAsync(user.Id, newRefreshToken, newJwtId);

                // Get merchant information if user is a merchant (using RBAC)
                var (merchantId, businessName) = await GetMerchantInfoAsync(user.Id, roles);

                return new AuthResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Data = new AuthData
                    {
                        User = new UserResponse
                        {
                            Id = user.Id,
                            Email = user.Email ?? string.Empty,
                            MerchantId = merchantId,
                            BusinessName = businessName,
                            IsTemporaryPassword = user.IsTemporaryPassword,
                            Role = roles.FirstOrDefault() ?? "User",
                            EmailVerified = user.EmailConfirmed
                        },
                        Token = newAccessToken,
                        RefreshToken = newRefreshToken,
                        ExpiresIn = _jwtSettings.ExpirationInMinutes * 60,
                        RequiresPasswordReset = user.IsTemporaryPassword
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred while refreshing token",
                    Errors = new List<string> { "INTERNAL_ERROR" }
                };
            }
        }

        public async Task<bool> LogoutAsync(string userId, string refreshToken)
        {
            try
            {
                await RevokeRefreshTokenAsync(refreshToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> IsAccountLockedAsync(string email)
        {
            var recentAttempts = await _context.UserLoginAttempts
                .Where(a => a.Email == email &&
                           a.AttemptDate > DateTime.UtcNow.AddMinutes(-15) &&
                           !a.Success)
                .CountAsync();

            return recentAttempts >= 5; // Lock after 5 failed attempts in 15 minutes
        }

        public async Task RecordLoginAttemptAsync(string email, string ipAddress, string userAgent, bool success)
        {
            try
            {
                var attempt = new UserLoginAttempt
                {
                    Email = email,
                    IpAddress = ipAddress,
                    AttemptDate = DateTime.UtcNow,
                    Success = success,
                    UserAgent = userAgent
                };

                _context.UserLoginAttempts.Add(attempt);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record login attempt for {Email}", email);
            }
        }

        #endregion

        #region Helper Methods

        // Helper method to extract first name from full name
        private static string ExtractFirstName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 0 ? parts[0] : fullName;
        }

        // Helper method to extract last name from full name  
        private static string ExtractLastName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;
            var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : string.Empty;
        }

        /// <summary>
        /// Gets merchant information for a user if they have the Merchant role.
        /// This is the proper RBAC approach instead of storing MerchantId in ApplicationUser.
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <param name="roles">The user's roles</param>
        /// <returns>A tuple containing (MerchantId, BusinessName) or (null, empty) if not a merchant</returns>
        private async Task<(Guid? MerchantId, string BusinessName)> GetMerchantInfoAsync(string userId, IList<string> roles)
        {
            if (!roles.Contains("Merchant"))
            {
                return (null, string.Empty);
            }

            try
            {
                var merchant = await _context.Merchants
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ApplicationUserId == userId);

                if (merchant != null)
                {
                    return (merchant.MerchantID, merchant.BusinessName ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not retrieve merchant information for user {UserId}", userId);
            }

            return (null, string.Empty);
        }

        public string GenerateRefreshToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var randomBytes = new byte[32];
                rng.GetBytes(randomBytes);
                return Convert.ToBase64String(randomBytes);
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash, byte[] storedSalt)
        {
            var hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: storedSalt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
                ));

            return hashed == storedHash;
        }

        public string HashPassword(string password, out byte[] salt)
        {
            salt = new byte[128 / 8];

            try
            {
                //Generate a random Number and get bytes
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(salt);
                }

                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                    numBytesRequested: 256 / 8
                    ));

                return hashed;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region JWT and Token Management

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user, IList<string> roles)
        {
            var jwtId = Guid.NewGuid().ToString();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, user.UserName ?? string.Empty),
                new("is_temporary_password", user.IsTemporaryPassword.ToString()),
                new(JwtRegisteredClaimNames.Jti, jwtId),
                new(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64)
            };

            // Add merchant_id claim if user is a merchant (using RBAC)
            var (merchantId, _) = await GetMerchantInfoAsync(user.Id, roles);
            claims.Add(new("merchant_id", merchantId?.ToString() ?? string.Empty));


            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateLifetime = false // We don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        public async Task<bool> SaveRefreshTokenAsync(string userId, string refreshToken, string jwtId)
        {
            try
            {
                var storedRefreshToken = new RefreshToken
                {
                    Token = refreshToken,
                    JwtId = jwtId,
                    ApplicationUserId = userId,
                    CreationDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
                };

                _context.RefreshTokens.Add(storedRefreshToken);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving refresh token for user {UserId}", userId);
                return false;
            }
        }

        public async Task<RefreshToken?> GetStoredRefreshTokenAsync(string refreshToken)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = await GetStoredRefreshTokenAsync(refreshToken);
                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return false;
            }
        }

        public async Task<bool> RevokeAllUserRefreshTokensAsync(string userId)
        {
            try
            {
                var userTokens = await _context.RefreshTokens
                    .Where(rt => rt.ApplicationUserId == userId && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var token in userTokens)
                {
                    token.IsRevoked = true;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all refresh tokens for user {UserId}", userId);
                return false;
            }
        }

        #endregion
    }
}
