using Microsoft.AspNetCore.Identity.Data;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.General;
using Minimart_Api.Models;
using Minimart_Api.Repositories.Authorization;

namespace Minimart_Api.Services
{
    public class AuthenticationService: IAuthentication
    {
        private readonly IAuthRepository _authRepository;
        public AuthenticationService(IAuthRepository authRepository ) {

            _authRepository = authRepository;
        }

        public async Task<RegisterResponse> Register(Register register) { 

          return  await _authRepository.Register(register);
        }

        public async Task<LoginResponse> Login(UserLogin userLogin)
        {

            return await _authRepository.Login(userLogin);
        }

        public async Task<Status> SendResetCode(PasswordResetDto passwordReset) {

            return await _authRepository.SendResetCode(passwordReset);
        }
        public async Task<Status> VerifyResetCode(VerifyResetCodeDto verifyResetCode) {

            return await _authRepository.VerifyResetCode(verifyResetCode);
        }

        public async Task<Status> VerifyEmailValidationCode(EmailVerificationCodeDTO verificationCodeDTO)
        {

            return await _authRepository.VerifyEmailValidationCode(verificationCodeDTO);
        }
        public async Task<Status> ResetPassword(ResetPasswordDto resetPassword) {

            return await _authRepository.ResetPassword(resetPassword);
        }

        public async Task<Status> SendEmailVerificationCode(string email) {

            return await _authRepository.SendEmailVerificationCode(email);
        }

        // Additional methods can be added here as needed

        public async Task<AuthResponse> LoginAsync(Minimart_Api.DTOS.Authorization.LoginRequest request, string ipAddress, string userAgent) { 

            return await _authRepository.LoginAsync(request, ipAddress, userAgent);
        }
        public async Task<AuthResponse> ResetPasswordAsync(string userId, Minimart_Api.DTOS.Authorization.ResetPasswordRequest request) { 

            return await _authRepository.ResetPasswordAsync(userId, request);
        }
        public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken) { 

            return await _authRepository.RefreshTokenAsync(token, refreshToken);
        }
        public async Task<bool> LogoutAsync(string userId, string refreshToken) { 
            return await _authRepository.LogoutAsync(userId, refreshToken);
        }
        public async Task<bool> IsAccountLockedAsync(string email) { 
            return await _authRepository.IsAccountLockedAsync(email);
        }
        public async Task RecordLoginAttemptAsync(string email, string ipAddress, string userAgent, bool success) { 
            await _authRepository.RecordLoginAttemptAsync(email, ipAddress, userAgent, success);
        }
    }
}
