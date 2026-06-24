using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Minimart_Api.Services.Identity;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.General;
using Minimart_Api.Models;
using Minimart_Api.Services;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;

namespace Minimart_Api.Controllers
{
    /// <summary>
    /// Unified Authentication Controller for the entire Minimart system
    /// Handles user registration, authentication, password management, and profile operations
    /// </summary>
    [ApiController]
    [Route("api/v2/[controller]")]
    [Route("api/[controller]")] // Keep backward compatibility
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly IAuthentication _authentication;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<IdentityController> _logger;
        private readonly MinimartDBContext _context;

        public IdentityController(
            IIdentityService identityService,
            IAuthentication authentication,
            UserManager<ApplicationUser> userManager,
            ILogger<IdentityController> logger,
            MinimartDBContext context)
        {
            _identityService = identityService;
            _authentication = authentication;
            _userManager = userManager;
            _logger = logger;
            _context = context;
        }

        #region Registration and Authentication

        /// <summary>
        /// Register a new user in the system
        /// </summary>
        /// <param name="model">Registration data</param>
        /// <returns>Registration response</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] Register model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new RegisterResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage))
                    });
                }

                var result = await _identityService.RegisterUserAsync(model);
                
                if (result.ResponseCode == 200)
                {
                    return Ok(result);
                }
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new RegisterResponse
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error during registration"
                });
            }
        }

        /// <summary>
        /// Authenticate user with email/username and password
        /// </summary>
        /// <param name="model">Login credentials</param>
        /// <returns>Login response with tokens</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] UserLogin model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponse
                    {
                        ResponseCode = 400,
                        ResponseMessage = string.Join(", ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage))
                    });
                }

                var result = await _identityService.LoginUserAsync(model);
                
                if (result.ResponseCode == 200)
                {
                    return Ok(result);
                }
                
                return Unauthorized(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new LoginResponse
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error during login"
                });
            }
        }

        /// <summary>
        /// Enhanced login for merchant system with IP tracking
        /// </summary>
        /// <param name="request">Enhanced login request</param>
        /// <returns>Enhanced authentication response</returns>
        [HttpPost("merchant/login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> MerchantLogin([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var userAgent = Request.Headers["User-Agent"].ToString();

                // First check if account is locked
                var isLocked = await _authentication.IsAccountLockedAsync(request.Email);
                if (isLocked)
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Account is temporarily locked due to multiple failed login attempts",
                        Errors = new List<string> { "ACCOUNT_LOCKED" }
                    });
                }

                var response = await _authentication.LoginAsync(request, ipAddress, userAgent);

                // Record login attempt
                await _authentication.RecordLoginAttemptAsync(request.Email, ipAddress, userAgent, response.Success);

                if (response.Success)
                {
                    return Ok(response);
                }

                return Unauthorized(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during merchant login");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "INTERNAL_ERROR" }
                });
            }
        }

        #endregion

        #region Password Management

        /// <summary>
        /// Send password reset code to user's email
        /// </summary>
        /// <param name="model">Email for password reset</param>
        /// <returns>Status response</returns>
        [HttpPost("send-password-reset")]
        [AllowAnonymous]
        public async Task<ActionResult<Status>> SendPasswordReset([FromBody] PasswordResetDto model)
        {
            try
            {
                var result = await _identityService.SendPasswordResetCodeAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset code");
                return StatusCode(500, new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Verify password reset code
        /// </summary>
        /// <param name="model">Reset code verification data</param>
        /// <returns>Status response</returns>
        [HttpPost("verify-reset-code")]
        [AllowAnonymous]
        public async Task<ActionResult<Status>> VerifyResetCode([FromBody] VerifyResetCodeDto model)
        {
            try
            {
                var result = await _identityService.VerifyResetCodeAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying reset code");
                return StatusCode(500, new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Reset password using verified code
        /// </summary>
        /// <param name="model">Password reset data</param>
        /// <returns>Status response</returns>
        //[HttpPost("reset-password")]
        //[AllowAnonymous]
        //public async Task<ActionResult<Status>> ResetPassword([FromBody] ResetPasswordDto model)
        //{
        //    try
        //    {
        //        var result = await _identityService.ResetPasswordAsync(model);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error resetting password");
        //        return StatusCode(500, new Status
        //        {
        //            ResponseCode = 500,
        //            ResponseMessage = "Internal server error"
        //        });
        //    }
        //}

        /// <summary>
        /// Change password for authenticated user (from temporary password)
        /// </summary>
        /// <param name="request">Password change request</param>
        /// <returns>Authentication response</returns>
        [HttpPost("reset-password")]
        [Authorize]
        public async Task<ActionResult<AuthResponse>> ChangePasswordFromTemp([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid user context",
                        Errors = new List<string> { "INVALID_USER_CONTEXT" }
                    });
                }

                var response = await _authentication.ResetPasswordAsync(userId, request);

                if (response.Success)
                {
                    return Ok(response);
                }

                return BadRequest(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "INTERNAL_ERROR" }
                });
            }
        }

        /// <summary>
        /// Change password for authenticated user (standard change)
        /// </summary>
        /// <param name="model">Change password request</param>
        /// <returns>Status response</returns>
        [HttpPost("update-password")]
        [Authorize]
        public async Task<ActionResult<Status>> ChangePassword([FromBody] ChangePasswordDto model)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value 
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new Status
                    {
                        ResponseCode = 401,
                        ResponseMessage = "Invalid token"
                    });
                }

                var result = await _identityService.ChangePasswordAsync(model, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error"
                });
            }
        }

        #endregion

        #region Email Verification

        /// <summary>
        /// Send email verification code
        /// </summary>
        /// <param name="model">Email verification request</param>
        /// <returns>Status response</returns>
        [HttpPost("send-email-verification")]
        [AllowAnonymous]
        public async Task<ActionResult<Status>> SendEmailVerification([FromBody] EmailVerificationDto model)
        {
            try
            {
                var result = await _identityService.SendEmailVerificationCodeAsync(model.Email);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email verification");
                return StatusCode(500, new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Verify email with verification code
        /// </summary>
        /// <param name="model">Email verification code data</param>
        /// <returns>Status response</returns>
        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<ActionResult<Status>> VerifyEmail([FromBody] EmailVerificationCodeDTO model)
        {
            try
            {
                var result = await _identityService.VerifyEmailAsync(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email");
                return StatusCode(500, new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error"
                });
            }
        }

        #endregion

        #region Token Management

        /// <summary>
        /// Refresh expired access token
        /// </summary>
        /// <param name="request">Token refresh request</param>
        /// <returns>New authentication tokens</returns>
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Handle both legacy and new token refresh formats
                string token = request.UserID ?? ""; // Legacy support
                string refreshToken = ""; // Need to extract from cookie or request

                var response = await _authentication.RefreshTokenAsync(token, refreshToken);

                if (response.Success)
                {
                    return Ok(response);
                }

                return Unauthorized(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "INTERNAL_ERROR" }
                });
            }
        }

        /// <summary>
        /// Logout user and revoke tokens
        /// </summary>
        /// <param name="request">Logout request</param>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout([FromBody] LogoutRequest? request = null)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value 
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                // Try enhanced logout first (for merchant system)
                try
                {
                    var success = await _authentication.LogoutAsync(userId, request?.RefreshToken ?? "");
                    if (success)
                    {
                        return Ok(new { success = true, message = "Logged out successfully" });
                    }
                }
                catch
                {
                    // Fallback to basic logout
                }

                // Fallback to basic logout (for regular system)
                var user = await _identityService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    user.IsLoggedIn = false;
                    await _userManager.UpdateAsync(user);
                }

                return Ok(new
                {
                    success = true,
                    message = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        #endregion

        #region User Profile Management

        /// <summary>
        /// Get current user profile information
        /// </summary>
        /// <returns>User profile data</returns>
        [HttpGet("profile")]
        [HttpGet("me")] // Alternative endpoint name
        [Authorize]
        public async Task<ActionResult<UserResponse>> GetProfile()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value 
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid token");
                }

                var user = await _identityService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "User";

                // Get merchant information if user is a merchant (using RBAC)
                Guid? merchantId = null;
                string businessName = "";
                
                if (roles.Contains("Merchant"))
                {
                    try
                    {
                        var merchant = await _context.Merchants
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);
                        
                        if (merchant != null)
                        {
                            merchantId = merchant.MerchantID;
                            businessName = merchant.BusinessName ?? string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not retrieve merchant information for user {UserId}", user.Id);
                    }
                }

                var userResponse = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    MerchantId = merchantId,
                    BusinessName = businessName,
                    IsTemporaryPassword = user.IsTemporaryPassword,
                    Role = userRole,
                    EmailVerified = user.EmailConfirmed
                };

                // Return in legacy format for compatibility
                return Ok(new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    DisplayName = user.DisplayName ?? $"{user.FirstName} {user.LastName}".Trim() ?? user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsEmailVerified = user.IsEmailVerified,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin,
                    IsLoggedIn = user.IsLoggedIn,
                    IsTemporaryPassword = user.IsTemporaryPassword,
                    PasswordChangesOn = user.PasswordChangesOn,
                    Role = userRole,
                    Roles = roles.ToList(),
                    RequiresPasswordChange = user.IsTemporaryPassword,
                    MerchantId = merchantId, // Now properly retrieved from Merchants table
                    BusinessName = businessName,
                    // Enhanced response data
                    data = userResponse
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        /// <param name="model">Profile update data</param>
        /// <returns>Update status</returns>
        [HttpPut("profile")]
        [HttpPut("update-profile")] // Alternative endpoint name
        [Authorize]
        public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value 
                          ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Invalid token");
                }

                var user = await _identityService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Update user properties
                user.FirstName = model.FirstName ?? user.FirstName;
                user.LastName = model.LastName ?? user.LastName;
                user.DisplayName = model.DisplayName ?? user.DisplayName;
                user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new { 
                        ResponseCode = 400,
                        ResponseMessage = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                return Ok(new {
                    ResponseCode = 200,
                    ResponseMessage = "Profile updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region Admin Endpoints

        /// <summary>
        /// Get all users (Admin only)
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>Paginated list of users</returns>
        [HttpGet("users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = _userManager.Users
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        DisplayName = u.DisplayName,
                        Email = u.Email,
                        PhoneNumber = u.PhoneNumber,
                        CreatedAt = u.CreatedAt,
                        LastLogin = u.LastLogin,
                        IsLoggedIn = u.IsLoggedIn,
                        EmailConfirmed = u.EmailConfirmed,
                        //MerchantId = u.MerchantID,
                        IsTemporaryPassword = u.IsTemporaryPassword
                    })
                    .ToList();

                var totalUsers = _userManager.Users.Count();

                return Ok(new
                {
                    Users = users,
                    TotalCount = totalUsers,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                return StatusCode(500, "Internal server error");
            }
        }

        #endregion

        #region Role-based Test Endpoints

        [HttpGet("admin-only")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("This is only accessible by Admins");
        }

        [HttpGet("merchant-only")]
        [Authorize(Policy = "MerchantOnly")]
        public IActionResult MerchantOnlyEndpoint()
        {
            return Ok("This is only accessible by Merchants");
        }

        [HttpGet("user-only")]
        [Authorize(Policy = "UserOnly")]
        public IActionResult UserOnlyEndpoint()
        {
            return Ok("This is only accessible by Users");
        }

        #endregion

       

      

        /// <summary>
        /// Legacy send reset code endpoint
        /// </summary>
        //[HttpPost("SendResetCode")]
        //[AllowAnonymous]
        //public async Task<IActionResult> SendResetCode([FromBody] PasswordResetDto passwordReset)
        //{
        //    var result = await SendPasswordReset(passwordReset);
        //    return result;
        //}

        ///// <summary>
        ///// Legacy verify email validation code
        ///// </summary>
        //[HttpPost("VerifyEmailValidationCode")]
        //[AllowAnonymous]
        //public async Task<IActionResult> VerifyEmailValidationCode([FromBody] EmailVerificationCodeDTO verificationCodeDTO)
        //{
        //    var result = await VerifyEmail(verificationCodeDTO);
        //    return result;
        //}

        ///// <summary>
        ///// Legacy verify reset code
        ///// </summary>
        //[HttpPost("VerifyResetCode")]
        //[AllowAnonymous]
        //public async Task<IActionResult> VerifyResetCodeLegacy([FromBody] VerifyResetCodeDto verifyResetCode)
        //{
        //    var result = await VerifyResetCode(verifyResetCode);
        //    return result;
        //}

        ///// <summary>
        ///// Legacy reset password endpoint
        ///// </summary>
        //[HttpPost("ResetPassword")]
        //[AllowAnonymous]
        //public async Task<IActionResult> ResetPasswordLegacy([FromBody] ResetPasswordDto resetPassword)
        //{
        //    var result = await ResetPassword(resetPassword);
        //    return result;
        //}

        ///// <summary>
        ///// Legacy send email verification
        ///// </summary>
        //[HttpPost("SendEmailVerificationCode")]
        //[AllowAnonymous]
        //public async Task<IActionResult> SendEmailVerificationCode([FromBody] EmailVerificationDto emailVerification)
        //{
        //    var result = await SendEmailVerification(emailVerification);
        //    return result;
        //}

        //#endregion
    }
}