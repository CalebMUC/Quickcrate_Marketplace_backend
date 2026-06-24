using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Mpesa;
using Minimart_Api.DTOS.Payments;
using Minimart_Api.DTOS.PaymentMethods;
using Minimart_Api.Services.Mpesa;
using Minimart_Api.Services.PaymentMethods;
using Minimart_Api.Services.CurrentUserServices;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Controllers
{
    /// <summary>
    /// Payment controller for handling payment operations including payment methods management
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> _logger;
        private readonly IMpesaService _mpesaService;
        private readonly IPaymentMethodService _paymentMethodService;
        private readonly ICurrentUserService _currentUserService;

        public PaymentController(
            ILogger<PaymentController> logger, 
            IMpesaService mpesaService,
            IPaymentMethodService paymentMethodService,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _mpesaService = mpesaService;
            _paymentMethodService = paymentMethodService;
            _currentUserService = currentUserService;
        }

        #region System Payment Methods (Admin Only)

        /// <summary>
        /// Get all system payment methods
        /// </summary>
        /// <returns>List of all system payment methods</returns>
        [HttpGet("system/payment-methods")]
        [Authorize]
        public async Task<IActionResult> GetSystemPaymentMethods()
        {
            try
            {
                _logger.LogInformation("Retrieving all system payment methods");
                
                var result = await _paymentMethodService.GetSystemPaymentMethodsAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system payment methods");
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Get a specific system payment method by ID
        /// </summary>
        /// <param name="id">Payment method ID</param>
        /// <returns>System payment method details</returns>
        [HttpGet("system/payment-methods/{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetSystemPaymentMethodById([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid payment method ID"));
                }

                _logger.LogInformation("Retrieving system payment method {Id}", id);
                
                var result = await _paymentMethodService.GetSystemPaymentMethodByIdAsync(id);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return NotFound(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system payment method {Id}", id);
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Create a new system payment method (Admin only)
        /// </summary>
        /// <param name="request">Payment method creation request</param>
        /// <returns>Created payment method details</returns>
        [HttpPost("system/payment-methods")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateSystemPaymentMethod([FromBody] CreateSystemPaymentMethodDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList();
                    
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid request data", errors));
                }

                _logger.LogInformation("Creating new system payment method: {Name}", request.Name);
                
                var result = await _paymentMethodService.CreateSystemPaymentMethodAsync(request);
                
                if (result.Success)
                {
                    return CreatedAtAction(
                        nameof(GetSystemPaymentMethodById),
                        new { id = result.Data!.PaymentMethodId },
                        result
                    );
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating system payment method");
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Update an existing system payment method (Admin only)
        /// </summary>
        /// <param name="id">Payment method ID</param>
        /// <param name="request">Payment method update request</param>
        /// <returns>Updated payment method details</returns>
        [HttpPut("system/payment-methods/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSystemPaymentMethod(
            [FromRoute] int id,
            [FromBody] CreateSystemPaymentMethodDto request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid payment method ID"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList();
                    
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid request data", errors));
                }

                _logger.LogInformation("Updating system payment method {Id}", id);
                
                var result = await _paymentMethodService.UpdateSystemPaymentMethodAsync(id, request);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system payment method {Id}", id);
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Delete a system payment method (Admin only)
        /// </summary>
        /// <param name="id">Payment method ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("system/payment-methods/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSystemPaymentMethod([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid payment method ID"));
                }

                _logger.LogInformation("Deleting system payment method {Id}", id);
                
                var result = await _paymentMethodService.DeleteSystemPaymentMethodAsync(id);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting system payment method {Id}", id);
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Merchant Payment Methods

        /// <summary>
        /// Get all payment methods for a specific merchant
        /// </summary>
        /// <param name="merchantId">Merchant ID</param>
        /// <returns>List of merchant payment methods</returns>
        [HttpGet("merchant/payment-methods/{merchantId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetMerchantPaymentMethods([FromRoute] Guid merchantId)
        {
            try
            {
                if (merchantId == Guid.Empty)
                {
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid merchant ID"));
                }

                _logger.LogInformation("Retrieving payment methods for merchant {MerchantId}", merchantId);
                
                var result = await _paymentMethodService.GetMerchantPaymentMethodsAsync(merchantId);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant payment methods for {MerchantId}", merchantId);
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Add a payment method to a merchant
        /// </summary>
        /// <param name="request">Merchant payment method creation request</param>
        /// <returns>Created merchant payment method details</returns>
        [HttpPost("merchant/payment-methods")]
        [Authorize(Roles = "Admin,Merchant")]
        public async Task<IActionResult> AddMerchantPaymentMethod([FromBody] CreateMerchantPaymentMethodDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList();
                    
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid request data", errors));
                }

                _logger.LogInformation("Adding payment method {PaymentMethodId} to merchant {MerchantId}", 
                    request.PaymentMethodId, request.MerchantId);
                
                var result = await _paymentMethodService.AddMerchantPaymentMethodAsync(request);
                
                if (result.Success)
                {
                    return CreatedAtAction(
                        nameof(GetMerchantPaymentMethods),
                        new { merchantId = request.MerchantId },
                        result
                    );
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding payment method to merchant");
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Update a merchant's payment method configuration
        /// </summary>
        /// <param name="id">Merchant payment method ID</param>
        /// <param name="request">Update request</param>
        /// <returns>Updated merchant payment method details</returns>
        [HttpPut("merchant/payment-methods/{id:int}")]
        [Authorize(Roles = "Admin,Merchant")]
        public async Task<IActionResult> UpdateMerchantPaymentMethod(
            [FromRoute] int id,
            [FromBody] CreateMerchantPaymentMethodDto request)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid merchant payment method ID"));
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                        .ToList();
                    
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid request data", errors));
                }

                _logger.LogInformation("Updating merchant payment method {Id}", id);
                
                var result = await _paymentMethodService.UpdateMerchantPaymentMethodAsync(id, request);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating merchant payment method {Id}", id);
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Remove a payment method from a merchant
        /// </summary>
        /// <param name="id">Merchant payment method ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("merchant/payment-methods/{id:int}")]
        [Authorize(Roles = "Admin,Merchant")]
        public async Task<IActionResult> RemoveMerchantPaymentMethod([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid merchant payment method ID"));
                }

                _logger.LogInformation("Removing merchant payment method {Id}", id);
                
                var result = await _paymentMethodService.RemoveMerchantPaymentMethodAsync(id);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing merchant payment method {Id}", id);
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Enable or disable a payment method for a merchant
        /// </summary>
        /// <param name="id">Merchant payment method ID</param>
        /// <param name="isEnabled">Enable/disable flag</param>
        /// <returns>Toggle result</returns>
        [HttpPatch("merchant/payment-methods/{id:int}/toggle")]
        [Authorize(Roles = "Admin,Merchant")]
        public async Task<IActionResult> ToggleMerchantPaymentMethod(
            [FromRoute] int id,
            [FromQuery] bool isEnabled)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(PaymentMethodResponse<object>.CreateError("Invalid merchant payment method ID"));
                }

                _logger.LogInformation("Toggling merchant payment method {Id} to {Status}", 
                    id, isEnabled ? "enabled" : "disabled");
                
                var result = await _paymentMethodService.ToggleMerchantPaymentMethodAsync(id, isEnabled);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling merchant payment method {Id}", id);
                return StatusCode(500, PaymentMethodResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Existing M-Pesa Endpoints

        [HttpPost("confirmation")]
        public async Task<IActionResult> Confirmation([FromBody] ConfimationRequest request)
        {
            try
            {
                _logger.LogInformation("Received Confirmation Request: {@Request}", request);
                // Process the confirmation request here
                var response = _mpesaService.Confirmation(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("validation")]
        public async Task<IActionResult> Validation([FromBody] ValidationRequest request)
        {
            try
            {
                _logger.LogInformation("Received Validation Request: {@Request}", request);
                var response = await _mpesaService.Validation(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in validation");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUrl()
        {
            try
            {
                var response = await _mpesaService.RegisterUrl();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in registering url");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("stkPush")]
        public async Task<IActionResult> StkPush([FromBody] StkPushRequest request)
        {
            try
            {
                _logger.LogInformation("Received STK Push Request: {@Request}", request);
                var response = await _mpesaService.StkPush(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in STK Push");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("query-status")]
        public async Task<IActionResult> TrxQueryStatus([FromBody] MpesaTrxQuery query)
        {
            try
            {
                _logger.LogInformation("Received STK Push Request: {@Request}", query);
                var response = await _mpesaService.TrxQueryStatus(query); 
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in STK Push");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("stkcallback")]
        [Consumes("application/json")]
        public async Task<IActionResult> STKCallback()
        {
            string rawRequestBody = string.Empty;

            try
            {
                Request.EnableBuffering();

                using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    rawRequestBody = await reader.ReadToEndAsync();
                }
                Request.Body.Position = 0;

                _logger.LogInformation("📥 RAW STK Callback Received: {RawData}", rawRequestBody);

                if (string.IsNullOrWhiteSpace(rawRequestBody))
                {
                    _logger.LogWarning("Empty callback received");
                    return Ok(new { ResultCode = 0, ResultDesc = "Success" });
                }

                var callbackData = JObject.Parse(rawRequestBody);
                _logger.LogInformation("📋 Parsed Callback Structure: {@CallbackData}", callbackData);

                var stkCallback = callbackData["Body"]?["stkCallback"] ?? callbackData["stkCallback"];
                if (stkCallback == null)
                {
                    _logger.LogWarning("❌ No stkCallback found in callback data. Available keys: {Keys}",
                        string.Join(", ", callbackData.Properties().Select(p => p.Name)));
                    return Ok(new { ResultCode = 0, ResultDesc = "Success" });
                }

                var resultCode = stkCallback["ResultCode"]?.Value<int>() ?? -1;
                var resultDesc = stkCallback["ResultDesc"]?.ToString();
                var checkoutRequestId = stkCallback["CheckoutRequestID"]?.ToString();
                var merchantRequestId = stkCallback["MerchantRequestID"]?.ToString();
                var callbackMetadata = stkCallback["CallbackMetadata"];

                _logger.LogInformation("🔍 Callback Processing - ResultCode: {ResultCode}, Desc: {ResultDesc}, CheckoutID: {CheckoutId}",
                    resultCode, resultDesc, checkoutRequestId);

                if (resultCode == 0)
                {
                    // Payment successful - extract transaction details
                    var paymentData = ExtractPaymentDetails(callbackMetadata);

                    // Validate extracted data
                    if (string.IsNullOrWhiteSpace(paymentData.MpesaReceiptNumber) ||
                        string.IsNullOrWhiteSpace(paymentData.Amount) ||
                        string.IsNullOrWhiteSpace(paymentData.PhoneNumber))
                    {
                        _logger.LogError("Missing required payment data: {@PaymentData}", paymentData);
                        return Ok(new { ResultCode = 0, ResultDesc = "Success" });
                    }

                    _logger.LogInformation("✅ PAYMENT SUCCESS | Receipt: {Receipt} | Amount: {Amount} | Phone: {Phone} | Date: {Date}",
                        paymentData.MpesaReceiptNumber, paymentData.Amount, paymentData.PhoneNumber, paymentData.TransactionDate);

                    // Save to database via service
                    var processed = await _mpesaService.ProcessSuccessfulPayment(paymentData, checkoutRequestId, merchantRequestId);
                    if (!processed)
                    {
                        _logger.LogError("Failed to process successful payment for CheckoutRequestID: {CheckoutRequestId}", checkoutRequestId);
                    }
                }
                else
                {
                    _logger.LogWarning("❌ PAYMENT FAILED | Code: {ResultCode} | Desc: {ResultDesc} | CheckoutID: {CheckoutId}",
                        resultCode, resultDesc, checkoutRequestId);

                    await ProcessFailedPayment(checkoutRequestId, merchantRequestId, resultCode, resultDesc);
                }

                // Always return success to Safaricom to prevent retries
                return Ok(new { ResultCode = 0, ResultDesc = "Success" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 ERROR processing STK Callback. Raw request: {RawRequest}", rawRequestBody);
                // Always return success to Safaricom to prevent retries
                return Ok(new { ResultCode = 0, ResultDesc = "Success" });
            }
        }

        #endregion

        #region Helper Methods

        // Improved extraction with null/format handling
        private PaymentData ExtractPaymentDetails(JToken callbackMetadata)
        {
            var paymentData = new PaymentData();

            if (callbackMetadata?["Item"] is JArray items)
            {
                foreach (var item in items)
                {
                    var name = item["Name"]?.ToString();
                    var valueToken = item["Value"];
                    var value = valueToken?.ToString();

                    switch (name)
                    {
                        case "Amount":
                            paymentData.Amount = value;
                            break;
                        case "MpesaReceiptNumber":
                            paymentData.MpesaReceiptNumber = value;
                            break;
                        case "PhoneNumber":
                            paymentData.PhoneNumber = value;
                            break;
                        case "TransactionDate":
                            paymentData.TransactionDate = value;
                            break;
                        case "AccountReference":
                            paymentData.AccountReference = value;
                            break;
                    }
                }
            }

            return paymentData;
        }

        private async Task ProcessFailedPayment(string checkoutRequestId, string merchantRequestId, int resultCode, string resultDesc)
        {
            try
            {
                // TODO: Implement your failed payment handling logic here
                _logger.LogInformation("💸 Failed payment recorded for CheckoutID: {CheckoutId}", checkoutRequestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving failed payment for CheckoutID: {CheckoutId}", checkoutRequestId);
            }
        }

        #endregion
    }
}
