using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Minimart_Api.Data;
using Minimart_Api.DTOS;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.Payments;
using Minimart_Api.Services.RabbitMQ;
using Minimart_Api.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Minimart_Api.Repositories
{
    public class MyRepository : IRepository
    {
        private readonly MinimartDBContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly MpesaSandBox _mpesaSandBox;
        private readonly IOrderEventPublisher _orderEventPublisher;
        private readonly ILogger<MyRepository> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public MyRepository(MinimartDBContext myDBContext,
            IConfiguration configuration,
            IOptions<MpesaSandBox> mpesaSandBox, 
            IOrderEventPublisher orderEventPublisher,
            ILogger<MyRepository> logger,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = myDBContext;
            _configuration = configuration;
            _mpesaSandBox = mpesaSandBox.Value;
            _orderEventPublisher = orderEventPublisher;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task SaveRefreshToken(string jsonData)
        {
            var parameters = new[]
            {
                new SqlParameter("@jsonData", jsonData)
            };

            await _dbContext.Database.ExecuteSqlRawAsync("EXEC p_SaveRefreshToken @jsonData", parameters);
        }

        public async Task<UserInfo> GetRefreshToken(string userID)
        {
            try
            {
                // Find Identity user by ID
                var identityUser = await _userManager.FindByIdAsync(userID);
                if (identityUser != null)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    var userRole = roles.FirstOrDefault() ?? "User";

                    return new UserInfo
                    {
                        UserInfoId = 0, // Remove LegacyUserId reference
                        Name = identityUser.DisplayName ?? identityUser.UserName ?? "",
                        Email = identityUser.Email ?? "",
                        RefreshToken = "", // Identity users don't store refresh tokens in the user entity
                        phonenumber = identityUser.PhoneNumber ?? "",
                        Password = "", // Never return password
                        RoleID = userRole,
                        StatusId = 1,
                        Status = new LoginResponseStatus { 
                            ResponseCode = true,
                            ResponseStatusId = 200,
                            ResponseMessage = "Refresh Token Retrieved Successfully"
                        }
                    };
                }

                // Return empty UserInfo if not found
                return new UserInfo
                {
                    Status = new LoginResponseStatus
                    {
                        ResponseCode = false,
                        ResponseStatusId = 404,
                        ResponseMessage = "User not found"
                    }
                };
            }
            catch (Exception ex) 
            {
                _logger.LogInformation($"Error in retrieving Refresh Token, {ex.Message}");
                return new UserInfo
                {
                    Status = new LoginResponseStatus
                    {
                        ResponseCode = false,
                        ResponseStatusId = 500,
                        ResponseMessage = $"Error retrieving refresh token: {ex.Message}"
                    }
                };
            } 
        }

        // Category operations following the same pattern as existing methods
        public async Task<List<object>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _dbContext.Categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .Select(c => new 
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        SortOrder = c.SortOrder,
                        MerchantId = c.MerchantID,
                        CreatedOn = c.CreatedOn,
                        CreatedBy = c.CreatedBy
                    })
                    .ToListAsync();

                return categories.Cast<object>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in retrieving Categories");
                return new List<object>();
            }
        }

        public async Task<object> GetCategoryAsync(Guid categoryId)
        {
            try
            {
                var category = await _dbContext.Categories
                    .Where(c => c.CategoryId == categoryId)
                    .Select(c => new 
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        SortOrder = c.SortOrder,
                        MerchantId = c.MerchantID,
                        CreatedOn = c.CreatedOn,
                        CreatedBy = c.CreatedBy
                    })
                    .FirstOrDefaultAsync();

                if (category == null)
                {
                    return new { Message = "Category not found" };
                }

                return category;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in retrieving Category {CategoryId}", categoryId);
                return new { Message = $"Error: {ex.Message}" };
            }
        }

        public async Task<object> CreateCategoryAsync(string jsonData)
        {
            try
            {
                var categoryRequest = JsonConvert.DeserializeObject<dynamic>(jsonData);
                
                var category = new Models.Category
                {
                    CategoryId = Guid.NewGuid(),
                    Name = categoryRequest?.Name ?? "New Category",
                    Description = categoryRequest?.Description,
                    IsActive = true,
                    SortOrder = 0,
                    MerchantID = categoryRequest?.MerchantId ?? Guid.Empty,
                    CreatedBy = categoryRequest?.UserId ?? "",
                    CreatedOn = DateTime.UtcNow
                };

                _dbContext.Categories.Add(category);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Category created: {CategoryId}", category.CategoryId);

                return new 
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    Message = "Category created successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in creating Category");
                return new { Message = $"Error: {ex.Message}" };
            }
        }

        private async Task<STKPushResponse> InitiateMpesaSTKPush(PaymentDetailsDto paymentDetails)
        {
            string token = string.Empty;

            try
            {
                var newSTKPushRequest = new STKPushRequest
                {
                    BusinessShortCode = "174379",
                    Password = GeneratePassword(),
                    Timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                    TransactionType = "CustomerPayBillOnline",
                    Amount = paymentDetails.Amount,
                    PartyA = Convert.ToString(paymentDetails.PaymentReference),
                    PartyB = "174379",
                    PhoneNumber = Convert.ToString(paymentDetails.PaymentReference),
                    CallBackURL = "https://ce19-102-213-49-29.ngrok-free.app/mpesa/callback",
                    AccountReference = $"Order{paymentDetails.PaymentID}",
                    TransactionDesc = $"Payment For {paymentDetails.PaymentID}",
                };

                var json = JsonConvert.SerializeObject(newSTKPushRequest);

                var ConsumerKey = _mpesaSandBox.ConsumerKey;
                var ConsumerSecret = _mpesaSandBox.ConsumerSecret;
                var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ConsumerKey}:{ConsumerSecret}"))
                );

                var Authresponse = await client.GetAsync(_mpesaSandBox.MpesaSandboxUrl);

                if (Authresponse.IsSuccessStatusCode)
                {
                    var content = await Authresponse.Content.ReadAsStringAsync();

                    var data = JsonConvert.DeserializeObject<dynamic>(content);

                    token = data?["access_token"]?.ToString() ?? throw new InvalidOperationException();
                }
                else
                {
                    throw new HttpRequestException($"Failed to get access token. Status Code: {Authresponse.StatusCode}");
                }

                // Send STK PUSH REQUEST
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsJsonAsync(_mpesaSandBox.STKPushUrl, newSTKPushRequest);
                var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                if (response.IsSuccessStatusCode)
                {
                    return new STKPushResponse
                    {
                        MerchantRequestID = responseData.MerchantRequestID,
                        CheckoutRequestID = responseData.CheckoutRequestID,
                        ResponseCode = responseData.ResponseCode,
                        ResponseDescription = responseData.ResponseDescription,
                        CustomerMessage = responseData.CustomerMessage,
                    };
                }
                else
                {
                    // Return a failure response if the API call fails
                    return new STKPushResponse
                    {
                        MerchantRequestID = "",
                        CheckoutRequestID = "",
                        ResponseCode = responseData?.ResponseCode ?? "1",
                        ResponseDescription = responseData?.ResponseDescription ?? "Failed to initiate STK push.",
                        CustomerMessage = responseData?.CustomerMessage ?? "Failed to initiate STK push.",
                    };
                }
            }
            catch (Exception ex)
            {
                // Handle the exception by returning a generic error response
                return new STKPushResponse
                {
                    MerchantRequestID = "",
                    CheckoutRequestID = "",
                    ResponseCode = "1",
                    ResponseDescription = "An error occurred while initiating the STK push.",
                    CustomerMessage = "An error occurred while initiating the STK push.",
                };
            }
        }

        private string GeneratePassword()
        {
            var shortcode = "174379"; // Replace with your shortcode
            var passkey = "bfb279f9aa9bdbcf158e97dd71a467cd2f54f2a74b1cfcfc9e68d8f7cbe72956"; // Replace with your passkey
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var dataToEncode = shortcode + passkey + timestamp;

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(dataToEncode));
        }
    }
}
