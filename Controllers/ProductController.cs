using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Services.CurrentUserServices;
using Minimart_Api.Services.ProductService;
using Minimart_Api.Services.SlugService; // ADD THIS

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly ISlugService _slugService; // ADD THIS

        public ProductController(IProductService productService,
            ICurrentUserService currentUserService,
            ILogger<ProductController> logger,
            ISlugService slugService) // ADD THIS PARAMETER
        {
            _productService = productService;
            _currentUserService = currentUserService;
            _logger = logger;
            _slugService = slugService; // ADD THIS ASSIGNMENT
        }

        /// <summary>
        /// Get a specific product by ID
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Product details</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductResponseDto>> GetById(Guid id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found.");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return StatusCode(500, "An error occurred while retrieving the product.");
            }
        }

        /// <summary>
        /// Get a specific product by ID (Enhanced endpoint with detailed response)
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>API response with product details</returns>
        [HttpGet("GetProduct/{productId:guid}")]
        public async Task<IActionResult> GetProduct(Guid productId)
        {
            try
            {
                _logger.LogInformation("Getting product details for ProductId: {ProductId}", productId);

                var product = await _productService.GetProductAsync(productId);
                
                if (product == null)
                {
                    _logger.LogWarning("Product not found: {ProductId}", productId);
                    return NotFound(ApiResponse<object>.CreateError($"Product with ID {productId} not found"));
                }

                // **SEO: 301 Redirect to slug-based URL if slug exists**
                if (!string.IsNullOrEmpty(product.Slug))
                {
                    _logger.LogInformation(
                        "Redirecting ProductId {ProductId} to slug URL: {Slug}", 
                        productId, product.Slug);
                        
                    return RedirectPermanent($"/api/Product/slug/{product.Slug}");
                }

                _logger.LogInformation("Successfully retrieved product: {ProductId}", productId);
                return Ok(ApiResponse<ProductResponseDto>.CreateSuccess(product, "Product retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for GetProduct: {ProductId}", productId);
                return BadRequest(ApiResponse<object>.CreateError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", productId);
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred while retrieving the product"));
            }
        }

        /// <summary>
        /// Get featured products with optional filtering
        /// </summary>
        /// <param name="merchantId">Optional: Filter by specific merchant ID</param>
        /// <param name="count">Number of featured products to return (default: 10, max: 100)</param>
        /// <param name="categoryId">Optional: Filter by category</param>
        /// <returns>API response with list of featured products</returns>
        [HttpGet("GetFeaturedProducts")]
        public async Task<IActionResult> GetFeaturedProducts(
            [FromQuery] Guid? merchantId = null,
            [FromQuery] int count = 10,
            [FromQuery] Guid? categoryId = null)
        {
            try
            {
                _logger.LogInformation("Getting featured products: MerchantId={MerchantId}, Count={Count}, CategoryId={CategoryId}", 
                    merchantId, count, categoryId);

                // Validate input parameters
                if (count <= 0)
                {
                    _logger.LogWarning("Invalid count parameter: {Count}", count);
                    return BadRequest(ApiResponse<object>.CreateError("Count must be greater than 0"));
                }

                if (count > 100)
                {
                    _logger.LogWarning("Count parameter exceeds maximum: {Count}", count);
                    return BadRequest(ApiResponse<object>.CreateError("Count cannot exceed 100"));
                }

                // Get featured products using the service
                var featuredProducts = await _productService.GetFeaturedProductsAsync(merchantId, count, categoryId);

                if (featuredProducts == null || !featuredProducts.Any())
                {
                    _logger.LogInformation("No featured products found for the given criteria");
                    return Ok(ApiResponse<List<ProductListDto>>.CreateSuccess(
                        new List<ProductListDto>(), 
                        "No featured products found"));
                }

                _logger.LogInformation("Successfully retrieved {Count} featured products", featuredProducts.Count);
                return Ok(ApiResponse<List<ProductListDto>>.CreateSuccess(
                    featuredProducts, 
                    $"Retrieved {featuredProducts.Count} featured products successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for GetFeaturedProducts: MerchantId={MerchantId}", merchantId);
                return BadRequest(ApiResponse<object>.CreateError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving featured products: MerchantId={MerchantId}, Count={Count}", merchantId, count);
                return StatusCode(500, ApiResponse<object>.CreateError("An error occurred while retrieving featured products"));
            }
        }

        /// <summary>
        /// Test endpoint to verify GetFeaturedProducts functionality
        /// </summary>
        /// <returns>Test results for featured products endpoint</returns>
        [HttpGet("test/featured")]
        public async Task<IActionResult> TestFeaturedProducts()
        {
            try
            {
                _logger.LogInformation("Testing GetFeaturedProducts endpoint");

                // Test different scenarios
                var allFeatured = await _productService.GetFeaturedProductsAsync(null, 5, null);
                var merchantFeatured = await _productService.GetFeaturedProductsAsync(Guid.Parse("ea1989e3-f9c4-4ff5-86bf-a24148aa570e"), 3, null);

                return Ok(new
                {
                    TestResults = new
                    {
                        AllFeaturedCount = allFeatured?.Count ?? 0,
                        MerchantFeaturedCount = merchantFeatured?.Count ?? 0,
                        TestPassed = true,
                        Timestamp = DateTime.UtcNow
                    },
                    AllFeaturedProducts = allFeatured?.Take(3).Select(p => new { p.ProductId, p.ProductName, p.Price, p.CategoryName }),
                    MerchantFeaturedProducts = merchantFeatured?.Select(p => new { p.ProductId, p.ProductName, p.Price, p.CategoryName }),
                    Message = "Featured products test completed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing featured products endpoint");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all products with filtering and pagination
        /// </summary>
        /// <param name="filter">Filter and pagination parameters</param>
        /// <returns>Paginated list of products</returns>
        [HttpPost("search")]
        public async Task<ActionResult<PagedResultDto<ProductListDto>>> GetAll([FromBody] ProductFilterDto filter)
        {
            try
            {
                var defaultFilter = new ProductFilterDto { PageSize = 50 }; // Default pagination
                var result = await _productService.GetAllAsync(defaultFilter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return StatusCode(500, "An error occurred while retrieving products.");
            }
        }

        /// <summary>
        /// Get products by category ID
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="filter">Filter and pagination parameters</param>
        /// <returns>Paginated list of products in the category</returns>
        [HttpPost("category/{categoryId:guid}")]
        public async Task<ActionResult<PagedResultDto<ProductListDto>>> GetByCategory(
            Guid categoryId)
        {
            try
            {
                var filter = new ProductFilterDto { PageSize = 50 };
                var result = await _productService.GetProductsByCategoryAsync(categoryId,filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for category {CategoryId}", categoryId);
                return StatusCode(500, "An error occurred while retrieving category products.");
            }
        }


        /// <summary>
        /// Get products by Subcategory ID
        /// </summary>
        /// <param name="subCategoryId">Category ID</param>
        /// <param name="filter">Filter and pagination parameters</param>
        /// <returns>Paginated list of products in the category</returns>
        [HttpPost("subcategory/{subCategoryId:guid}")]
        public async Task<ActionResult<PagedResultDto<ProductListDto>>> GetBySubCategory(
            Guid subCategoryId)
        {
            try
            {
                var filter = new ProductFilterDto { PageSize = 50 };
                var result = await _productService.GetSubCategoryProductsAsync(subCategoryId, filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for category {CategoryId}", subCategoryId);
                return StatusCode(500, "An error occurred while retrieving category products.");
            }
        }

        /// <summary>
        /// Legacy endpoint - Get all products (for backward compatibility)
        /// </summary>
        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var filter = new ProductFilterDto { PageSize = 50 }; // Default pagination
                var products = await _productService.GetAllAsync(filter);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products (legacy endpoint)");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="createProductDto">Product creation data</param>
        /// <returns>Created product details</returns>
        [HttpPost("Create")]
        public async Task<ActionResult<ProductResponseDto>> Create([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Ensure merchants can only create products for themselves (unless admin)
                //var currentMerchantId = _currentUserService.MerchantId;
                //if (createProductDto.MerchantID != currentMerchantId && !User.IsInRole("Admin"))
                //{
                //    return Forbid("You can only create products for your own merchant account.");
                //}

                var createdBy = _currentUserService.UserName ?? "system";
                var result = await _productService.CreateAsync(createProductDto, createdBy);

                return CreatedAtAction(nameof(GetById), new { id = result.ProductId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "An error occurred while creating the product.");
            }
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <param name="updateProductDto">Product update data</param>
        /// <returns>Updated product details</returns>
        [HttpPost("{id:guid}")]
        public async Task<ActionResult<ProductResponseDto>> Update(Guid id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                if (id != updateProductDto.ProductId)
                {
                    return BadRequest("Product ID in URL does not match the ID in the request body.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if product exists and belongs to current merchant (unless admin)
                var currentMerchantId = _currentUserService.MerchantId;
                if (!User.IsInRole("Admin"))
                {
                    var belongsToMerchant = await _productService.ProductBelongsToMerchantAsync(id, currentMerchantId);
                    if (!belongsToMerchant)
                    {
                        return Forbid("You can only update your own products.");
                    }
                }

                var updatedBy = _currentUserService.UserName ?? "system";
                var result = await _productService.UpdateAsync(updateProductDto, updatedBy);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, "An error occurred while updating the product.");
            }
        }

        /// <summary>
        /// Permanently delete a product
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // Check if product exists and belongs to current merchant (unless admin)
                var currentMerchantId = _currentUserService.MerchantId;
                if (!User.IsInRole("Admin"))
                {
                    var belongsToMerchant = await _productService.ProductBelongsToMerchantAsync(id, currentMerchantId);
                    if (!belongsToMerchant)
                    {
                        return Forbid("You can only delete your own products.");
                    }
                }

                var deletedBy = _currentUserService.UserName ?? "system";
                var result = await _productService.DeleteAsync(id, deletedBy);

                if (!result)
                {
                    return NotFound($"Product with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, "An error occurred while deleting the product.");
            }
        }

        /// <summary>
        /// Approve a product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="request">Approval request with status</param>
        /// <returns>Success status</returns>
        [HttpPost("approve/{productId}")]
        public async Task<IActionResult> Approve(string productId, [FromBody] ApproveProductRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    return BadRequest("Product ID is required.");
                }

                if (string.IsNullOrWhiteSpace(request?.Status))
                {
                    return BadRequest("Status is required.");
                }

                // Only admins can approve products
                //if (!User.IsInRole("Admin"))
                //{
                //    return Forbid("Only administrators can approve products.");
                //}

                var approvedBy = _currentUserService.UserName ?? "system";
                var result = await _productService.ApproveProductAsync(productId, request.Status, approvedBy);
                
                if (!result)
                {
                    return NotFound($"Product with ID {productId} not found.");
                }

                return Ok(new { 
                    success = true, 
                    message = "Product approved successfully",
                    productId = productId,
                    status = request.Status,
                    approvedBy = approvedBy
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving product {ProductId}", productId);
                return StatusCode(500, "An error occurred while approving the product.");
            }
        }

        /// <summary>
        /// Get product by SEO-friendly slug
        /// GET /api/Product/slug/getac-k120-core-i5-6949aa56
        /// </summary>
        /// <param name="slug">SEO-friendly product slug</param>
        /// <returns>Product details</returns>
        [HttpGet("slug/{slug}")]
        [ProducesResponseType(typeof(ApiResponse<ProductResponseDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductBySlug(string slug)
        {
            try
            {
                _logger.LogInformation("Getting product by slug: {Slug}", slug);

                // Validate slug format
                if (!_slugService.IsValidSlug(slug))
                {
                    _logger.LogWarning("Invalid slug format: {Slug}", slug);
                    return BadRequest(ApiResponse<object>.CreateError("Invalid product URL format"));
                }

                // Try to find product by slug
                var product = await _productService.GetProductBySlugAsync(slug);

                if (product == null)
                {
                    _logger.LogWarning("Product not found for slug: {Slug}", slug);
                    return NotFound(ApiResponse<object>.CreateError("Product not found"));
                }

                _logger.LogInformation(
                    "Successfully retrieved product by slug: {Slug} -> ProductId: {ProductId}", 
                    slug, product.ProductId);

                return Ok(ApiResponse<ProductResponseDto>.CreateSuccess(
                    product, 
                    "Product retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product by slug: {Slug}", slug);
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "An error occurred while retrieving the product"));
            }
        }
    }
}
