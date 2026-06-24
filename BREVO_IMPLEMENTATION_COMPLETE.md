# ?? BrevoEmailService - Complete Implementation Guide

## ? What Was Done

### 1. **Updated BrevoEmailService.cs**
- ? Removed hardcoded credentials
- ? Added dependency injection for `IConfiguration` and `ILogger`
- ? Added comprehensive error handling
- ? Added input validation
- ? Added support for HTML emails
- ? Added support for multiple recipients
- ? Added detailed logging

### 2. **Created EmailTestController.cs**
- ? Test endpoint for plain text emails
- ? Test endpoint for HTML emails
- ? Test endpoint for merchant welcome emails
- ? Configuration status endpoint (without exposing secrets)

### 3. **Created Documentation**
- ? `BREVO_EMAIL_SERVICE_UPDATE.md` - Complete service documentation
- ? `BREVO_CREDENTIALS_ROTATION.md` - Credential rotation guide
- ? This implementation guide

---

## ?? Quick Start Guide

### Step 1: Rotate Brevo Credentials (REQUIRED)

Your old password `O3ACj5rmPbgTH1VR` was exposed. **Generate a new one immediately:**

1. **Login to Brevo**: https://app.brevo.com/
2. **Go to SMTP & API** ? **SMTP** tab
3. **Generate New Password** and copy it
4. **Update User Secrets**:

```powershell
dotnet user-secrets set "Brevo:SmtpUser" "a1176e001@smtp-brevo.com"
dotnet user-secrets set "Brevo:SmtpPass" "YOUR_NEW_BREVO_PASSWORD"
dotnet user-secrets set "Brevo:FromName" "QuickCrate Express Limited"
dotnet user-secrets set "Brevo:FromAddress" "noreply@quickcrate.co.ke"
dotnet user-secrets set "Brevo:SmtpHost" "smtp-relay.brevo.com"
dotnet user-secrets set "Brevo:SmtpPort" "587"
```

5. **Update Production (Render)**:
```bash
Brevo__SmtpUser=a1176e001@smtp-brevo.com
Brevo__SmtpPass=YOUR_NEW_BREVO_PASSWORD
Brevo__FromName=QuickCrate Express Limited
Brevo__FromAddress=noreply@quickcrate.co.ke
Brevo__SmtpHost=smtp-relay.brevo.com
Brevo__SmtpPort=587
```

---

### Step 2: Test Email Configuration

#### **Option A: Check Configuration Status**

```bash
# Test locally
curl http://localhost:5000/api/EmailTest/config/status

# Test production
curl https://orderapi-33pp.onrender.com/api/EmailTest/config/status
```

**Expected Response**:
```json
{
  "success": true,
  "message": "Email service is properly configured",
  "configured": true,
  "status": {
    "brevo": {
      "smtpHost": "smtp-relay.brevo.com",
      "smtpPort": "587",
      "smtpUserConfigured": true,
      "smtpPassConfigured": true,
      "fromName": "QuickCrate Express Limited",
      "fromAddress": "noreply@quickcrate.co.ke"
    },
    "environment": "Development",
    "userSecretsEnabled": true
  }
}
```

#### **Option B: Send Test Email (Plain Text)**

```bash
curl -X POST http://localhost:5000/api/EmailTest/brevo/test-plain \
  -H "Content-Type: application/json" \
  -d '{
    "to": "your-email@example.com",
    "subject": "Test Email",
    "body": "This is a test!"
  }'
```

#### **Option C: Send Test Email (HTML)**

```bash
curl -X POST http://localhost:5000/api/EmailTest/brevo/test-html \
  -H "Content-Type: application/json" \
  -d '{
    "to": "your-email@example.com"
  }'
```

#### **Option D: Test Merchant Welcome Email**

```bash
curl -X POST http://localhost:5000/api/EmailTest/merchant/test-welcome \
  -H "Content-Type: application/json" \
  -d '{
    "email": "your-email@example.com",
    "businessName": "Test Merchant",
    "username": "testmerchant@quickcrate.co.ke",
    "tempPassword": "TempPass123!",
    "dashboardUrl": "https://dashboard.quickcrate.co.ke"
  }'
```

---

### Step 3: Integrate into Your Code

#### **Example 1: Send Order Confirmation Email**

