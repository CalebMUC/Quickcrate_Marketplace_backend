namespace Minimart_Api.Services.EmailServices
{
    public interface IEmailService
    {
        Task<bool> SendMerchantWelcomeEmailAsync(string email, string businessName,
      string username, string temporaryPassword, string dashboardUrl);
    }
}
