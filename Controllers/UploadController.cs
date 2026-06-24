using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minimart_Api.DTOS.AWS;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ILogger<UploadController> _logger;
        private readonly IConfiguration _configuration;
        private readonly AwsConfig _awsConfig;
        public UploadController(ILogger<UploadController> logger, IConfiguration configuration, IOptions<AwsConfig> awsOptions)
        {
            _logger = logger;
            _configuration = configuration;
            _awsConfig = awsOptions.Value;
        }


        [HttpPost("Documents")]
        public async Task<IActionResult> UploadImages(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // 1. Configure S3 Client with Environment Variables (Production/Development)
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
                // Fallback to local storage if AWS fails (development only)
                //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                //{
                //    try
                //    {
                //        var localPath = Path.Combine("wwwroot", "uploads", fileName);
                //        using var stream = new FileStream(localPath, FileMode.Create);
                //        await file.CopyToAsync(stream);
                //        return Ok(new { Url = $"/uploads/{fileName}" });
                //    }
                //    catch
                //    {
                //        return StatusCode(500, "Both S3 and local storage failed");
                //    }
                //}
                return StatusCode(500, "Upload failed");
            }
        }

        //[HttpPost("Document")]
        //public async Task<IActionResult> UploadDocuments(IFormFile formFile)
        //{
        //if (formFile == null || formFile.Length == 0)
        //{
        //    return BadRequest("No file uploaded.");
        //}

        //try
        //{
        //    // 1. Configure AWS Region
        //    var regionName = Environment.GetEnvironmentVariable("AWS_REGION")
        //                     ?? _configuration["AWS:Region"]
        //                     ?? "us-east-1";

        //    var s3Config = new Amazon.S3.AmazonS3Config
        //    {
        //        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(regionName)
        //    };

        //    // 2. Create S3 client
        //    using var s3Client = new Amazon.S3.AmazonS3Client(
        //        Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? _configuration["AWS:AccessKeyId"],
        //        Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? _configuration["AWS:SecretAccessKey"],
        //        s3Config
        //    );

        //    // 3. Prepare file metadata
        //    var bucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME")
        //                     ?? _configuration["AWS:S3BucketName"];

        //    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
        //    var folderPrefix = Environment.GetEnvironmentVariable("S3_UPLOAD_FOLDER")
        //                       ?? _configuration["AWS:FolderPrefix"];

        //    var objectKey = string.IsNullOrEmpty(folderPrefix)
        //        ? fileName
        //        : $"{folderPrefix.TrimEnd('/')}/{fileName}";

        //    await using var newMemoryStream = new MemoryStream();
        //    await formFile.CopyToAsync(newMemoryStream);
        //    newMemoryStream.Position = 0; // ✅ Reset position before upload

        //    // 4. Upload file to S3
        //    var putRequest = new Amazon.S3.Model.PutObjectRequest
        //    {
        //        BucketName = bucketName,
        //        Key = objectKey,
        //        InputStream = newMemoryStream,
        //        ContentType = formFile.ContentType,
        //        CannedACL = Amazon.S3.S3CannedACL.PublicRead
        //    };

        //    await s3Client.PutObjectAsync(putRequest);

        //    // 5. Construct File URL
        //    var cdnUrl = (Environment.GetEnvironmentVariable("AWS_CLOUDFRONT_URL")
        //                  ?? _configuration["AWS:CloudFrontUrl"])?.TrimEnd('/');

        //    var fileUrl = !string.IsNullOrEmpty(cdnUrl)
        //        ? $"{cdnUrl}/{objectKey}"
        //        : $"https://{bucketName}.s3.{s3Config.RegionEndpoint.SystemName}.amazonaws.com/{objectKey}";

        //    return Ok(new { Url = fileUrl });
        //}
        //catch (AmazonS3Exception ex)
        //{
        //    _logger.LogError(ex, "S3 Error: {Message}", ex.Message);
        //    return StatusCode(500, $"S3 Error: {ex.Message}");
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "File upload failed: {Message}", ex.Message);
        //    return StatusCode(500, $"Internal server error: {ex.Message}");
        //}
        //}

    }
}
