using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Services.ProductService;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("p")]
    public class ShareController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ShareController> _logger;
        private readonly IConfiguration _configuration;

        public ShareController(
            IProductService productService, 
            ILogger<ShareController> logger,
            IConfiguration configuration)
        {
            _productService = productService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("{slug}")]
        [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByHeader = "User-Agent")]
        public async Task<IActionResult> GetProduct(string slug)
        {
            try
            {
                _logger.LogInformation("Share page requested for slug: {Slug}", slug);

                var userAgent = Request.Headers["User-Agent"].ToString().ToLower();
                
                // Detect social media crawlers and bots
                var isCrawler = userAgent.Contains("whatsapp") ||
                                userAgent.Contains("facebookexternalhit") ||
                                userAgent.Contains("facebot") ||
                                userAgent.Contains("twitterbot") ||
                                userAgent.Contains("linkedinbot") ||
                                userAgent.Contains("slackbot") ||
                                userAgent.Contains("telegrambot") ||
                                userAgent.Contains("discordbot") ||
                                userAgent.Contains("pinterest") ||
                                userAgent.Contains("googlebot") ||
                                userAgent.Contains("bingbot");

                // Fetch product data
                var product = await _productService.GetProductBySlugAsync(slug);

                if (product == null)
                {
                    _logger.LogWarning("Product not found for slug: {Slug}", slug);
                    
                    // Return 404 HTML for crawlers, redirect for humans
                    if (isCrawler)
                    {
                        return NotFound(Generate404Html(slug));
                    }
                    
                    var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "https://quickcrate.co.ke";
                    return Redirect($"{frontendUrl}/404");
                }

                if (isCrawler)
                {
                    _logger.LogInformation("Serving crawler HTML for slug: {Slug}, User-Agent: {UserAgent}", slug, userAgent);
                    var html = GenerateProductHtml(product);
                    return Content(html, "text/html");
                }
                else
                {
                    //Redirect human users to React frontend
                    var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "https://quickcrate.co.ke";
                    _logger.LogInformation("Redirecting human user to: {FrontendUrl}/p/{Slug}", frontendUrl, slug);
                    return Redirect($"{frontendUrl}/product/{slug}");

                    //var html = GenerateProductHtml(product);
                    //return Content(html, "text/html");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating share page for slug: {Slug}", slug);
                return StatusCode(500, "An error occurred while loading the product");
            }
        }

        private string GenerateProductHtml(ProductResponseDto product)
        {
            var discountedPrice = product.Discount > 0 
                ? product.Price * (1 - product.Discount / 100m) 
                : product.Price;
            
            var firstImage = product.ImageUrls?.FirstOrDefault() ?? "https://quickcrate.co.ke/default-product.jpg";
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "https://quickcrate.co.ke";
            var canonicalUrl = $"{frontendUrl}/p/{product.Slug}";
            var apiBaseUrl = _configuration["Api:BaseUrl"] ?? "https://api.quickcrate.co.ke";
            
            // Sanitize for HTML
            var productName = System.Net.WebUtility.HtmlEncode(product.ProductName);
            var description = System.Net.WebUtility.HtmlEncode(
                product.MetaDescription ?? 
                product.Description ?? 
                $"Buy {product.ProductName} in Kenya at QuickCrate"
            );

            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{productName} | Buy in Kenya | QuickCrate</title>
    
    <!-- SEO Meta Tags -->
    <meta name=""description"" content=""{description}"" />
    <meta name=""robots"" content=""index, follow"" />
    <link rel=""canonical"" href=""{canonicalUrl}"" />
    
    <!-- Open Graph / Facebook -->
    <meta property=""og:type"" content=""product"" />
    <meta property=""og:url"" content=""{canonicalUrl}"" />
    <meta property=""og:title"" content=""{productName}"" />
    <meta property=""og:description"" content=""{description}"" />
    <meta property=""og:image"" content=""{firstImage}"" />
    <meta property=""og:image:secure_url"" content=""{firstImage}"" />
    <meta property=""og:image:width"" content=""1200"" />
    <meta property=""og:image:height"" content=""630"" />
    <meta property=""og:image:alt"" content=""{productName}"" />
    <meta property=""og:site_name"" content=""QuickCrate"" />
    <meta property=""og:locale"" content=""en_KE"" />
    <meta property=""product:price:amount"" content=""{discountedPrice:F2}"" />
    <meta property=""product:price:currency"" content=""KES"" />
    <meta property=""product:availability"" content=""{(product.IsActive && product.StockQuantity > 0 ? "in stock" : "out of stock")}"" />
    <meta property=""product:condition"" content=""new"" />
    
    <!-- Twitter Card -->
    <meta name=""twitter:card"" content=""summary_large_image"" />
    <meta name=""twitter:site"" content=""@quickcrate"" />
    <meta name=""twitter:title"" content=""{productName}"" />
    <meta name=""twitter:description"" content=""{description}"" />
    <meta name=""twitter:image"" content=""{firstImage}"" />
    <meta name=""twitter:image:alt"" content=""{productName}"" />
    
    <!-- WhatsApp specific (uses OG tags) -->
    <meta property=""og:image:type"" content=""image/jpeg"" />
    
    <!-- Structured Data (JSON-LD) for Google -->
    <script type=""application/ld+json"">
    {{
        ""@context"": ""https://schema.org/"",
        ""@type"": ""Product"",
        ""name"": ""{productName}"",
        ""image"": ""{firstImage}"",
        ""description"": ""{description}"",
        ""sku"": ""{product.SKU}"",
        ""brand"": {{
            ""@type"": ""Brand"",
            ""name"": ""QuickCrate""
        }},
        ""offers"": {{
            ""@type"": ""Offer"",
            ""url"": ""{canonicalUrl}"",
            ""priceCurrency"": ""KES"",
            ""price"": ""{discountedPrice:F2}"",
            ""priceValidUntil"": ""{DateTime.UtcNow.AddMonths(1):yyyy-MM-dd}"",
            ""availability"": ""https://schema.org/{(product.IsActive && product.StockQuantity > 0 ? "InStock" : "OutOfStock")}"",
            ""seller"": {{
                ""@type"": ""Organization"",
                ""name"": ""QuickCrate""
            }}
        }}
    }}
    </script>
    
    <!-- Instant redirect for crawlers (after meta tags are read) -->
    <meta http-equiv=""refresh"" content=""0;url={canonicalUrl}"" />
    
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
        }}
        .container {{
            text-align: center;
            padding: 2rem;
        }}
        .logo {{
            font-size: 3rem;
            margin-bottom: 1rem;
        }}
        h1 {{
            font-size: 1.5rem;
            margin: 1rem 0;
        }}
        .spinner {{
            border: 3px solid rgba(255, 255, 255, 0.3);
            border-radius: 50%;
            border-top: 3px solid white;
            width: 40px;
            height: 40px;
            animation: spin 1s linear infinite;
            margin: 2rem auto;
        }}
        @keyframes spin {{
            0% {{ transform: rotate(0deg); }}
            100% {{ transform: rotate(360deg); }}
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""logo"">🛒</div>
        <h1>{productName}</h1>
        <p>KES {discountedPrice:N2}</p>
        <div class=""spinner""></div>
        <p>Redirecting to QuickCrate...</p>
    </div>
</body>
</html>";
        }

        private string Generate404Html(string slug)
        {
            var frontendUrl = _configuration["Frontend:BaseUrl"] ?? "https://quickcrate.co.ke";
            
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Product Not Found | QuickCrate</title>
    
    <meta property=""og:type"" content=""website"" />
    <meta property=""og:title"" content=""Product Not Found"" />
    <meta property=""og:description"" content=""The product you're looking for is not available."" />
    <meta property=""og:image"" content=""{frontendUrl}/og-default.jpg"" />
    
    <meta http-equiv=""refresh"" content=""3;url={frontendUrl}"" />
    
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
            margin: 0;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-align: center;
        }}
        h1 {{ font-size: 3rem; }}
    </style>
</head>
<body>
    <div>
        <h1>404</h1>
        <p>Product not found: {slug}</p>
        <p>Redirecting to homepage...</p>
    </div>
</body>
</html>";
        }
    }
}
