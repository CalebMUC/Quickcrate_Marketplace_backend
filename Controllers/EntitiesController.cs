using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Authentication_and_Authorization_Api.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.DTOS.AWS;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.Notification;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Mappings;
using Minimart_Api.Mappings;
using Minimart_Api.Models;
using Minimart_Api.Services;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntitiesController : ControllerBase
    {
        private readonly IMyService _myService;
        private readonly IConfiguration _config;
        private readonly CoreLibraries _coreLibraries;
        private readonly OrderMapper _orderMapper;
        private readonly AwsConfig _awsConfig;

        public EntitiesController(IMyService myService, 
            IConfiguration config,
            CoreLibraries coreLibraries, 
            OrderMapper oderMapper,
            IOptions<AwsConfig> awsOptions
            )
        {
            _myService = myService;
            _config = config;
            _coreLibraries = coreLibraries;
            _orderMapper = oderMapper;
            _awsConfig = awsOptions.Value;
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var newToken = "";
            var refreshToken = Request.Cookies["refereshToken"];

            if (refreshTokenRequest.UserID == null)
            {
                return Ok("UseID cannot be null");
            }

            var UserID = refreshTokenRequest.UserID;

            RefreshTokens refreshTokens = new RefreshTokens()
            {
                RefreshToken = refreshToken,
                UserName = UserID
            };

            var jsonData = JsonConvert.SerializeObject(refreshTokens);

            try
            {
                UserInfo response = await _myService.GetRefreshToken(UserID);

                if (Convert.ToBoolean(response.Status.ResponseCode))//--true
                {
                    //Generate a new Json Web Token
                    //var token = _coreLibraries.GenerateToken(response);
                    var token = "";

                    newToken = token;

                    //Generate a new RefreshToken
                    var newrefreshToken = CoreLibraries.GenerateRefreshToken(UserID);

                    //Save RefreshToken
                    //Serialize RefreshToken
                    var jsonRefreshData = JsonConvert.SerializeObject(newrefreshToken);

                    _myService.SaveRefreshToken(jsonRefreshData);

                    //Set RefreshToken
                    //Set Refresh Token
                    var Cookie = CoreLibraries.SetRefreshToken(newrefreshToken);

                    Response.Cookies.Append("refereshToken", newrefreshToken.RefreshToken, Cookie);

                    return Ok(newToken);
                }
                else
                {
                    return Ok(response.Status.ResponseMessage);
                }
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("UploadImages")]
        public async Task<IActionResult> UploadImages(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                var region = (Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1").Trim();

                // 1. Configure S3 Client with Environment Variables (Production/Development)
                var s3Config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.GetBySystemName(region)
                };

                using var client = new AmazonS3Client(
                    Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? _awsConfig.AccessKey,
                    Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? _awsConfig.SecretKey,
                    s3Config
                );

                // 2. Bucket Name from Environment (with fallback)
                var bucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET") ?? "minimartke-products-upload";
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var folderPrefix = Environment.GetEnvironmentVariable("S3_UPLOAD_FOLDER") ?? "product-images";

                // 3. Upload to S3
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);

                var uploadRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = $"{folderPrefix}/{fileName}",
                    InputStream = memoryStream,
                    ContentType = file.ContentType,
                    //CannedACL = S3CannedACL.PublicRead
                };

                await client.PutObjectAsync(uploadRequest);

                // 4. Generate URL (with CloudFront fallback to S3)
                var cdnUrl = Environment.GetEnvironmentVariable("CLOUDFRONT_URL");
                var fileUrl = cdnUrl != null
                    ? $"{cdnUrl}/{folderPrefix}/{fileName}"
                    : $"https://{bucketName}.s3.{s3Config.RegionEndpoint.SystemName}.amazonaws.com/{folderPrefix}/{fileName}";

                return Ok(new { Url = fileUrl });
            }
            catch (AmazonS3Exception ex)
            {
                // Log the error (implement your logging)
                return StatusCode(500, $"S3 Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Upload failed");
            }
        }

        // New Category endpoints - following same pattern as existing endpoints
        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _myService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpGet("GetCategory/{categoryId}")]
        public async Task<IActionResult> GetCategory(Guid categoryId)
        {
            try
            {
                var category = await _myService.GetCategoryAsync(categoryId);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(request);
                var result = await _myService.CreateCategoryAsync(jsonData);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
            }
        }

        // All other legacy endpoints have been removed
        // Use the Identity system endpoints in IdentityController instead
        // Use the new Order endpoints in OrderController instead
        // Use the new Category endpoints in CategoryController instead
    }

    // Simple request model following the same pattern as RefreshTokenRequest
    public class CreateCategoryRequest
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public Guid MerchantId { get; set; }
        public string UserId { get; set; } = "";
    }
}
