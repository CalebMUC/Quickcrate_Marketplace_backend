using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minimart_Api.Models;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.General;
using Minimart_Api.Services.EmailServices;
using Minimart_Api.Data;
using StackExchange.Redis;
using Authentication_and_Authorization_Api.Core;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Cryptography;

namespace Minimart_Api.Services.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<Models.ApplicationUser> _userManager;
        private readonly SignInManager<Models.ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly MinimartDBContext _context;
        private readonly BrevoEmailService _emailService;
        private readonly IDatabase _redisDatabase;
        private readonly CoreLibraries _coreLibraries;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentityService> _logger;

        public IdentityService(
            UserManager<Models.ApplicationUser> userManager,
            SignInManager<Models.ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            MinimartDBContext context,
            BrevoEmailService emailService,
            IDatabase redisDatabase,
            CoreLibraries coreLibraries,
            IConfiguration configuration,
            ILogger<IdentityService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _emailService = emailService;
            _redisDatabase = redisDatabase;
            _coreLibraries = coreLibraries;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RegisterResponse> RegisterUserAsync(Register model)
        {
            try
            {
                // Validate role input - only allow specific roles for registration
                var allowedRoles = new[] { "User", "Admin" }; // Note: Merchant registration should use separate endpoint
                var roleToAssign = string.IsNullOrWhiteSpace(model.Role) ? "User" : model.Role.Trim();
            
                if (!allowedRoles.Contains(roleToAssign, StringComparer.OrdinalIgnoreCase))
                {
                    return new RegisterResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = $"Invalid role '{model.Role}'. Only 'User' and 'Admin' roles are allowed for registration."
                    };
                }

                // Check if user already exists - Use no-tracking to avoid MerchantID issues
                var existingUserCheck = await _userManager.Users
                    .AsNoTracking()
                    .Select(u => new { u.Email, u.Id })
                    .FirstOrDefaultAsync(u => u.Email == model.Email);
            
                if (existingUserCheck != null)
                {
                    return new RegisterResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = "User with the provided email already exists"
                    };
                }

                // Check phone number uniqueness - Use no-tracking to avoid MerchantID issues
                var existingPhoneUser = await _userManager.Users
                    .AsNoTracking()
                    .Select(u => new { u.PhoneNumber, u.Id })
                    .FirstOrDefaultAsync(u => u.PhoneNumber == model.PhoneNumber);
            
                if (existingPhoneUser != null)
                {
                    return new RegisterResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = "User with the provided phone number already exists"
                    };
                }

            // Create new user with ONLY essential Identity properties
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            // Log before creation for debugging
            _logger.LogInformation("Attempting to create user with email: {Email}, UserName: {UserName}", 
                    model.Email, user.UserName);

                // Create user - this should auto-generate the Id
                var result = await _userManager.CreateAsync(user, model.password);
            
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    _logger.LogError("User creation failed for {Email}. Errors: {Errors}", model.Email, errors);
                
                    return new RegisterResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                // Verify that Id was generated
                if (string.IsNullOrEmpty(user.Id))
                {
                    _logger.LogError("User created but Id is still null for {Email}", model.Email);
                    return new RegisterResponse
                    {
                        ResponseCode = 500,
                        ResponseMessage = "User creation succeeded but ID generation failed"
                    };
                }

                _logger.LogInformation("User created successfully with Id: {UserId}", user.Id);

                // Now update with custom properties AFTER creation
                user.DisplayName = model.UserName;
                user.FirstName = ExtractFirstName(model.UserName);
                user.LastName = ExtractLastName(model.UserName);
                user.CreatedAt = DateTime.UtcNow;
                user.IsLoggedIn = false;
                user.IsEmailVerified = true;
                user.EmailConfirmed = true;
                user.FailedAttempts = 0;
                user.IsActive = true;
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
                var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);
            
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to assign role {Role} to user {UserId}. Errors: {Errors}",
                        normalizedRole, user.Id, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                // Log the registration for audit purposes
                _logger.LogInformation("New {Role} registered: {Email} with UserID: {UserId}", 
                    normalizedRole, model.Email, user.Id);

                return new RegisterResponse
                {
                    ResponseCode = 200,
                    ResponseMessage = $"You have successfully created an account as {normalizedRole}. Welcome to Minimart!",
                    UserId = user.Id,
                    Username = user.DisplayName,
                    Role = normalizedRole,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during user registration for email {Email} with role {Role}. Stack trace: {StackTrace}", 
                    model.Email, model.Role, ex.StackTrace);
                return new RegisterResponse
                {
                    ResponseCode = 500,
                    ResponseMessage = "An error occurred during registration"
                };
            }
        }

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

        public async Task<LoginResponse> LoginUserAsync(UserLogin model)
        {
            try
            {
                // Validate input format
                var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                var phoneRegex = @"^\+?[0-9]{10,15}$";

                bool isEmail = Regex.IsMatch(model.EmailorPhone, emailRegex);
                bool isPhoneNumber = Regex.IsMatch(model.EmailorPhone, phoneRegex);

                if (!isEmail && !isPhoneNumber)
                {
                    return new LoginResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = "Invalid email or phone number format"
                    };
                }

                // Find user - Use no-tracking queries to avoid MerchantID issues
                Models.ApplicationUser? user = null;
                if (isEmail)
                {
                    // First find the user ID with no-tracking
                    var userCheck = await _userManager.Users
                        .AsNoTracking()
                        .Select(u => new { u.Email, u.Id })
                        .FirstOrDefaultAsync(u => u.Email == model.EmailorPhone);
                    
                    if (userCheck != null)
                    {
                        // Then get the full user object for authentication
                        user = await _userManager.FindByIdAsync(userCheck.Id);
                    }
                }
                else
                {
                    // Use no-tracking query for phone number lookup
                    var phoneUserCheck = await _userManager.Users
                        .AsNoTracking()
                        .Select(u => new { u.PhoneNumber, u.Id })
                        .FirstOrDefaultAsync(u => u.PhoneNumber == model.EmailorPhone);
                    
                    if (phoneUserCheck != null)
                    {
                        // Get the full user object for authentication
                        user = await _userManager.FindByIdAsync(phoneUserCheck.Id);
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

                // Check password
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded)
                {
                    return new LoginResponse
                    {
                        ResponseCode = 401,
                        ResponseMessage = "Invalid Password"
                    };
                }

                // Get user roles FIRST
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "User";

                // FIXED: Use GenerateAccessTokenAsync with roles instead of legacy CoreLibraries.GenerateToken
                var token = await GenerateAccessTokenAsync(user, roles);
                var refreshToken = GenerateRefreshToken();

                // Update user login status and last login time
                user.IsLoggedIn = true;
                user.LastLogin = DateTime.UtcNow;
                user.FailedAttempts = 0; // Reset failed attempts on successful login
                await _userManager.UpdateAsync(user);

                return new LoginResponse
                {
                    Username = user.DisplayName ?? user.UserName ?? user.Email,
                    UserId = user.Id,
                    UserRole = userRole,
                    Token = token,
                    Refreshtoken = refreshToken,
                    ResponseCode = 200,
                    ResponseMessage = "Login Successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {EmailOrPhone}", model.EmailorPhone);
                return new LoginResponse
                {
                    ResponseCode = 500,
                    ResponseMessage = "An unexpected error occurred"
                };
            }
        }

        public async Task<Status> SendEmailVerificationCodeAsync(string email)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "User with the provided email already exists"
                    };
                }

                // Rate limiting logic (same as before)
                string rateLimitKey = $"EmailRateLimit:{email}";
                var attemptCount = await _redisDatabase.StringGetAsync(rateLimitKey);

                if (attemptCount.HasValue && int.Parse(attemptCount) >= 3)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "Too many password resets. Please try again later"
                    };
                }

                // Generate and store verification code
                var code = new Random().Next(100000, 999999).ToString();
                var verificationKey = $"EmailVerification:{email}";

                await _redisDatabase.StringSetAsync(verificationKey, code, TimeSpan.FromMinutes(10));

                // Update rate limit
                if (!attemptCount.HasValue)
                {
                    await _redisDatabase.StringSetAsync(rateLimitKey, "1", TimeSpan.FromMinutes(10));
                }
                else
                {
                    await _redisDatabase.StringIncrementAsync(rateLimitKey);
                }

                await _emailService.SendAsync(email, "Email Verification Code", $"Your email verification code is {code}");

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Email validation code sent successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verification code to {Email}", email);
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Error sending verification code"
                };
            }
        }

        public async Task<Status> VerifyEmailAsync(EmailVerificationCodeDTO model)
        {
            try
            {
                var codeKey = $"EmailVerification:{model.Email}";
                var storedCode = await _redisDatabase.StringGetAsync(codeKey);

                if (storedCode.IsNullOrEmpty || storedCode.ToString().Trim() != model.Code.Trim())
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = $"Invalid/Expired verification code for {model.Email}"
                    };
                }

                // Mark as verified and clean up
                var verifiedKey = $"EmailVerified:{model.Email}";
                await _redisDatabase.KeyDeleteAsync(codeKey);
                await _redisDatabase.StringSetAsync(verifiedKey, "true", TimeSpan.FromMinutes(10));

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Email verified successfully. You can now register."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email for {Email}", model.Email);
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error during verification"
                };
            }
        }

        public async Task<Status> SendPasswordResetCodeAsync(PasswordResetDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "User with the provided email doesn't exist"
                    };
                }

                // Rate limiting and code generation logic (same as before)
                string rateLimitKey = $"PasswordResetLimit:{model.Email}";
                var attemptCount = await _redisDatabase.StringGetAsync(rateLimitKey);

                if (attemptCount.HasValue && int.Parse(attemptCount) >= 3)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "Too many password resets. Please try again later"
                    };
                }

                var code = new Random().Next(100000, 999999).ToString();
                var verificationKey = $"ResetCodeVerification:{model.Email}";

                await _redisDatabase.StringSetAsync(verificationKey, code, TimeSpan.FromMinutes(10));

                // Update rate limit
                if (!attemptCount.HasValue)
                {
                    await _redisDatabase.StringSetAsync(rateLimitKey, "1", TimeSpan.FromMinutes(10));
                }
                else
                {
                    await _redisDatabase.StringIncrementAsync(rateLimitKey);
                }

                await _emailService.SendAsync(model.Email, "Password Reset Code", $"Your password reset code is {code}");

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Password reset code sent successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reset code to {Email}", model.Email);
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Error sending reset code"
                };
            }
        }

        public async Task<Status> VerifyResetCodeAsync(VerifyResetCodeDto model)
        {
            try
            {
                var codeKey = $"ResetCodeVerification:{model.Email}";
                var storedCode = await _redisDatabase.StringGetAsync(codeKey);

                if (storedCode.IsNullOrEmpty || storedCode.ToString().Trim() != model.Code.Trim())
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = $"Invalid/Expired reset code for {model.Email}"
                    };
                }

                var verifiedKey = $"ResetCodeVerified:{model.Email}";
                await _redisDatabase.KeyDeleteAsync(codeKey);
                await _redisDatabase.StringSetAsync(verifiedKey, "true", TimeSpan.FromMinutes(10));

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Code verified. Proceed to reset your password"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset code for {Email}", model.Email);
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error during code verification"
                };
            }
        }

        public async Task<Status> ResetPasswordAsync(ResetPasswordDto model)
        {
            try
            {
                var verifiedKey = $"ResetCodeVerified:{model.Email}";
                var isVerified = await _redisDatabase.StringGetAsync(verifiedKey);

                if (isVerified.IsNullOrEmpty || isVerified.ToString() != "true")
                {
                    return new Status
                    {
                        ResponseCode = 403,
                        ResponseMessage = "Unauthorized or expired verification. Please verify the code first."
                    };
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return new Status
                    {
                        ResponseCode = 404,
                        ResponseMessage = "User not found"
                    };
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

                if (!result.Succeeded)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                // Update password-related fields
                user.IsTemporaryPassword = false; // User has set their own password
                user.PasswordChangesOn = DateTime.UtcNow;
                user.FailedAttempts = 0;
                await _userManager.UpdateAsync(user);

                // Clean up verification key
                await _redisDatabase.KeyDeleteAsync(verifiedKey);

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Password reset successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", model.Email);
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error while resetting password"
                };
            }
        }

        public async Task<Status> ChangePasswordAsync(ChangePasswordDto model, string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return new Status
                    {
                        ResponseCode = 404,
                        ResponseMessage = "User not found"
                    };
                }

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = string.Join(", ", result.Errors.Select(e => e.Description))
                    };
                }

                // Update password-related fields
                user.IsTemporaryPassword = false; // User has set their own password
                user.PasswordChangesOn = DateTime.UtcNow;
                user.FailedAttempts = 0;
                await _userManager.UpdateAsync(user);

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Password changed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error while changing password"
                };
            }
        }

        public async Task<Models.ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<Models.ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        #region JWT Token Generation Methods - IMPLEMENTATION ADDED

        /// <summary>
        /// Generate JWT Access Token with role and merchant claims
        /// </summary>
        public async Task<string> GenerateAccessTokenAsync(Models.ApplicationUser user, IList<string> roles)
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

            // Add role claims - THIS IS THE KEY FIX
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Gets merchant information for a user if they have the Merchant role.
        /// This is the proper RBAC approach instead of storing MerchantId in ApplicationUser.
        /// </summary>
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

        /// <summary>
        /// Generate secure refresh token (UPDATED - more secure)
        /// </summary>
        private new string GenerateRefreshToken()  // 'new' keyword to hide the existing method
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[32];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        #endregion
    }
}