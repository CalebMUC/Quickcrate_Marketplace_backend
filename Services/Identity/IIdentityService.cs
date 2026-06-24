using Microsoft.AspNetCore.Identity;
using Minimart_Api.Models;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.General;

namespace Minimart_Api.Services.Identity
{
    public interface IIdentityService
    {
        Task<RegisterResponse> RegisterUserAsync(Register model);
        Task<LoginResponse> LoginUserAsync(UserLogin model);
        Task<Status> SendEmailVerificationCodeAsync(string email);
        Task<Status> VerifyEmailAsync(EmailVerificationCodeDTO model);
        Task<Status> SendPasswordResetCodeAsync(PasswordResetDto model);
        Task<Status> VerifyResetCodeAsync(VerifyResetCodeDto model);
        Task<Status> ResetPasswordAsync(ResetPasswordDto model);
        Task<Status> ChangePasswordAsync(ChangePasswordDto model, string userId);
        Task<Models.ApplicationUser?> GetUserByIdAsync(string userId);
        Task<Models.ApplicationUser?> GetUserByEmailAsync(string email);
        //Task<bool> MigrateExistingUserAsync(Users legacyUser);
    }
}