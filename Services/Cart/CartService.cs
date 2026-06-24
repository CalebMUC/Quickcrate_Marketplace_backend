using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;
using Minimart_Api.Repositories.Cart;
using Minimart_Api.Repositories.ProductRepository;
using Newtonsoft.Json;

namespace Minimart_Api.Services.Cart
{
    /// <summary>
    /// Cart service implementation supporting both legacy and modern methods
    /// </summary>
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICartRepo _legacyCartRepo; // Keep legacy repo for backward compatibility
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            ICartRepo legacyCartRepo,
            IProductRepository productRepository,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _legacyCartRepo = legacyCartRepo;
            _productRepository = productRepository;
            _logger = logger;
        }

        #region Legacy Methods (for backward compatibility)

        public async Task<Status> AddToCart(string cartItems)
        {
            return await _legacyCartRepo.AddToCart(cartItems);
        }

        public async Task<Status> DeleteCartItems(CartItemsDTO cartItems)
        {
            return await _legacyCartRepo.DeleteCartItems(cartItems);
        }

        public async Task<IEnumerable<CartResults>> GetCartItems(string applicationUserId)
        {
            // Convert ApplicationUserId to legacy format if needed for compatibility
            if (int.TryParse(applicationUserId, out int legacyUserId))
            {
                return await _legacyCartRepo.GetCartItems(legacyUserId);
            }
            
            _logger.LogWarning("Could not convert ApplicationUserId {ApplicationUserId} to legacy format", applicationUserId);
            return new List<CartResults>();
        }

        public async Task<IEnumerable<CartResults>> GetBoughtItems(string applicationUserId)
        {
            if (int.TryParse(applicationUserId, out int legacyUserId))
            {
                return await _legacyCartRepo.GetBoughtItems(legacyUserId);
            }
            
            _logger.LogWarning("Could not convert ApplicationUserId {ApplicationUserId} to legacy format", applicationUserId);
            return new List<CartResults>();
        }

        public async Task<SavedProductsDto> SaveItemAsync(SaveItemDto itemDto)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new ArgumentException("Product not found");
                }

                var savedItem = new SavedItems
                {
                    ApplicationUserId = itemDto.ApplicationUserId,
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    SavedOn = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _legacyCartRepo.SaveItemAsync(savedItem);
                
                // Map ProductResponseDto to SavedProductsDto
                return new SavedProductsDto
                {
                    ProductID = product.ProductId,
                    ProductName = product.ProductName,
                    ProductImage = product.ImageUrls?.FirstOrDefault() ?? "",
                    Price = product.Price,
                    Discount = (double)product.Discount,
                    InStock = product.InStock,
                    CategoryName = product.CategoryName ?? "",
                    SavedOn = result.SavedOn,
                    Quantity = result.Quantity
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving item for user {ApplicationUserId}", itemDto.ApplicationUserId);
                throw;
            }
        }

        public async Task<bool> RemoveItemAsync(string applicationUserId, string productId)
        {
            try
            {
                if (!Guid.TryParse(productId, out Guid productGuid))
                {
                    throw new ArgumentException("Invalid product ID format");
                }

                if (int.TryParse(applicationUserId, out int legacyUserId))
                {
                    return await _legacyCartRepo.RemoveItemAsync(legacyUserId, productGuid);
                }
                
                _logger.LogWarning("Could not convert ApplicationUserId {ApplicationUserId} to legacy format", applicationUserId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing saved item for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<IEnumerable<SavedProductsDto>> GetSavedItemsAsync(string applicationUserId)
        {
            try
            {
                if (!int.TryParse(applicationUserId, out int legacyUserId))
                {
                    _logger.LogWarning("Could not convert ApplicationUserId {ApplicationUserId} to legacy format", applicationUserId);
                    return new List<SavedProductsDto>();
                }

                var savedItems = await _legacyCartRepo.GetSavedItemsAsync(legacyUserId);
                
                // Fetch products individually instead of using non-existent GetProductsByIdsAsync
                var result = new List<SavedProductsDto>();
                
                foreach (var savedItem in savedItems)
                {
                    var product = await _productRepository.GetByIdAsync(savedItem.ProductId);
                    if (product != null)
                    {
                        result.Add(new SavedProductsDto
                        {
                            ProductID = product.ProductId,
                            ProductName = product.ProductName,
                            ProductImage = product.ImageUrls?.FirstOrDefault() ?? "",
                            Price = product.Price,
                            Discount = (double)product.Discount,
                            InStock = product.InStock,
                            CategoryName = product.CategoryName ?? "",
                            SavedOn = savedItem.SavedOn,
                            Quantity = savedItem.Quantity
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting saved items for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        private SavedProductsDto? MapToLegacyDto(SavedItems savedItem, ProductResponseDto? product)
        {
            if (product == null) return null;

            return new SavedProductsDto
            {
                ProductID = product.ProductId,
                ProductName = product.ProductName,
                ProductImage = product.ImageUrls?.FirstOrDefault() ?? "",
                Price = product.Price,
                Discount = (double)product.Discount,
                InStock = product.InStock,
                CategoryName = product.CategoryName ?? "",
                SavedOn = savedItem.SavedOn,
                Quantity = savedItem.Quantity
            };
        }

        #endregion

        #region Modern Methods

        public async Task<ApiResponse<CartSummaryDto>> GetCartAsync(string applicationUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                {
                    return ApiResponse<CartSummaryDto>.CreateError("User ID is required");
                }

                var cart = await _cartRepository.GetCartAsync(applicationUserId);
                
                if (cart == null)
                {
                    var emptyCart = new CartSummaryDto
                    {
                        ApplicationUserId = applicationUserId,
                        Items = new List<CartItemDto>(),
                        CreatedAt = DateTime.UtcNow
                    };
                    return ApiResponse<CartSummaryDto>.CreateSuccess(emptyCart, "Cart is empty");
                }

                return ApiResponse<CartSummaryDto>.CreateSuccess(cart, "Cart retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<CartSummaryDto>.CreateError("Failed to retrieve cart");
            }
        }

        public async Task<ApiResponse<IEnumerable<CartItemDto>>> GetBoughtItemsAsync(string applicationUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                {
                    return ApiResponse<IEnumerable<CartItemDto>>.CreateError("User ID is required");
                }

                var boughtItems = await _cartRepository.GetBoughtItemsAsync(applicationUserId);
                return ApiResponse<IEnumerable<CartItemDto>>.CreateSuccess(boughtItems, "Bought items retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bought items for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<IEnumerable<CartItemDto>>.CreateError("Failed to retrieve bought items");
            }
        }

        public async Task<ApiResponse<CartSummaryDto>> AddToCartAsync(AddToCartDto request)
        {
            try
            {
                if (request == null)
                    return ApiResponse<CartSummaryDto>.CreateError("Request cannot be null");

                var cart = await _cartRepository.AddToCartAsync(request);
                return ApiResponse<CartSummaryDto>.CreateSuccess(cart, "Product added to cart successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error adding to cart for user {ApplicationUserId}", request?.ApplicationUserId);
                return ApiResponse<CartSummaryDto>.CreateError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to cart for user {ApplicationUserId}", request?.ApplicationUserId);
                return ApiResponse<CartSummaryDto>.CreateError("Failed to add product to cart");
            }
        }

        public async Task<ApiResponse<CartSummaryDto>> UpdateCartItemAsync(UpdateCartItemDto request)
        {
            try
            {
                if (request == null)
                    return ApiResponse<CartSummaryDto>.CreateError("Request cannot be null");

                var cart = await _cartRepository.UpdateCartItemAsync(request);
                return ApiResponse<CartSummaryDto>.CreateSuccess(cart, 
                    request.Quantity == 0 ? "Product removed from cart" : "Cart item updated successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error updating cart for user {ApplicationUserId}", request?.ApplicationUserId);
                return ApiResponse<CartSummaryDto>.CreateError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart for user {ApplicationUserId}", request?.ApplicationUserId);
                return ApiResponse<CartSummaryDto>.CreateError("Failed to update cart item");
            }
        }

        public async Task<ApiResponse<bool>> RemoveFromCartAsync(RemoveFromCartDto request)
        {
            try
            {
                if (request == null)
                    return ApiResponse<bool>.CreateError("Request cannot be null");

                var result = await _cartRepository.RemoveFromCartAsync(request);
                
                return result
                    ? ApiResponse<bool>.CreateSuccess(true, "Product removed from cart successfully")
                    : ApiResponse<bool>.CreateError("Product not found in cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from cart for user {ApplicationUserId}", request?.ApplicationUserId);
                return ApiResponse<bool>.CreateError("Failed to remove product from cart");
            }
        }

        public async Task<ApiResponse<bool>> ClearCartAsync(string applicationUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<bool>.CreateError("User ID is required");

                var result = await _cartRepository.ClearCartAsync(applicationUserId);
                
                return result
                    ? ApiResponse<bool>.CreateSuccess(true, "Cart cleared successfully")
                    : ApiResponse<bool>.CreateError("Cart not found or already empty");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<bool>.CreateError("Failed to clear cart");
            }
        }

        public async Task<ApiResponse<CartValidationResult>> ValidateCartAsync(string applicationUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<CartValidationResult>.CreateError("User ID is required");

                var validation = await _cartRepository.ValidateCartAsync(applicationUserId);
                return ApiResponse<CartValidationResult>.CreateSuccess(validation, 
                    validation.IsValid ? "Cart is valid" : "Cart validation issues found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<CartValidationResult>.CreateError("Failed to validate cart");
            }
        }

        public async Task<ApiResponse<int>> GetCartItemCountAsync(string applicationUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<int>.CreateError("User ID is required");

                var count = await _cartRepository.GetCartItemCountAsync(applicationUserId);
                return ApiResponse<int>.CreateSuccess(count, "Cart item count retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<int>.CreateError("Failed to get cart item count");
            }
        }

        public async Task<ApiResponse<decimal>> GetCartTotalAsync(string applicationUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<decimal>.CreateError("User ID is required");

                var total = await _cartRepository.GetCartTotalAsync(applicationUserId);
                return ApiResponse<decimal>.CreateSuccess(total, "Cart total retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<decimal>.CreateError("Failed to get cart total");
            }
        }

        public async Task<ApiResponse<bool>> HasItemInCartAsync(string applicationUserId, Guid productId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<bool>.CreateError("User ID is required");

                var hasItem = await _cartRepository.CartItemExistsAsync(applicationUserId, productId);
                return ApiResponse<bool>.CreateSuccess(hasItem, "Cart item existence check completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cart item existence for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<bool>.CreateError("Failed to check cart item existence");
            }
        }

        public async Task<ApiResponse<IEnumerable<SavedItemDto>>> GetSavedItemsModernAsync(string applicationUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<IEnumerable<SavedItemDto>>.CreateError("User ID is required");

                var savedItems = await _cartRepository.GetSavedItemsAsync(applicationUserId);
                return ApiResponse<IEnumerable<SavedItemDto>>.CreateSuccess(savedItems, "Saved items retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting saved items for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<IEnumerable<SavedItemDto>>.CreateError("Failed to retrieve saved items");
            }
        }

        public async Task<ApiResponse<SavedItemDto>> SaveItemModernAsync(SaveItemRequestDto request)
        {
            try
            {
                if (request == null)
                    return ApiResponse<SavedItemDto>.CreateError("Request cannot be null");

                var savedItem = await _cartRepository.SaveItemAsync(request);
                return ApiResponse<SavedItemDto>.CreateSuccess(savedItem, "Item saved successfully");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error saving item for user {ApplicationUserId}", request?.ApplicationUserId);
                return ApiResponse<SavedItemDto>.CreateError(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving item for user {ApplicationUserId}", request?.ApplicationUserId);
                return ApiResponse<SavedItemDto>.CreateError("Failed to save item");
            }
        }

        public async Task<ApiResponse<bool>> RemoveSavedItemAsync(string applicationUserId, Guid productId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<bool>.CreateError("User ID is required");

                var result = await _cartRepository.RemoveSavedItemAsync(applicationUserId, productId);
                
                return result
                    ? ApiResponse<bool>.CreateSuccess(true, "Saved item removed successfully")
                    : ApiResponse<bool>.CreateError("Saved item not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing saved item for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<bool>.CreateError("Failed to remove saved item");
            }
        }

        public async Task<ApiResponse<bool>> MoveToCartAsync(string applicationUserId, Guid productId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<bool>.CreateError("User ID is required");

                var result = await _cartRepository.MoveToCartAsync(applicationUserId, productId);
                
                return result
                    ? ApiResponse<bool>.CreateSuccess(true, "Item moved to cart successfully")
                    : ApiResponse<bool>.CreateError("Saved item not found or could not be moved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving item to cart for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<bool>.CreateError("Failed to move item to cart");
            }
        }

        public async Task<ApiResponse<bool>> MoveToSavedAsync(string applicationUserId, Guid productId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<bool>.CreateError("User ID is required");

                var result = await _cartRepository.MoveToSavedAsync(applicationUserId, productId);
                
                return result
                    ? ApiResponse<bool>.CreateSuccess(true, "Item moved to saved items successfully")
                    : ApiResponse<bool>.CreateError("Cart item not found or could not be moved");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving item to saved for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<bool>.CreateError("Failed to move item to saved items");
            }
        }

        public async Task<ApiResponse<CartSummaryDto>> AddMultipleToCartAsync(string applicationUserId, IEnumerable<AddToCartDto> items)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<CartSummaryDto>.CreateError("User ID is required");

                if (items == null || !items.Any())
                    return ApiResponse<CartSummaryDto>.CreateError("Items list cannot be empty");

                var successCount = 0;
                foreach (var item in items)
                {
                    try
                    {
                        item.ApplicationUserId = applicationUserId;
                        await _cartRepository.AddToCartAsync(item);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error adding product {ProductId} to cart", item.ProductId);
                    }
                }

                var cart = await _cartRepository.GetCartAsync(applicationUserId);
                return ApiResponse<CartSummaryDto>.CreateSuccess(cart!, $"Added {successCount} items to cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding multiple items to cart for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<CartSummaryDto>.CreateError("Failed to add multiple items to cart");
            }
        }

        public async Task<ApiResponse<bool>> RemoveMultipleFromCartAsync(string applicationUserId, IEnumerable<Guid> productIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<bool>.CreateError("User ID is required");

                var successCount = 0;
                foreach (var productId in productIds)
                {
                    try
                    {
                        var request = new RemoveFromCartDto
                        {
                            ApplicationUserId = applicationUserId,
                            ProductId = productId
                        };

                        var result = await _cartRepository.RemoveFromCartAsync(request);
                        if (result) successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error removing product {ProductId} from cart", productId);
                    }
                }

                return ApiResponse<bool>.CreateSuccess(successCount > 0, $"Removed {successCount} items from cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing multiple items from cart for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<bool>.CreateError("Failed to remove multiple items from cart");
            }
        }

        public async Task<ApiResponse<CartSummaryDto>> UpdateMultipleCartItemsAsync(string applicationUserId, IEnumerable<UpdateCartItemDto> updates)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<CartSummaryDto>.CreateError("User ID is required");

                var successCount = 0;
                foreach (var update in updates)
                {
                    try
                    {
                        update.ApplicationUserId = applicationUserId;
                        await _cartRepository.UpdateCartItemAsync(update);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error updating product {ProductId} in cart", update.ProductId);
                    }
                }

                var cart = await _cartRepository.GetCartAsync(applicationUserId);
                return ApiResponse<CartSummaryDto>.CreateSuccess(cart!, $"Updated {successCount} items in cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating multiple cart items for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<CartSummaryDto>.CreateError("Failed to update multiple cart items");
            }
        }

        public async Task<ApiResponse<CartSummaryDto>> MergeCartAsync(string sourceUserId, string targetUserId)
        {
            try
            {
                // Get source cart
                var sourceCart = await _cartRepository.GetCartAsync(sourceUserId);
                if (sourceCart == null || !sourceCart.Items.Any())
                {
                    var targetCart = await _cartRepository.GetCartAsync(targetUserId);
                    return ApiResponse<CartSummaryDto>.CreateSuccess(targetCart ?? new CartSummaryDto(), 
                        "No items to merge from source cart");
                }

                // Add source cart items to target cart
                var addRequests = sourceCart.Items.Select(item => new AddToCartDto
                {
                    ApplicationUserId = targetUserId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                });

                await AddMultipleToCartAsync(targetUserId, addRequests);
                await _cartRepository.ClearCartAsync(sourceUserId);

                var mergedCart = await _cartRepository.GetCartAsync(targetUserId);
                return ApiResponse<CartSummaryDto>.CreateSuccess(mergedCart!, 
                    $"Successfully merged {sourceCart.Items.Count} items from source cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error merging cart from {SourceUserId} to {TargetUserId}", sourceUserId, targetUserId);
                return ApiResponse<CartSummaryDto>.CreateError("Failed to merge carts");
            }
        }

        public async Task<ApiResponse<bool>> TransferCartAsync(string fromUserId, string toUserId)
        {
            try
            {
                var mergeResult = await MergeCartAsync(fromUserId, toUserId);
                return ApiResponse<bool>.CreateSuccess(mergeResult.Success, 
                    mergeResult.Success ? "Cart transferred successfully" : "Failed to transfer cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring cart from {FromUserId} to {ToUserId}", fromUserId, toUserId);
                return ApiResponse<bool>.CreateError("Failed to transfer cart");
            }
        }

        public async Task<ApiResponse<IEnumerable<CartItemDto>>> GetRecommendedItemsAsync(string applicationUserId)
        {
            try
            {
                var recommendations = new List<CartItemDto>();
                return ApiResponse<IEnumerable<CartItemDto>>.CreateSuccess(recommendations, 
                    "Recommendations retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<IEnumerable<CartItemDto>>.CreateError("Failed to get recommendations");
            }
        }

        public async Task<ApiResponse<IEnumerable<CartItemDto>>> GetRecentCartItemsAsync(string applicationUserId, int limit = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<IEnumerable<CartItemDto>>.CreateError("User ID is required");

                var recentItems = await _cartRepository.GetRecentCartItemsAsync(applicationUserId, limit);
                return ApiResponse<IEnumerable<CartItemDto>>.CreateSuccess(recentItems, 
                    "Recent cart items retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent cart items for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<IEnumerable<CartItemDto>>.CreateError("Failed to get recent cart items");
            }
        }

        public async Task<ApiResponse<Dictionary<string, object>>> GetCartAnalyticsAsync(string applicationUserId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(applicationUserId))
                    return ApiResponse<Dictionary<string, object>>.CreateError("User ID is required");

                var cart = await _cartRepository.GetCartAsync(applicationUserId);
                var analytics = new Dictionary<string, object>();

                if (cart != null)
                {
                    analytics["totalItems"] = cart.TotalItems;
                    analytics["totalValue"] = cart.Total;
                    analytics["subtotal"] = cart.SubTotal;
                    analytics["totalDiscount"] = cart.TotalDiscount;
                    analytics["itemCount"] = cart.Items.Count;
                    analytics["averageItemPrice"] = cart.Items.Any() ? cart.Items.Average(i => i.Price) : 0;
                    analytics["categoriesCount"] = cart.Items.Where(i => !string.IsNullOrEmpty(i.CategoryName))
                                                          .Select(i => i.CategoryName).Distinct().Count();
                    analytics["merchantsCount"] = cart.Items.Select(i => i.MerchantId).Distinct().Count();
                    analytics["outOfStockItems"] = cart.Items.Count(i => !i.InStock);
                }
                else
                {
                    analytics = new Dictionary<string, object>
                    {
                        ["totalItems"] = 0,
                        ["totalValue"] = 0m,
                        ["subtotal"] = 0m,
                        ["totalDiscount"] = 0m,
                        ["itemCount"] = 0,
                        ["averageItemPrice"] = 0m,
                        ["categoriesCount"] = 0,
                        ["merchantsCount"] = 0,
                        ["outOfStockItems"] = 0
                    };
                }

                return ApiResponse<Dictionary<string, object>>.CreateSuccess(analytics, 
                    "Cart analytics retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart analytics for user {ApplicationUserId}", applicationUserId);
                return ApiResponse<Dictionary<string, object>>.CreateError("Failed to get cart analytics");
            }
        }

        #endregion
    }
}
