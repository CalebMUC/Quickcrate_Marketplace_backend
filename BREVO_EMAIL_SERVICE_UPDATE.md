# BrevoEmailService - Security Update Documentation

## ?? What Changed

The `BrevoEmailService` has been updated to follow security best practices and use the configuration system properly.

### Before (Insecure) ?
```csharp
// Hardcoded credentials in source code
await Client.AuthenticateAsync("8b33f8001@smtp-brevo.com", "O3ACj5rmPbgTH1VR");
```

### After (Secure) ?
```csharp
// Credentials loaded from User Secrets/Environment Variables
var smtpUser = _configuration["Brevo:SmtpUser"];
var smtpPass = _configuration["Brevo:SmtpPass"];
await client.AuthenticateAsync(smtpUser, smtpPass);
```

---

## ? Key Improvements

### 1. **Configuration-Based Credentials**
- ? SMTP credentials loaded from User Secrets (development)
- ? SMTP credentials loaded from Environment Variables (production)
- ? No hardcoded secrets in source code

### 2. **Dependency Injection**
```csharp
public BrevoEmailService(IConfiguration configuration, ILogger<BrevoEmailService> logger)
{
    _configuration = configuration;
    _logger = logger;
}
```

### 3. **Comprehensive Logging**
```csharp
_logger.LogInformation("? Email sent successfully to {Recipient}", to);
_logger.LogError(ex, "? SMTP error while sending email to {Recipient}", to);
```

### 4. **Error Handling**
- ? Catches `SmtpCommandException` (authentication, mailbox errors)
- ? Catches `SmtpProtocolException` (protocol errors)
- ? Catches general exceptions
- ? Returns `bool` indicating success/failure

### 5. **Input Validation**
- ? Validates recipient email is not empty
- ? Validates subject is not empty
- ? Validates SMTP credentials are configured

### 6. **New Features**
- ? `SendAsync()` - Send plain text or HTML emails
- ? `SendHtmlAsync()` - Convenience method for HTML emails
- ? `SendToMultipleAsync()` - Send to multiple recipients

---

## ?? Configuration Required

### User Secrets (Local Development)

Set these using the script or manually:

```bash
dotnet user-secrets set "Brevo:SmtpUser" "YOUR_BREVO_SMTP_USER"
dotnet user-secrets set "Brevo:SmtpPass" "YOUR_NEW_BREVO_PASSWORD"
dotnet user-secrets set "Brevo:FromName" "QuickCrate Express Limited"
dotnet user-secrets set "Brevo:FromAddress" "noreply@quickcrate.co.ke"
dotnet user-secrets set "Brevo:SmtpHost" "smtp-relay.brevo.com"
dotnet user-secrets set "Brevo:SmtpPort" "587"
```

### Environment Variables (Production - Render)

```bash
Brevo__SmtpUser=YOUR_BREVO_SMTP_USER
Brevo__SmtpPass=YOUR_NEW_BREVO_PASSWORD
Brevo__FromName=QuickCrate Express Limited
Brevo__FromAddress=noreply@quickcrate.co.ke
Brevo__SmtpHost=smtp-relay.brevo.com
Brevo__SmtpPort=587
```

---

## ?? Usage Examples

### Example 1: Send Plain Text Email
```csharp
public class MyController : ControllerBase
{
    private readonly BrevoEmailService _emailService;

    public MyController(BrevoEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send-welcome")]
    public async Task<IActionResult> SendWelcome(string email)
    {
        var subject = "Welcome to QuickCrate!";
        var body = "Thank you for joining QuickCrate Express Limited.";
        
        var success = await _emailService.SendAsync(email, subject, body);
        
        if (success)
            return Ok("Email sent successfully");
        else
            return StatusCode(500, "Failed to send email");
    }
}
```