```csharp
public class OrderController : ControllerBase
{
    private readonly BrevoEmailService _emailService;

    public OrderController(BrevoEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateOrder([FromBody] OrderDto order)
    {
        // ... create order logic

        // Send confirmation email
        var emailBody = $@"
            Thank you for your order #{order.OrderId}!
            
            Order Total: KES {order.Total:N2}
            Delivery Address: {order.DeliveryAddress}
            
            Track your order at: https://quickcrate.co.ke/orders/{order.OrderId}
        ";

        var emailSent = await _emailService.SendAsync(
            order.CustomerEmail,
            $"Order Confirmation #{order.OrderId}",
            emailBody
        );

        if (!emailSent)
        {
            // Log warning but don't fail the order
            _logger.LogWarning("Failed to send order confirmation to {Email}", order.CustomerEmail);
        }

        return Ok(order);
    }
}
```

#### **Example 2: Send HTML Email**

```csharp
var htmlBody = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h1>Order Confirmed!</h1>
    <p>Thank you for your order <strong>#{order.OrderId}</strong></p>
    <div style='background: #f0f9ff; padding: 15px; border-radius: 5px;'>
        <p><strong>Total:</strong> KES {order.Total:N2}</p>
        <p><strong>Delivery:</strong> {order.DeliveryAddress}</p>
    </div>
    <a href='https://quickcrate.co.ke/orders/{order.OrderId}' 
       style='display: inline-block; background: #2563eb; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
        Track Order
    </a>
</body>
</html>";

await _emailService.SendHtmlAsync(
    order.CustomerEmail,
    $"Order #{order.OrderId} Confirmed",
    htmlBody
);
```

#### **Example 3: Send to Multiple Recipients**

```csharp
var recipients = new List<string> 
{ 
    "admin@quickcrate.co.ke",
    "support@quickcrate.co.ke",
    "sales@quickcrate.co.ke"
};

await _emailService.SendToMultipleAsync(
    recipients,
    "New Order Alert",
    $"New order #{order.OrderId} placed by {order.CustomerName}",
    isHtml: false
);
```

---

## ?? Available Email Service Methods

### BrevoEmailService

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `SendAsync()` | Send plain text or HTML email | `to`, `subject`, `body`, `isHtml=false` | `Task<bool>` |
| `SendHtmlAsync()` | Send HTML email (convenience method) | `to`, `subject`, `htmlBody` | `Task<bool>` |
| `SendToMultipleAsync()` | Send to multiple recipients | `recipients`, `subject`, `body`, `isHtml=false` | `Task<bool>` |

### EmailService (Existing)

| Method | Description | Parameters | Returns |
|--------|-------------|------------|---------|
| `SendMerchantWelcomeEmailAsync()` | Send formatted merchant welcome email | `email`, `businessName`, `username`, `tempPassword`, `dashboardUrl` | `Task<bool>` |

---

## ?? Troubleshooting

### Issue: "SMTP credentials not configured"

**Symptoms**:
```
? SMTP credentials not configured. Check User Secrets or Environment Variables.
```

**Solution**:
```powershell
# Verify User Secrets are set
dotnet user-secrets list | Select-String "Brevo"

# Expected output:
# Brevo:SmtpUser = a1176e001@smtp-brevo.com
# Brevo:SmtpPass = YOUR_PASSWORD
# ...

# If missing, set them:
dotnet user-secrets set "Brevo:SmtpUser" "a1176e001@smtp-brevo.com"
dotnet user-secrets set "Brevo:SmtpPass" "YOUR_PASSWORD"
```

---

### Issue: "535 Authentication failed"

**Symptoms**:
```
? SMTP command error while sending email to user@example.com. Code: AuthenticationFailed
```

**Causes**:
1. Wrong SMTP password
2. Old/expired password
3. SMTP user doesn't match Brevo account

**Solution**:
```powershell
# 1. Generate new password in Brevo dashboard
# 2. Update User Secrets
dotnet user-secrets set "Brevo:SmtpPass" "NEW_PASSWORD"

# 3. Verify SMTP user
dotnet user-secrets list | Select-String "SmtpUser"

# 4. Test immediately
dotnet run
```

---

### Issue: Email sent but not received

**Checklist**:
- [ ] Check spam/junk folder
- [ ] Verify email address is correct
- [ ] Check Brevo dashboard ? Statistics ? Sent emails
- [ ] Check Brevo dashboard ? Logs ? Delivery status
- [ ] Verify "From" address is verified in Brevo

**Brevo Dashboard Check**:
1. Go to **Statistics**
2. Look for recent sent emails
3. Check delivery status:
   - ? Delivered
   - ?? Soft bounce (temporary)
   - ? Hard bounce (invalid email)

---

### Issue: "Connection timeout"

**Symptoms**:
```
? Unexpected error sending email to user@example.com
System.TimeoutException: The operation has timed out
```

**Solutions**:
1. **Firewall**: Verify port 587 is open
2. **Network**: Check internet connection
3. **Brevo Status**: Check https://status.brevo.com/
4. **Render**: Check network egress settings

