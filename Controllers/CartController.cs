using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.Services.Cart;
using Minimart_Api.Services.Recommedation;
using Minimart_Api.Services.SimilarProducts;
using Minimart_Api.Services.CurrentUserServices;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Controllers
{
    /// <summary>
    /// Modern cart controller following best practices
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ISimilarProductsService _similarProductsService;
        private readonly IRecomedationService _recommendationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            ISimilarProductsService similarProductsService,
            IRecomedationService recommendationService,
            ICurrentUserService currentUserService,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _similarProductsService = similarProductsService;
            _recommendationService = recommendationService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        #region Cart Operations

        /// <summary>
        /// Get user's cart
        /// </summary>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>Cart summary with all items</returns>
        [HttpGet("cart")]
        public async Task<IActionResult> GetCart([FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.GetCartAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Get user's bought items (purchase history)
        /// </summary>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>List of previously purchased items</returns>
        //[HttpGet("bought-items")]
        //public async Task<IActionResult> GetBoughtItems([FromQuery] string? applicationUserId = null)
        //{
        //    try
        //    {
        //        var userId = applicationUserId ?? _currentUserService.UserId;

        //        if (string.IsNullOrWhiteSpace(userId))
        //        {
        //            return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
        //        }

        //        var boughtItems = await _cartService.GetBoughtItems(userId);
        //        return Ok(ApiResponse<object>.CreateSuccess(boughtItems, "Bought items retrieved successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting bought items for user {UserId}", applicationUserId);
        //        return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
        //    }
        //}

        /// <summary>
        /// Get user's bought items using modern API (purchase history)
        /// </summary>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>List of previously purchased items with enhanced details</returns>
        [HttpGet("bought-items")]
        public async Task<IActionResult> GetBoughtItemsModern([FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.GetBoughtItemsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bought items (modern) for user {UserId}", applicationUserId);
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        /// <param name="request">Add to cart request</param>
        /// <returns>Updated cart summary</returns>
        [HttpPost("cart/add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                // Use current user if not specified in request
                if (string.IsNullOrWhiteSpace(request.ApplicationUserId))
                {
                    request.ApplicationUserId = _currentUserService.UserId ?? string.Empty;
                }

                var result = await _cartService.AddToCartAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        /// <param name="request">Update cart item request</param>
        /// <returns>Updated cart summary</returns>
        [HttpPut("cart/update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                // Use current user if not specified in request
                if (string.IsNullOrWhiteSpace(request.ApplicationUserId))
                {
                    request.ApplicationUserId = _currentUserService.UserId ?? string.Empty;
                }

                var result = await _cartService.UpdateCartItemAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        /// <param name="productId">Product ID to remove</param>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>Success status</returns>
        [HttpDelete("cart/{productId:guid}")]
        public async Task<IActionResult> RemoveFromCart(
            Guid productId,
            [FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var request = new RemoveFromCartDto
                {
                    ApplicationUserId = userId,
                    ProductId = productId
                };

                var result = await _cartService.RemoveFromCartAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>Success status</returns>
        [HttpDelete("cart/clear")]
        public async Task<IActionResult> ClearCart([FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.ClearCartAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Validate cart before checkout
        /// </summary>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>Validation result with any issues</returns>
        [HttpPost("cart/validate")]
        public async Task<IActionResult> ValidateCart([FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.ValidateCartAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Quick Cart Info

        /// <summary>
        /// Get cart item count
        /// </summary>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>Number of items in cart</returns>
        [HttpGet("cart/count")]
        public async Task<IActionResult> GetCartItemCount([FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.GetCartItemCountAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Get cart total value
        /// </summary>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>Total cart value</returns>
        [HttpGet("cart/total")]
        public async Task<IActionResult> GetCartTotal([FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.GetCartTotalAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Check if product is in cart
        /// </summary>
        /// <param name="productId">Product ID to check</param>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>Boolean indicating if product is in cart</returns>
        [HttpGet("cart/has-item/{productId:guid}")]
        public async Task<IActionResult> HasItemInCart(
            Guid productId,
            [FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.HasItemInCartAsync(userId, productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if item is in cart");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Saved Items

        /// <summary>
        /// Get user's saved items
        /// </summary>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>List of saved items</returns>
        [HttpGet("saved")]
        public async Task<IActionResult> GetSavedItems([FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.GetSavedItemsModernAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting saved items");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Save item for later
        /// </summary>
        /// <param name="request">Save item request</param>
        /// <returns>Saved item details</returns>
        [HttpPost("saved")]
        public async Task<IActionResult> SaveItem([FromBody] SaveItemRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                // Use current user if not specified in request
                if (string.IsNullOrWhiteSpace(request.ApplicationUserId))
                {
                    request.ApplicationUserId = _currentUserService.UserId ?? string.Empty;
                }

                var result = await _cartService.SaveItemModernAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving item");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Remove saved item
        /// </summary>
        /// <param name="productId">Product ID to remove</param>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>Success status</returns>
        [HttpDelete("saved/{productId:guid}")]
        public async Task<IActionResult> RemoveSavedItem(
            Guid productId,
            [FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.RemoveSavedItemAsync(userId, productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing saved item");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Move saved item to cart
        /// </summary>
        /// <param name="request">Move to cart request</param>
        /// <returns>Success status</returns>
        [HttpPost("saved/move-to-cart")]
        public async Task<IActionResult> MoveToCart([FromBody] MoveToCartRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                // Use current user if not specified in request
                if (string.IsNullOrWhiteSpace(request.ApplicationUserId))
                {
                    request.ApplicationUserId = _currentUserService.UserId ?? string.Empty;
                }

                var result = await _cartService.MoveToCartAsync(request.ApplicationUserId, request.ProductId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving item to cart");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Move cart item to saved items
        /// </summary>
        /// <param name="request">Move to saved request</param>
        /// <returns>Success status</returns>
        [HttpPost("cart/move-to-saved")]
        public async Task<IActionResult> MoveToSaved([FromBody] MoveToSavedRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                // Use current user if not specified in request
                if (string.IsNullOrWhiteSpace(request.ApplicationUserId))
                {
                    request.ApplicationUserId = _currentUserService.UserId ?? string.Empty;
                }

                var result = await _cartService.MoveToSavedAsync(request.ApplicationUserId, request.ProductId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving item to saved");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// Add multiple items to cart
        /// </summary>
        /// <param name="request">Bulk add request</param>
        /// <returns>Updated cart summary</returns>
        [HttpPost("cart/bulk-add")]
        public async Task<IActionResult> AddMultipleToCart([FromBody] BulkAddToCartRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                var userId = request.ApplicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.AddMultipleToCartAsync(userId, request.Items);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding multiple items to cart");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Remove multiple items from cart
        /// </summary>
        /// <param name="request">Bulk remove request</param>
        /// <returns>Success status</returns>
        [HttpDelete("cart/bulk-remove")]
        public async Task<IActionResult> RemoveMultipleFromCart([FromBody] BulkRemoveFromCartRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Invalid request",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                var userId = request.ApplicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.RemoveMultipleFromCartAsync(userId, request.ProductIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing multiple items from cart");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Recommendations and Similar Products

        /// <summary>
        /// Get personalized recommendations
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="limit">Number of recommendations to return</param>
        /// <returns>List of recommended products</returns>
        [HttpGet("recommendations/personalized/{userId}")]
        public async Task<IActionResult> GetPersonalizedRecommendations(
            string userId,
            [FromQuery] int limit = 5)
        {
            try
            {
                var recommendations = await _recommendationService.GetPersonalizedRecommendations(userId, limit);
                return Ok(ApiResponse<object>.CreateSuccess(recommendations, "Personalized recommendations retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting personalized recommendations");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Get complementary products
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="limit">Number of products to return</param>
        /// <returns>List of complementary products</returns>
        [HttpGet("recommendations/complementary/{productId}")]
        public async Task<IActionResult> GetComplementaryProducts(
            string productId,
            [FromQuery] int limit = 5)
        {
            try
            {
                var recommendations = await _recommendationService.GetComplementaryProducts(productId, limit);
                return Ok(ApiResponse<object>.CreateSuccess(recommendations, "Complementary products retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting complementary products");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Get frequently bought together products
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="limit">Number of products to return</param>
        /// <returns>List of frequently bought together products</returns>
        [HttpGet("recommendations/frequently-bought/{productId}")]
        public async Task<IActionResult> GetFrequentlyBoughtTogether(
            string productId,
            [FromQuery] int limit = 5)
        {
            try
            {
                var recommendations = await _recommendationService.GetFrequentlyBoughtTogether(productId, limit);
                return Ok(ApiResponse<object>.CreateSuccess(recommendations, "Frequently bought together products retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting frequently bought together products");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        /// <summary>
        /// Get similar products
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="limit">Number of products to return</param>
        /// <returns>List of similar products</returns>
        [HttpGet("recommendations/similar/{productId}")]
        public async Task<IActionResult> GetSimilarProducts(
            string productId,
            [FromQuery] int limit = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("Product ID is required"));
                }

                if (limit <= 0 || limit > 20)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Limit must be between 1 and 20"));
                }

                var similarProducts = await _similarProductsService.GetSimilarProductsAsync(productId, limit);

                if (!similarProducts.Any())
                {
                    return Ok(ApiResponse<object>.CreateSuccess(new List<object>(),
                        "No similar products found with similarity score of 50 or above"));
                }

                return Ok(ApiResponse<object>.CreateSuccess(similarProducts, "Similar products retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting similar products for {ProductId}", productId);
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get cart analytics
        /// </summary>
        /// <param name="applicationUserId">User ID (optional, uses current user if not provided)</param>
        /// <returns>Cart analytics data</returns>
        [HttpGet("cart/analytics")]
        public async Task<IActionResult> GetCartAnalytics([FromQuery] string? applicationUserId = null)
        {
            try
            {
                var userId = applicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(ApiResponse<object>.CreateError("User ID is required"));
                }

                var result = await _cartService.GetCartAnalyticsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart analytics");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error"));
            }
        }

        #endregion

        #region Legacy Support (for backward compatibility)

        /// <summary>
        /// Legacy endpoint - Add to cart (maintains backward compatibility)
        /// </summary>
        [HttpPost("AddCartItems")]
        public async Task<IActionResult> AddCartItemsLegacy([FromBody] LegacyAddToCartRequest request)
        {
            try
            {
                // Convert legacy request to new format
                var modernRequest = new AddToCartDto
                {
                    ApplicationUserId = request.ApplicationUserId ?? _currentUserService.UserId ?? string.Empty,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };

                var result = await _cartService.AddToCartAsync(modernRequest);

                // Return legacy format response
                return Ok(new
                {
                    ResponseCode = result.Success ? 200 : 400,
                    ResponseMessage = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in legacy add to cart endpoint");
                return BadRequest(new
                {
                    ResponseCode = 500,
                    ResponseMessage = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Legacy endpoint - Get cart items (maintains backward compatibility)
        /// </summary>
        [HttpPost("GetCartItems")]
        public async Task<IActionResult> GetCartItemsLegacy([FromBody] LegacyGetCartRequest request)
        {
            try
            {
                var userId = request.ApplicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { message = "User ID is required" });
                }

                var result = await _cartService.GetCartAsync(userId);

                if (result.Success && result.Data != null)
                {
                    // Convert to legacy format
                    var legacyItems = result.Data.Items.Select(item => new
                    {
                        productID = item.ProductId,
                        ProductName = item.ProductName,
                        ProductImage = item.ProductImage,
                        price = item.Price,
                        Quantity = item.Quantity,
                        InStock = item.InStock,
                        MerchantId = item.MerchantId,
                        ProductDescription = "", // Not available in new format
                        KeyFeatures = "", // Not available in new format
                        Specification = "", // Not available in new format
                        Box = "", // Not available in new format
                        CartID = result.Data.CartId,
                        CartItemID = item.CartItemId
                    });

                    return Ok(legacyItems);
                }

                return Ok(new List<object>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in legacy get cart items endpoint");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Legacy endpoint - Get bought items (maintains backward compatibility)
        /// </summary>
        [HttpPost("GetBoughtItems")]
        public async Task<IActionResult> GetBoughtItemsLegacy([FromBody] LegacyGetCartRequest request)
        {
            try
            {
                var userId = request.ApplicationUserId ?? _currentUserService.UserId;

                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new { message = "User ID is required" });
                }

                var boughtItems = await _cartService.GetBoughtItems(userId);

                return Ok(boughtItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in legacy get bought items endpoint");
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Request Models

        /// <summary>
        /// Bulk add to cart request
        /// </summary>
        public class BulkAddToCartRequest
        {
            public string? ApplicationUserId { get; set; }

            [Required]
            public List<AddToCartDto> Items { get; set; } = new();
        }

        /// <summary>
        /// Bulk remove from cart request
        /// </summary>
        public class BulkRemoveFromCartRequest
        {
            public string? ApplicationUserId { get; set; }

            [Required]
            public List<Guid> ProductIds { get; set; } = new();
        }

        /// <summary>
        /// Legacy request models for backward compatibility
        /// </summary>
        public class LegacyAddToCartRequest
        {
            public string? ApplicationUserId { get; set; }
            public Guid ProductId { get; set; }
            public int Quantity { get; set; }
        }

        public class LegacyGetCartRequest
        {
            public string? ApplicationUserId { get; set; }
        }

        #endregion
    }
}
