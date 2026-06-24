using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minimart_Api.Data;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Mpesa;
using Minimart_Api.DTOS.Payments;
using Minimart_Api.Models;
using Newtonsoft.Json;

namespace Minimart_Api.Repositories.Mpesa
{
    public class MpesaRepo : IMpesaRepo
    {
        private readonly MinimartDBContext _dbContext;
        private readonly ILogger<MpesaRepo> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private readonly MpesaGoLive mpesaGoLive;
        private readonly IConfiguration _config;

        public MpesaRepo(MinimartDBContext dBContext, ILogger<MpesaRepo> logger,
            IHttpClientFactory clientFactory, IOptions<MpesaGoLive> options, IConfiguration config)
        {
            _dbContext = dBContext;
            _logger = logger;
            _clientFactory = clientFactory;
            mpesaGoLive = options.Value;
            _config = config;
        }
        public async Task<ConfirmationResponse> Confirmation(ConfimationRequest request)
        {
            try
            {
                _logger.LogInformation("Processing Confirmation Request: {@Request}", request);
                // Implement your logic to handle the confirmation request here

                var transaction = new MpesaTransaction
                {
                    TransactionType = request.TransactionType,
                    TransID = request.TransID,
                    TransTime = request.TransTime,
                    TransAmount = request.TransAmount,
                    BusinessShortCode = request.BusinessShortCode,
                    BillRefNumber = request.BillRefNumber,
                    InvoiceNumber = request.InvoiceNumber,
                    OrgAccountBalance = request.OrgAccountBalance,
                    ThirdPartyTransID = request.ThirdPartyTransID,
                    MSISDN = request.MSISDN,
                    FirstName = request.FirstName,
                    MiddleName = request.MiddleName,
                    LastName = request.LastName
                };

                _dbContext.MpesaTransactions.Add(transaction);
                await _dbContext.SaveChangesAsync();

                var response = new ConfirmationResponse
                {
                    ResultCode = 0,
                    ResultDesc = "Confirmation received successfully"
                };
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing confirmation request: {@Request}", request);
                throw;
            }
        }
        public async Task<ValidationResponse> Validation(ValidationRequest request)
        {
            _logger.LogInformation("Processing Validation: {@Request}", request);

            // Example rule: reject transactions less than 1 shilling
            if (decimal.TryParse(request.TransAmount, out decimal amount) && amount < 1)
            {
                return new ValidationResponse
                {
                    ResultCode = 1,
                    ResultDesc = "Transaction amount too low"
                };
            }

            return new ValidationResponse
            {
                ResultCode = 0,
                ResultDesc = "Transaction validated successfully"
            };
        }

