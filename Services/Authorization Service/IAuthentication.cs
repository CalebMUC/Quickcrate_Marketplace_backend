using Microsoft.AspNetCore.Identity.Data;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.General;
using Minimart_Api.Models;

namespace Minimart_Api.Services
{
    public interface IAuthentication
    {
        public Task<RegisterResponse> Register(Register register);
        public Task<LoginResponse> Login(UserLogin login);

        public Task<Status> SendResetCode(PasswordResetDto passwordReset);
        public Task<Status> VerifyResetCode(VerifyResetCodeDto verifyResetCode);

        public Task<Status> VerifyEmailValidationCode(EmailVerificationCodeDTO verificationCodeDTO);
        public Task<Status> ResetPassword(ResetPasswordDto resetPassword);

        public Task<Status> SendEmailVerificationCode(string email);

        // Additional methods can be added here as needed

        Task<AuthResponse> LoginAsync(Minimart_Api.DTOS.Authorization.LoginRequest request, string ipAddress, string userAgent);
        Task<AuthResponse> ResetPasswordAsync(string userId, Minimart_Api.DTOS.Authorization.ResetPasswordRequest request);
        Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);
        Task<bool> LogoutAsync(string userId, string refreshToken);
        Task<bool> IsAccountLockedAsync(string email);
        Task RecordLoginAttemptAsync(string email, string ipAddress, string userAgent, bool success);
    }
}
