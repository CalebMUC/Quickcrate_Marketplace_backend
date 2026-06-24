using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Authorization;

namespace Minimart_Api.Controllers
{
    /// <summary>
    /// DEPRECATED: This controller has been superseded by IdentityController
    /// Please use api/identity endpoints instead of api/authentication
    /// This controller is maintained for backward compatibility only
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Obsolete("This controller is deprecated. Please use IdentityController (api/identity) instead.", false)]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/register instead
        /// </summary>
        [HttpPost("Register")]
        [Obsolete("Use api/identity/register instead", false)]
        public IActionResult Register([FromBody] Register register)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/register", "api/authentication/Register");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/register' instead.",
                new_endpoint = "/api/identity/register",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/login instead
        /// </summary>
        [HttpPost("Login")]
        [Obsolete("Use api/identity/login instead", false)]
        public IActionResult Login([FromBody] UserLogin login)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/login", "api/authentication/Login");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/login' instead.",
                new_endpoint = "/api/identity/login",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/send-password-reset instead
        /// </summary>
        [HttpPost("SendResetCode")]
        [Obsolete("Use api/identity/send-password-reset instead", false)]
        public IActionResult SendResetCode([FromBody] PasswordResetDto passwordReset)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/send-password-reset", "api/authentication/SendResetCode");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/send-password-reset' instead.",
                new_endpoint = "/api/identity/send-password-reset",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/verify-email instead
        /// </summary>
        [HttpPost("VerifyEmailValidationCode")]
        [Obsolete("Use api/identity/verify-email instead", false)]
        public IActionResult VerifyEmailValidationCode([FromBody] EmailVerificationCodeDTO verificationCodeDTO)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/verify-email", "api/authentication/VerifyEmailValidationCode");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/verify-email' instead.",
                new_endpoint = "/api/identity/verify-email",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/verify-reset-code instead
        /// </summary>
        [HttpPost("VerifyResetCode")]
        [Obsolete("Use api/identity/verify-reset-code instead", false)]
        public IActionResult VerifyResetCode([FromBody] VerifyResetCodeDto verifyResetCode)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/verify-reset-code", "api/authentication/VerifyResetCode");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/verify-reset-code' instead.",
                new_endpoint = "/api/identity/verify-reset-code",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/reset-password instead
        /// </summary>
        [HttpPost("ResetPassword")]
        [Obsolete("Use api/identity/reset-password instead", false)]
        public IActionResult ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/reset-password", "api/authentication/ResetPassword");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/reset-password' instead.",
                new_endpoint = "/api/identity/reset-password",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/send-email-verification instead
        /// </summary>
        [HttpPost("SendEmailVerificationCode")]
        [Obsolete("Use api/identity/send-email-verification instead", false)]
        public IActionResult SendEmailVerificationCode([FromBody] EmailVerificationDto emailVerification)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/send-email-verification", "api/authentication/SendEmailVerificationCode");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/send-email-verification' instead.",
                new_endpoint = "/api/identity/send-email-verification",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/merchant/login instead
        /// </summary>
        [HttpPost("login")]
        [Obsolete("Use api/identity/merchant/login instead", false)]
        public IActionResult MerchantLogin([FromBody] LoginRequest request)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/merchant/login", "api/authentication/login");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/merchant/login' instead.",
                new_endpoint = "/api/identity/merchant/login",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/change-password instead
        /// </summary>
        [HttpPost("reset-password")]
        [Obsolete("Use api/identity/change-password instead", false)]
        public IActionResult ResetPasswordAuth([FromBody] ResetPasswordRequest request)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/change-password", "api/authentication/reset-password");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/change-password' instead.",
                new_endpoint = "/api/identity/change-password",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/refresh-token instead
        /// </summary>
        [HttpPost("refresh")]
        [Obsolete("Use api/identity/refresh-token instead", false)]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/refresh-token", "api/authentication/refresh");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/refresh-token' instead.",
                new_endpoint = "/api/identity/refresh-token",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/logout instead
        /// </summary>
        [HttpPost("logout")]
        [Obsolete("Use api/identity/logout instead", false)]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/logout", "api/authentication/logout");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/logout' instead.",
                new_endpoint = "/api/identity/logout",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }

        /// <summary>
        /// DEPRECATED: Use api/identity/profile instead
        /// </summary>
        [HttpGet("me")]
        [Obsolete("Use api/identity/profile instead", false)]
        public IActionResult GetCurrentUser()
        {
            _logger.LogWarning("Deprecated endpoint called: {Endpoint}. Please use api/identity/profile", "api/authentication/me");

            return BadRequest(new
            {
                error = "DEPRECATED_ENDPOINT",
                message = "This endpoint has been deprecated. Please use 'api/identity/profile' instead.",
                new_endpoint = "/api/identity/profile",
                documentation = "Please update your client to use the new unified authentication system."
            });
        }
    }
}