### Example 2: Send HTML Email
```csharp
var htmlBody = @"
<html>
<body>
    <h1>Welcome to QuickCrate!</h1>
    <p>Thank you for registering.</p>
    <a href='https://dashboard.quickcrate.co.ke'>Login to Dashboard</a>
</body>
</html>";

var success = await _emailService.SendHtmlAsync(
    "user@example.com", 
    "Welcome!", 
    htmlBody
);
```

### Example 3: Send to Multiple Recipients
```csharp
var recipients = new List<string> 
{ 
    "admin@quickcrate.co.ke", 
    "support@quickcrate.co.ke" 
};

var success = await _emailService.SendToMultipleAsync(
    recipients,
    "New Order Alert",
    "A new order has been placed",
    isHtml: false
);
```

---

## ?? Integration with Existing Code

### Where BrevoEmailService is Already Registered

The service is already registered in `Program.cs`:

```csharp
builder.Services.AddScoped<BrevoEmailService>();
```

### Inject into Your Controllers/Services

```csharp
public class OrderController : ControllerBase
{
    private readonly BrevoEmailService _emailService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        BrevoEmailService emailService, 
        ILogger<OrderController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("confirm-order")]
    public async Task<IActionResult> ConfirmOrder([FromBody] OrderDto order)
    {
        // ... order processing logic

        // Send confirmation email
        var emailSent = await _emailService.SendHtmlAsync(
            order.CustomerEmail,
            "Order Confirmation #" + order.OrderId,
            BuildOrderConfirmationEmail(order)
        );

        if (!emailSent)
        {
            _logger.LogWarning("Order email failed for {OrderId}", order.OrderId);
            // Continue - don't fail the order if email fails
        }

        return Ok(order);
    }
}
```

---

## ?? Security Features

### 1. **No Secrets in Code**
- All credentials loaded from configuration
- Works with User Secrets (local) and Environment Variables (production)

### 2. **Secure SMTP Connection**
- Uses `SecureSocketOptions.StartTls` for encrypted connection
- Port 587 with STARTTLS (Brevo requirement)

### 3. **Proper Resource Disposal**
- Uses `using` statement to ensure SMTP client is disposed
- Properly disconnects from SMTP server

### 4. **Logging Without Exposing Secrets**
```csharp
// Logs recipient and subject, but NOT credentials
_logger.LogInformation("Email sent to {Recipient}", to);
// Never logs: smtpUser, smtpPass
```

### 5. **Configuration Validation**
```csharp
if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
{
    _logger.LogError("SMTP credentials not configured");
    return false;
}
```

---

## ?? Comparison with EmailService

| Feature | BrevoEmailService | EmailService |
|---------|-------------------|--------------|
| Configuration-based | ? Yes | ? Yes |
| Dependency Injection | ? Yes | ? Yes |
| Logging | ? Yes | ? Yes |
| HTML Support | ? Yes | ? Yes (merchant welcome) |
| Multiple Recipients | ? Yes | ? No |
| Plain Text Support | ? Yes | ? No (HTML only) |
| Generic Email Sending | ? Yes | ? Specialized |

**Recommendation**: 
- Use `EmailService` for specialized merchant welcome emails (already has template)
- Use `BrevoEmailService` for general-purpose email sending

---

## ?? Migration Notes

### Old Code Using BrevoEmailService (Breaking Change)

If you have code calling the old `BrevoEmailService.SendAsync()`, update it:

```csharp
// OLD (will still work, but no logging/error handling)
await brevoService.SendAsync("user@example.com", "Subject", "Body");

// NEW (better - returns success indicator)
var success = await brevoService.SendAsync("user@example.com", "Subject", "Body");
if (!success)
{
    // Handle email failure
}
```

---

## ?? Testing

### Test Locally (Development)

1. **Set User Secrets**:
```bash
dotnet user-secrets set "Brevo:SmtpUser" "your-brevo-user"
dotnet user-secrets set "Brevo:SmtpPass" "your-brevo-password"
```