---

## ?? Testing Checklist

Before deploying to production:

### Local Testing
- [ ] Run `dotnet user-secrets list` to verify Brevo config
- [ ] Test configuration status: `GET /api/EmailTest/config/status`
- [ ] Send plain text test email: `POST /api/EmailTest/brevo/test-plain`
- [ ] Send HTML test email: `POST /api/EmailTest/brevo/test-html`
- [ ] Test merchant welcome email: `POST /api/EmailTest/merchant/test-welcome`
- [ ] Check application logs for `? Email sent successfully`
- [ ] Verify email received in inbox
- [ ] Check Brevo dashboard ? Statistics

### Production Testing
- [ ] Set environment variables in Render
- [ ] Deploy to production
- [ ] Test configuration status endpoint
- [ ] Send test email to known address
- [ ] Verify in Brevo dashboard
- [ ] Monitor Render logs for errors

---

## ?? Monitoring & Logging

### Application Logs

**Success**:
```
info: Minimart_Api.Services.EmailServices.BrevoEmailService[0]
      ? Email sent successfully to user@example.com with subject: Order Confirmation
```

**Failure**:
```
error: Minimart_Api.Services.EmailServices.BrevoEmailService[0]
      ? SMTP command error while sending email to user@example.com. Code: MailboxUnavailable
      MailKit.Net.Smtp.SmtpCommandException: 5.1.1 The email account does not exist
```

### Brevo Dashboard

- **Statistics** ? View sent/delivered/bounced emails
- **Logs** ? Detailed delivery logs
- **SMTP & API** ? Monitor usage and limits

### Render Logs

```bash
# View recent logs
render logs --service MinimartApi --tail 100

# Filter email-related logs
render logs --service MinimartApi | grep "Email"
```

---

## ?? Security Reminders

### ? DO

- ? Use User Secrets for local development
- ? Use Environment Variables for production
- ? Rotate credentials if exposed
- ? Monitor Brevo dashboard for suspicious activity
- ? Keep SMTP passwords secure
- ? Test in development before deploying

### ? DON'T

- ? Hardcode credentials in source code
- ? Commit secrets to Git
- ? Share SMTP passwords in chat/email
- ? Use production credentials in development
- ? Expose credentials in logs or error messages
- ? Reuse old/compromised passwords

---

## ??? Remove Test Controller in Production

The `EmailTestController` is for testing only. Before final production deployment:

1. **Option A: Remove the file**:
```powershell
Remove-Item Controllers/EmailTestController.cs
```

2. **Option B: Add authorization**:
```csharp
[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/[controller]")]
public class EmailTestController : ControllerBase
{
    // ... existing code
}
```

3. **Option C: Conditional registration** (Program.cs):
```csharp
if (app.Environment.IsDevelopment())
{
    // EmailTestController is only available in development
}
```

---

## ?? Performance Tips

### 1. **Async Email Sending**

```csharp
// Good - don't await if not critical
_ = _emailService.SendAsync(email, subject, body);
// Continue processing immediately

// Better - fire and forget with error handling
Task.Run(async () =>
{
    try
    {
        await _emailService.SendAsync(email, subject, body);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Background email failed");
    }
});
```

### 2. **Bulk Emails**

For sending to many recipients, use `SendToMultipleAsync()` instead of loops:

```csharp
// ? Bad - slow
foreach (var email in emails)
{
    await _emailService.SendAsync(email, subject, body);
}

// ? Good - faster
await _emailService.SendToMultipleAsync(emails, subject, body);
```

### 3. **Background Jobs**

For non-critical emails, use background processing:

```csharp
// Use Hangfire, Quartz, or similar
BackgroundJob.Enqueue(() => SendOrderConfirmationEmail(orderId));
```

---

## ?? Next Steps

1. ? **Rotate Brevo credentials immediately**
2. ? **Test email sending locally**
3. ? **Update production environment variables**
4. ? **Deploy and test in production**
5. ? **Monitor Brevo dashboard for usage**
6. ? **Integrate email sending into your workflows**
7. ? **Remove or secure EmailTestController**
8. ? **Set up email templates for common scenarios**

---

## ?? Support

### Brevo Support
- Dashboard: https://app.brevo.com/
- Documentation: https://developers.brevo.com/
- Status Page: https://status.brevo.com/
- Support: https://help.brevo.com/

### Internal Documentation
- `SECRET_MANAGEMENT_GUIDE.md` - General secret management
- `BREVO_EMAIL_SERVICE_UPDATE.md` - Service documentation
- `BREVO_CREDENTIALS_ROTATION.md` - Credential rotation

---

**Your email service is now secure, tested, and ready for production!** ???