        public async Task<RegisterUrlResponse> Register()
        {
            string accessToken = await GetAccessTokenAsync();
            _logger.LogInformation("AccessToken: {Token}", accessToken);

            try
            {
                var client = _clientFactory.CreateClient();
                client.BaseAddress = new Uri("https://api.safaricom.co.ke/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var registerUrlRequest = new
                {
                    ShortCode = mpesaGoLive.ShortCode,
                    ResponseType = "Completed",
                    ConfirmationURL = mpesaGoLive.ConfirmationUrl,
                    ValidationURL = mpesaGoLive.ValidationUrl
                };

                var response = await client.PostAsJsonAsync("mpesa/c2b/v2/registerurl", registerUrlRequest);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var registerUrlResponse = JsonConvert.DeserializeObject<RegisterUrlResponse>(jsonResponse);
                    return registerUrlResponse!;
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to register URL. Status: {Status}, Response: {Response}", response.StatusCode, errorResponse);
                    throw new Exception($"Failed to register URL. Status: {response.StatusCode}, Response: {errorResponse}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering URL");
                throw;
            }
        }
      public async Task<StkPushResponse> StkPush(StkPushRequest request)
{
    try
    {
        // Get Access Token
        string accessToken = await GetAccessTokenAsync();
        _logger.LogInformation("AccessToken: {Token}", accessToken);

        // Create HttpClient with proper configuration
        var handler = new HttpClientHandler();
        var client = new HttpClient(handler);
        client.BaseAddress = new Uri("https://api.safaricom.co.ke/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Generate timestamp and password
        var timestamp = DateTime.UtcNow.AddHours(3).ToString("yyyyMMddHHmmss"); // EAT timezone
        var passwordRaw = $"{mpesaGoLive.ShortCode}{mpesaGoLive.Passkey}{timestamp}";
        var password = Convert.ToBase64String(Encoding.UTF8.GetBytes(passwordRaw));

        // Log all details for debugging
        _logger.LogInformation("=== STK Push Debug Information ===");
        _logger.LogInformation("ShortCode: {ShortCode}", mpesaGoLive.ShortCode);
        _logger.LogInformation("ShortCode Type: {Type}", mpesaGoLive.ShortCode.GetType());
        _logger.LogInformation("Passkey (first 10): {Passkey}", mpesaGoLive.Passkey.Substring(0, Math.Min(10, mpesaGoLive.Passkey.Length)));
        _logger.LogInformation("Timestamp: {Timestamp}", timestamp);
        _logger.LogInformation("Password Raw: {Raw}", passwordRaw);
        _logger.LogInformation("Password Base64: {Base64}", password);
        _logger.LogInformation("Callback URL: {Url}", mpesaGoLive.CallbackUrl);
        _logger.LogInformation("Amount: {Amount}", request.Amount);
        _logger.LogInformation("Phone: {Phone}", request.PhoneNumber);

        // Create request payload
        var stkPushRequest = new
        {
            BusinessShortCode = mpesaGoLive.ShortCode.Trim(),
            Password = password,
            Timestamp = timestamp,
            TransactionType = "CustomerPayBillOnline",
            Amount = Math.Round(decimal.Parse(request.Amount), 0).ToString(), // Ensure whole number
            PartyA = request.PhoneNumber.Trim(),
            PartyB = mpesaGoLive.ShortCode.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            CallBackURL = mpesaGoLive.CallbackUrl.Trim(),
            AccountReference = request.AccountReference.Trim(),
            TransactionDesc = request.TransactionDesc.Trim()
        };

        // Log the exact JSON being sent
        var jsonPayload = JsonConvert.SerializeObject(stkPushRequest, Formatting.Indented);
        _logger.LogInformation("Final JSON Payload:\n{Payload}", jsonPayload);

        // Send request
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        
        _logger.LogInformation("Sending request to Safaricom API...");
        var response = await client.PostAsync("mpesa/stkpush/v1/processrequest", content);
        
        var responseContent = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
        _logger.LogInformation("Response Content: {Content}", responseContent);

        if (response.IsSuccessStatusCode)
        {
            var stkPushResponse = JsonConvert.DeserializeObject<StkPushResponse>(responseContent);
            _logger.LogInformation("STK Push initiated successfully. CheckoutID: {CheckoutId}", 
                stkPushResponse?.CheckoutRequestID);

                    // Record payment
                    var newPayment = new PaymentDetails
                    {
                        PaymentMethodID = _dbContext.PaymentMethods.Where(pm => pm.Name == "M-Pesa").
                        Select(pm => pm.PaymentMethodID).FirstOrDefault(),
                        TrxReference = stkPushResponse.CheckoutRequestID,
                        Phonenumber = request.PhoneNumber,
                        Amount = Convert.ToDecimal(request.Amount),
                        PaymentDate = DateTime.UtcNow,
                        PaymentReference = request.PhoneNumber.Trim(),
                        Status = "Pending",
                    };

                    _dbContext.PaymentDetails.Add(newPayment);
                    await _dbContext.SaveChangesAsync();


                    return stkPushResponse!;
        }
        else
        {
            _logger.LogError("STK Push failed with status: {StatusCode}", response.StatusCode);
            throw new Exception($"STK Push failed: {responseContent}");
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "STK Push encountered an exception");
        throw;
    }
}

        public async Task<MpesaTrxQueryRes> TrxQueryStatus(MpesaTrxQuery query)
        {
            try
            {
                var response = await _dbContext.PaymentDetails
                    .FirstOrDefaultAsync(p => p.TrxReference == query.CheckoutRequestID);

                if (response == null)
                {
                    // Transaction does not exist in DB
                    return new MpesaTrxQueryRes
                    {
                        ResultCode = "99",
                        ResultDesc = "Transaction not found",
                        CheckoutRequestID = query.CheckoutRequestID
                    };
                }

                // Handle based on the status in the database
                switch (response.Status?.ToLower())
                {
                    case "success":
                        return new MpesaTrxQueryRes
                        {
                            ResultCode = "00",
                            ResultDesc = "Transaction successful",
                            CheckoutRequestID = response.TrxReference,
                            Amount = response.Amount,
                            Success = true,
                            MpesaReceiptNumber = response.PaymentReference,
                            PhoneNumber = response.Phonenumber
                        };

                    case "pending":
                    case null: // no callback yet, still processing
                        return new MpesaTrxQueryRes
                        {
                            ResultCode = "1032",
                            ResultDesc = "Transaction is still being processed. Please wait.",
                            CheckoutRequestID = response.TrxReference,
                            Success = false
                        };

                    case "failed":
                        return new MpesaTrxQueryRes
                        {
                            ResultCode = "01",
                            ResultDesc = "Transaction failed",
                            CheckoutRequestID = response.TrxReference,
                            Success = false
                        };

                    default:
                        return new MpesaTrxQueryRes
                        {
                            ResultCode = "02",
                            ResultDesc = $"Unknown transaction status: {response.Status}",
                            CheckoutRequestID = response.TrxReference,
                            Success = false
                        };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying transaction status");
                return new MpesaTrxQueryRes
                {
                    ResultCode = "98",
                    ResultDesc = "Internal server error while querying transaction",
                    CheckoutRequestID = query.CheckoutRequestID,
                    Success = false
                };
            }
        }



        public async Task<bool> ProcessSuccessfulPayment(PaymentData paymentData, string checkoutRequestId, string merchantRequestId)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();

                try
                {
                    _logger.LogInformation("🔄 Processing successful payment for CheckoutRequestID: {CheckoutRequestId}, MerchantRequestID: {MerchantRequestId}, PaymentData: {@PaymentData}",
                        checkoutRequestId, merchantRequestId, paymentData);

                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(paymentData.MpesaReceiptNumber) ||
                        string.IsNullOrWhiteSpace(paymentData.Amount) ||
                        string.IsNullOrWhiteSpace(paymentData.PhoneNumber))
                    {
                        _logger.LogError("Missing required payment data: {@PaymentData}", paymentData);
                        return false;
                    }

                    // Parse amount
                    if (!decimal.TryParse(paymentData.Amount, out var amount))
                    {
                        _logger.LogError("Invalid amount: {Amount}", paymentData.Amount);
                        return false;
                    }

                    // Parse phone number
                    if (!long.TryParse(paymentData.PhoneNumber, out var phone))
                    {
                        _logger.LogError("Invalid phone number: {PhoneNumber}", paymentData.PhoneNumber);
                        return false;
                    }

                    // Find existing payment record
                    var payment = await _dbContext.PaymentDetails
                        .FirstOrDefaultAsync(p => p.TrxReference == checkoutRequestId);

                    if (payment == null)
                    {
                        _logger.LogWarning("⚠️ Payment not found for CheckoutRequestID: {CheckoutRequestId}", checkoutRequestId);
                        return false;
                    }

                    // Update payment details
                    payment.Status = "Success";
                    payment.PaymentReference = paymentData.MpesaReceiptNumber;
                    payment.PaymentDate = DateTime.UtcNow;
                    payment.Amount = amount;
                    payment.Phonenumber = paymentData.PhoneNumber;

                    _dbContext.PaymentDetails.Update(payment);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("💰 Payment updated successfully: Receipt {Receipt}, Amount {Amount}",
                        paymentData.MpesaReceiptNumber, paymentData.Amount);

                    // ======== IMPORTANT CHANGE HERE ========
                    // Find the related order using PaymentID (FK on Order)
                    var order = await _dbContext.Orders
                        .FirstOrDefaultAsync(o => o.PaymentID == payment.PaymentID);

                    if (order != null)
                    {
                        order.StatusEnum = Models.Enums.OrderStatusEnum.Paid;
                        order.StatusMessage = "Payment confirmed via M-Pesa";
                        order.PaymentConfirmation = "Confirmed";

                        _dbContext.Orders.Update(order);

                        _logger.LogInformation("📦 Order {OrderId} marked as PAID.", order.OrderID);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ No order found linked to PaymentID: {PaymentID}", payment.PaymentID);
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    _logger.LogError(ex, "❌ Error while processing payment for CheckoutRequestID: {CheckoutRequestId}", checkoutRequestId);
                    return false;
                }
            });
        }






        public async Task<string> GetAccessTokenAsync()
        {
            using var client = new HttpClient();

            // Encode ConsumerKey:ConsumerSecret
            var authBytes = Encoding.UTF8.GetBytes($"{_config["MpesaGoLive:ConsumerKey"]}:{_config["MpesaGoLive:ConsumerSecret"]}");
            var authHeader = Convert.ToBase64String(authBytes);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response = await client.GetAsync(_config["MpesaGoLive:MpesaGoLiveUrl"]);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"❌ Failed to get token: {response.StatusCode} - {result}");
            }

            dynamic json = JsonConvert.DeserializeObject(result);
            string token = json.access_token;
            return token;
        }

    }
}