2. **Test Email Sending**:
```csharp
[HttpPost("test-email")]
public async Task<IActionResult> TestEmail()
{
    var success = await _emailService.SendAsync(
        "your-email@example.com",
        "Test Email",
        "This is a test email from QuickCrate API"
    );

    return Ok(new { Success = success });
}
```

3. **Check Logs**:
```
info: Minimart_Api.Services.EmailServices.BrevoEmailService[0]
      ? Email sent successfully to your-email@example.com with subject: Test Email
```

### Test Production (Render)

1. **Set Environment Variables** in Render Dashboard
2. **Deploy and Test** using the same endpoint
3. **Check Render Logs** for email confirmation

---

## ?? Credential Rotation

### When to Rotate Brevo Credentials

Rotate when:
- ? Credentials are exposed (like in Git history)
- ? Quarterly security review
- ? Team member leaves
- ? Suspicious email activity

### How to Rotate

1. **Get New SMTP Password from Brevo**:
   - Login to [Brevo Dashboard](https://app.brevo.com/)
   - Navigate to: SMTP & API ? SMTP
   - Click "Generate New Password"

2. **Update User Secrets**:
```bash
dotnet user-secrets set "Brevo:SmtpPass" "NEW_PASSWORD"
```

3. **Update Production**:
   - Render Dashboard ? Environment ? `Brevo__SmtpPass` ? Update
   - Redeploy (automatic on environment variable change)

4. **Test**:
```bash
# Test locally
dotnet run

# Test production API endpoint
curl -X POST https://your-api.com/api/test-email
```

---

## ?? Monitoring & Troubleshooting

### Common Issues

| Error | Cause | Solution |
|-------|-------|----------|
| "SMTP credentials not configured" | User Secrets/Env Vars not set | Run `dotnet user-secrets list` to verify |
| "Authentication failed" | Wrong credentials | Verify SMTP user/pass in Brevo dashboard |
| "535 Authentication failed" | Old/revoked password | Generate new SMTP password |
| "Connection timeout" | Firewall/network issue | Check Render logs, verify port 587 is open |

### Monitoring Email Sending

Check Brevo Dashboard:
- **Statistics** ? View sent emails
- **Logs** ? Check delivery status
- **SMTP & API** ? Monitor usage

Check Application Logs:
```csharp
// Success
? Email sent successfully to user@example.com with subject: Welcome

// Failure
? SMTP command error while sending email to user@example.com. Code: MailboxUnavailable
```

---

## ?? Best Practices

### ? DO

- ? Use `BrevoEmailService` for all general email sending
- ? Check return value (`bool`) and handle failures gracefully
- ? Log email failures but continue processing (don't fail transactions)
- ? Use meaningful subject lines for better tracking
- ? Test emails in development before deploying

### ? DON'T

- ? Don't hardcode email content in controllers (use templates)
- ? Don't fail critical operations if email fails (log and continue)
- ? Don't send emails synchronously in request handlers (use background jobs for bulk)
- ? Don't expose SMTP credentials in logs or error messages
- ? Don't commit SMTP credentials to Git

---

## ?? Summary

? **Security**: No hardcoded credentials, uses User Secrets/Environment Variables  
? **Logging**: Comprehensive logging for debugging and monitoring  
? **Error Handling**: Graceful failure with specific exception handling  
? **Flexibility**: Supports plain text, HTML, and multiple recipients  
? **Best Practices**: Dependency injection, input validation, resource disposal  
? **Production-Ready**: Works seamlessly in development and production  

---

## ?? Support

### Issues with Email Sending
1. Check User Secrets: `dotnet user-secrets list`
2. Check Brevo Dashboard for SMTP status
3. Review application logs for error details
4. Test with a simple plain text email first

### Need Help?
- ?? Brevo SMTP Docs: https://developers.brevo.com/docs/send-a-transactional-email
- ?? Brevo Support: https://help.brevo.com/
- ?? Internal: Check `SECRET_MANAGEMENT_GUIDE.md`

---

**Your BrevoEmailService is now secure and production-ready!** ??
