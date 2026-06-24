using Microsoft.AspNetCore.Mvc;
using Minimart_Api.Services.EmailServices;

namespace Minimart_Api.Controllers
{
    /// <summary>
    /// Test controller for email service - Remove in production
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTestController : ControllerBase
    {
        private readonly BrevoEmailService _brevoEmailService;
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailTestController> _logger;

        public EmailTestController(
            BrevoEmailService brevoEmailService,
            IEmailService emailService,
            ILogger<EmailTestController> logger)
        {
            _brevoEmailService = brevoEmailService;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Test BrevoEmailService - Send plain text email
        /// </summary>
        [HttpPost("brevo/test-plain")]
        public async Task<IActionResult> TestBrevoPlainEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return BadRequest(new { success = false, message = "Email address is required" });
            }

            var subject = request.Subject ?? "QuickCrate API - Test Email";
            var body = request.Body ?? "This is a test email from QuickCrate API. If you received this, the email service is working correctly!";

            var success = await _brevoEmailService.SendAsync(request.To, subject, body);

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Email sent successfully!",
                    details = new
                    {
                        to = request.To,
                        subject = subject,
                        service = "BrevoEmailService",
                        type = "plain text"
                    }
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send email. Check application logs for details."
                });
            }
        }

        /// <summary>
        /// Test BrevoEmailService - Send HTML email
        /// </summary>
        [HttpPost("brevo/test-html")]
        public async Task<IActionResult> TestBrevoHtmlEmail([FromBody] TestEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To))
            {
                return BadRequest(new { success = false, message = "Email address is required" });
            }

            var subject = request.Subject ?? "QuickCrate API - HTML Test Email";
            var htmlBody = request.Body ?? @"
                <html>
                <head><title>Test Email</title></head>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h1 style='color: #2563eb;'>? Email Service is Working!</h1>
                        <p>This is an <strong>HTML test email</strong> from QuickCrate API.</p>
                        <p>If you're seeing this formatted email, the Brevo email service is configured correctly.</p>
                        <div style='background-color: #f0f9ff; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <p style='margin: 0;'><strong>Configuration Status:</strong> ? Successful</p>
                            <p style='margin: 5px 0 0 0;'><strong>Service:</strong> BrevoEmailService</p>
                        </div>
                        <p style='color: #666; font-size: 0.9em;'>
                            Sent from QuickCrate API Test Endpoint
                        </p>
                    </div>
                </body>
                </html>";

            var success = await _brevoEmailService.SendHtmlAsync(request.To, subject, htmlBody);

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = "HTML email sent successfully!",
                    details = new
                    {
                        to = request.To,
                        subject = subject,
                        service = "BrevoEmailService",
                        type = "HTML"
                    }
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send email. Check application logs for details."
                });
            }
        }

        /// <summary>
        /// Test EmailService - Send merchant welcome email
        /// </summary>
        [HttpPost("merchant/test-welcome")]
        public async Task<IActionResult> TestMerchantWelcomeEmail([FromBody] MerchantWelcomeTestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { success = false, message = "Email address is required" });
            }

            var businessName = request.BusinessName ?? "Test Business";
            var username = request.Username ?? "testuser@quickcrate.co.ke";
            var tempPassword = request.TempPassword ?? "TestPass123!";
            var dashboardUrl = request.DashboardUrl ?? "https://dashboard.quickcrate.co.ke";

            var success = await _emailService.SendMerchantWelcomeEmailAsync(
                request.Email,
                businessName,
                username,
                tempPassword,
                dashboardUrl
            );

            if (success)
            {
                return Ok(new
                {
                    success = true,
                    message = "Merchant welcome email sent successfully!",
                    details = new
                    {
                        to = request.Email,
                        businessName = businessName,
                        service = "EmailService",
                        type = "Merchant Welcome (HTML)"
                    }
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send email. Check application logs for details."
                });
            }
        }

        /// <summary>
        /// Get email configuration status (without exposing secrets)
        /// </summary>
        [HttpGet("config/status")]
        public IActionResult GetConfigStatus()
        {
            var config = HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            var status = new
            {
                brevo = new
                {
                    smtpHost = config["Brevo:SmtpHost"] ?? "NOT SET",
                    smtpPort = config["Brevo:SmtpPort"] ?? "NOT SET",
                    smtpUserConfigured = !string.IsNullOrWhiteSpace(config["Brevo:SmtpUser"]),
                    smtpPassConfigured = !string.IsNullOrWhiteSpace(config["Brevo:SmtpPass"]),
                    fromName = config["Brevo:FromName"] ?? "NOT SET",
                    fromAddress = config["Brevo:FromAddress"] ?? "NOT SET"
                },
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                userSecretsEnabled = config["Brevo:SmtpUser"] != null
            };

            var allConfigured = status.brevo.smtpUserConfigured &&
                               status.brevo.smtpPassConfigured &&
                               status.brevo.smtpHost != "NOT SET";

            return Ok(new
            {
                success = true,
                message = allConfigured
                    ? "Email service is properly configured"
                    : "Email service is NOT properly configured",
                configured = allConfigured,
                status = status
            });
        }
    }

    // DTOs for test requests
    public class TestEmailRequest
    {
        public string To { get; set; } = "";
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }

    public class MerchantWelcomeTestRequest
    {
        public string Email { get; set; } = "";
        public string? BusinessName { get; set; }
        public string? Username { get; set; }
        public string? TempPassword { get; set; }
        public string? DashboardUrl { get; set; }
    }
}
