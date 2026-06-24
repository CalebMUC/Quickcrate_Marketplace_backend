using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Minimart_Api.Services.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendMerchantWelcomeEmailAsync(
            string email,
            string businessName,
            string username,
            string temporaryPassword,
            string dashboardUrl)
        {
            try
            {
                // Build HTML email body
                var emailBody = $@"
                <html>
                <head><title>Welcome to QuickCrate Express Limited - Merchant Account Created</title></head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #2563eb;'>Welcome to QuickCrate Platform!</h2>
                        
                        <p>Dear <strong>{businessName}</strong>,</p>
                        
                        <p>Congratulations! Your merchant application has been approved and your account has been created.</p>
                        
                        <div style='background-color: #f3f4f6; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='margin-top: 0; color: #1f2937;'>Your Login Credentials</h3>
                            <p><strong>Merchant Dashboard:</strong> <a href='{dashboardUrl}'>{dashboardUrl}</a></p>
                            <p><strong>Username:</strong> {username}</p>
                            <p><strong>Temporary Password:</strong> 
                                <code style='background-color: #e5e7eb; padding: 2px 6px; border-radius: 4px;'>{temporaryPassword}</code>
                            </p>
                        </div>

                        <div style='background-color: #fef3c7; padding: 15px; border-radius: 8px; border-left: 4px solid #f59e0b;'>
                            <p style='margin: 0;'><strong>Important Security Notice:</strong></p>
                            <ul style='margin: 10px 0;'>
                                <li>This is a temporary password that expires in 7 days</li>
                                <li>You will be required to change it on your first login</li>
                                <li>Keep this information secure and do not share it</li>
                            </ul>
                        </div>

                        <p>Next steps:</p>
                        <ol>
                            <li>Click the dashboard link above to access your merchant portal</li>
                            <li>Log in using the provided credentials</li>
                            <li>Complete your profile setup and change your password</li>
                            <li>Start managing your products and orders</li>
                        </ol>
                        
                        <p>If you have any questions or need assistance, please contact our support team.</p>
                        
                        <p>Welcome aboard!<br>
                        <strong>QuickCrate Team</strong></p>
                    </div>
                </body>
                </html>";

                // ✅ Brevo SMTP implementation
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                 _configuration["Brevo:FromName"] ?? "QuickCrate Express Limited",
                _configuration["Brevo:FromAddress"]));


                message.To.Add(new MailboxAddress(businessName, email));
                message.Subject = "Welcome to QuickCrate - Your Account is Ready!";
                message.Body = new TextPart(TextFormat.Html) { Text = emailBody };

                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _configuration["Brevo:SmtpHost"] ?? "smtp-relay.brevo.com",
                    int.Parse(_configuration["Brevo:SmtpPort"] ?? "587"),
                    SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    _configuration["Brevo:SmtpUser"],
                    _configuration["Brevo:SmtpPass"]);


                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"✅ Welcome email sent successfully to {email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Failed to send welcome email to {email}");
                return false;
            }
        }
    }
}
