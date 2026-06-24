using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Minimart_Api.Services.EmailServices
{
    public class BrevoEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BrevoEmailService> _logger;

        public BrevoEmailService(IConfiguration configuration, ILogger<BrevoEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Sends an email using Brevo SMTP with credentials from User Secrets/Environment Variables
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Plain text email body</param>
        /// <param name="isHtml">Whether the body contains HTML (default: false)</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        public async Task<bool> SendAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(to))
                {
                    _logger.LogWarning("Cannot send email: recipient address is empty");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(subject))
                {
                    _logger.LogWarning("Cannot send email: subject is empty");
                    return false;
                }

                // Build email message
                var message = new MimeMessage();
                
                // From address - use configuration with fallback
                var fromName = _configuration["Brevo:FromName"] ?? "QuickCrate Express Limited";
                var fromAddress = _configuration["Brevo:FromAddress"] ?? "noreply@quickcrate.co.ke";
                message.From.Add(new MailboxAddress(fromName, fromAddress));

                // To address
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                // Set body based on content type
                message.Body = isHtml 
                    ? new TextPart(TextFormat.Html) { Text = body }
                    : new TextPart(TextFormat.Plain) { Text = body };

                // Get SMTP configuration from User Secrets/Environment Variables
                var smtpHost = _configuration["Brevo:SmtpHost"] ?? "smtp-relay.brevo.com";
                var smtpPort = int.Parse(_configuration["Brevo:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Brevo:SmtpUser"];
                var smtpPass = _configuration["Brevo:SmtpPass"];

                // Validate SMTP credentials are configured
                if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
                {
                    _logger.LogError("SMTP credentials not configured. Check User Secrets or Environment Variables.");
                    return false;
                }

                // Send email
                using var client = new SmtpClient();
                
                // Connect to SMTP server
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                
                // Authenticate
                await client.AuthenticateAsync(smtpUser, smtpPass);
                
                // Send message
                await client.SendAsync(message);
                
                // Disconnect
                await client.DisconnectAsync(true);

                _logger.LogInformation("✅ Email sent successfully to {Recipient} with subject: {Subject}", to, subject);
                return true;
            }
            catch (SmtpCommandException ex)
            {
                _logger.LogError(ex, "❌ SMTP command error while sending email to {Recipient}. Code: {StatusCode}", to, ex.StatusCode);
                return false;
            }
            catch (SmtpProtocolException ex)
            {
                _logger.LogError(ex, "❌ SMTP protocol error while sending email to {Recipient}", to);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error sending email to {Recipient}", to);
                return false;
            }
        }

        /// <summary>
        /// Sends an HTML email using Brevo SMTP
        /// </summary>
        public async Task<bool> SendHtmlAsync(string to, string subject, string htmlBody)
        {
            return await SendAsync(to, subject, htmlBody, isHtml: true);
        }

        /// <summary>
        /// Sends an email to multiple recipients
        /// </summary>
        public async Task<bool> SendToMultipleAsync(List<string> recipients, string subject, string body, bool isHtml = false)
        {
            try
            {
                if (recipients == null || !recipients.Any())
                {
                    _logger.LogWarning("Cannot send email: no recipients provided");
                    return false;
                }

                var message = new MimeMessage();
                
                var fromName = _configuration["Brevo:FromName"] ?? "QuickCrate Express Limited";
                var fromAddress = _configuration["Brevo:FromAddress"] ?? "noreply@quickcrate.co.ke";
                message.From.Add(new MailboxAddress(fromName, fromAddress));

                // Add all recipients
                foreach (var recipient in recipients)
                {
                    if (!string.IsNullOrWhiteSpace(recipient))
                    {
                        message.To.Add(new MailboxAddress("", recipient));
                    }
                }

                if (!message.To.Any())
                {
                    _logger.LogWarning("Cannot send email: no valid recipients");
                    return false;
                }

                message.Subject = subject;
                message.Body = isHtml 
                    ? new TextPart(TextFormat.Html) { Text = body }
                    : new TextPart(TextFormat.Plain) { Text = body };

                var smtpHost = _configuration["Brevo:SmtpHost"] ?? "smtp-relay.brevo.com";
                var smtpPort = int.Parse(_configuration["Brevo:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Brevo:SmtpUser"];
                var smtpPass = _configuration["Brevo:SmtpPass"];

                if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
                {
                    _logger.LogError("SMTP credentials not configured");
                    return false;
                }

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(smtpUser, smtpPass);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("✅ Email sent successfully to {Count} recipients", recipients.Count);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending email to multiple recipients");
                return false;
            }
        }
    }
}
