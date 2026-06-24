using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.Models;

namespace Minimart_Api.Repositories.Cart
{
    /// <summary>
    /// Modern cart repository implementation following best practices
    /// </summary>
    public class CartRepository : ICartRepository
    {
        private readonly MinimartDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CartRepository> _logger;

        public CartRepository(
            MinimartDBContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<CartRepository> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        #region Cart Operations

        public async Task<CartSummaryDto?> GetCartAsync(string applicationUserId)
        {
            try
            {
                var cart = await _context.Cart
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                            .ThenInclude(p => p.Category)
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                            .ThenInclude(p => p.Merchant)
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);

                if (cart == null)
                    return null;

                var activeItems = cart.CartItems
                    .Where(ci => ci.IsActive && !ci.IsBought && ci.Product != null)
                    .ToList();

                return new CartSummaryDto
                {
                    CartId = cart.CartId,
                    ApplicationUserId = cart.ApplicationUserId!,
                    Items = activeItems.Select(MapToCartItemDto).ToList(),
                    CreatedAt = cart.CreatedAt ?? DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<CartSummaryDto> AddToCartAsync(AddToCartDto request)
        {
            
            try
            {
                // Validate product exists and is available
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == request.ProductId && 
                                            !p.IsDeleted && 
                                            p.IsActive);

                if (product == null)
                    throw new ArgumentException("Product not found or not available");

                if (product.StockQuantity < request.Quantity)
                    throw new ArgumentException($"Insufficient stock. Available: {product.StockQuantity}");

                // Get or create cart
                var cart = await GetOrCreateCartAsync(request.ApplicationUserId);

                // Check if item already exists in cart
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && 
                                             ci.ProductId == request.ProductId &&
                                             ci.IsActive && 
                                             !ci.IsBought);

                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.Quantity = request.Quantity;
                    existingItem.UpdatedOn = DateTime.UtcNow;
                }
                else
                {
                    // Add new item
                    var cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        IsActive = true,
                        IsBought = false,
                        CreatedOn = DateTime.UtcNow
                    };

                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Item {ProductId} added to cart for user {ApplicationUserId}", 
                    request.ProductId, request.ApplicationUserId);

                // Return updated cart
                return await GetCartAsync(request.ApplicationUserId) ?? 
                       throw new InvalidOperationException("Failed to retrieve updated cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {ApplicationUserId}", request.ApplicationUserId);
                throw;
            }
        }

        public async Task<CartSummaryDto> UpdateCartItemAsync(UpdateCartItemDto request)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var cart = await _context.Cart
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == request.ApplicationUserId);

                if (cart == null)
                    throw new ArgumentException("Cart not found");

                var cartItem = await _context.CartItems
                    .Include(ci => ci.Product)
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && 
                                             ci.ProductId == request.ProductId &&
                                             ci.IsActive && 
                                             !ci.IsBought);

                if (cartItem == null)
                    throw new ArgumentException("Cart item not found");

                if (request.Quantity == 0)
                {
                    // Remove item if quantity is 0
                    cartItem.IsActive = false;
                    cartItem.UpdatedOn = DateTime.UtcNow;
                }
                else
                {
                    // Validate stock
                    if (cartItem.Product!.StockQuantity < request.Quantity)
                        throw new ArgumentException($"Insufficient stock. Available: {cartItem.Product.StockQuantity}");

                    cartItem.Quantity = request.Quantity;
                    cartItem.UpdatedOn = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                //await transaction.CommitAsync();

                _logger.LogInformation("Cart item {ProductId} updated for user {ApplicationUserId}", 
                    request.ProductId, request.ApplicationUserId);

                return await GetCartAsync(request.ApplicationUserId) ?? 
                       throw new InvalidOperationException("Failed to retrieve updated cart");
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating cart item for user {ApplicationUserId}", request.ApplicationUserId);
                throw;
            }
        }

        public async Task<bool> RemoveFromCartAsync(RemoveFromCartDto request)
        {
            try
            {
                var cart = await _context.Cart
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == request.ApplicationUserId);

                if (cart == null)
                    return false;

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && 
                                             ci.ProductId == request.ProductId &&
                                             ci.IsActive && 
                                             !ci.IsBought);

                if (cartItem == null)
                    return false;

                cartItem.IsActive = false;
                cartItem.UpdatedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Item {ProductId} removed from cart for user {ApplicationUserId}", 
                    request.ProductId, request.ApplicationUserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart for user {ApplicationUserId}", request.ApplicationUserId);
                throw;
            }
        }

        public async Task<bool> ClearCartAsync(string applicationUserId)
        {
            try
            {
                var cart = await _context.Cart
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);

                if (cart == null)
                    return false;

                var activeItems = cart.CartItems
                    .Where(ci => ci.IsActive && !ci.IsBought)
                    .ToList();

                foreach (var item in activeItems)
                {
                    item.IsActive = false;
                    item.UpdatedOn = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cart cleared for user {ApplicationUserId}", applicationUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<CartValidationResult> ValidateCartAsync(string applicationUserId)
        {
            try
            {
                var result = new CartValidationResult { IsValid = true };
                
                var cart = await GetCartAsync(applicationUserId);
                if (cart == null)
                {
                    result.Errors.Add("Cart not found");
                    result.IsValid = false;
                    return result;
                }

                foreach (var item in cart.Items)
                {
                    var validation = new CartItemValidation
                    {
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        RequestedQuantity = item.Quantity,
                        AvailableQuantity = item.AvailableStock,
                        IsInStock = item.InStock,
                        IsActive = true
                    };

                    if (!item.InStock)
                    {
                        validation.Issues.Add("Product is out of stock");
                        validation.IsValid = false;
                        result.IsValid = false;
                    }
                    else if (item.Quantity > item.AvailableStock)
                    {
                        validation.Issues.Add($"Requested quantity ({item.Quantity}) exceeds available stock ({item.AvailableStock})");
                        validation.IsValid = false;
                        result.IsValid = false;
                    }

                    result.ItemValidations.Add(validation);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating cart for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        #endregion

        #region Cart Item Operations

        public async Task<CartItemDto?> GetCartItemAsync(string applicationUserId, Guid productId)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Category)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Merchant)
                    .FirstOrDefaultAsync(ci => ci.Cart.ApplicationUserId == applicationUserId &&
                                             ci.ProductId == productId &&
                                             ci.IsActive && 
                                             !ci.IsBought);

                return cartItem != null ? MapToCartItemDto(cartItem) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<bool> CartItemExistsAsync(string applicationUserId, Guid productId)
        {
            try
            {
                return await _context.CartItems
                    .Include(ci => ci.Cart)
                    .AnyAsync(ci => ci.Cart.ApplicationUserId == applicationUserId &&
                                  ci.ProductId == productId &&
                                  ci.IsActive && 
                                  !ci.IsBought);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cart item existence for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        #endregion

        #region Saved Items Operations

        public async Task<IEnumerable<SavedItemDto>> GetSavedItemsAsync(string applicationUserId)
        {
            try
            {
                var savedItems = await _context.SavedItems
                    .Include(si => si.Product)
                        .ThenInclude(p => p.Category)
                    .Include(si => si.Product)
                        .ThenInclude(p => p.Merchant)
                    .Where(si => si.ApplicationUserId == applicationUserId && si.IsActive)
                    .OrderByDescending(si => si.SavedOn)
                    .ToListAsync();

                return savedItems.Select(MapToSavedItemDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting saved items for user {ApplicationUserId}", applicationUserId);
                throw;
            }

        }

        public async Task<SavedItemDto> SaveItemAsync(SaveItemRequestDto request)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Validate product exists
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == request.ProductId && 
                                            !p.IsDeleted && 
                                            p.IsActive);

                if (product == null)
                    throw new ArgumentException("Product not found or not available");

                // Check if item already saved
                var existingItem = await _context.SavedItems
                    .FirstOrDefaultAsync(si => si.ApplicationUserId == request.ApplicationUserId &&
                                             si.ProductId == request.ProductId);

                SavedItems savedItem;

                if (existingItem != null)
                {
                    existingItem.Quantity = request.Quantity;
                    existingItem.IsActive = true;
                    existingItem.SavedOn = DateTime.UtcNow;
                    savedItem = existingItem;
                }
                else
                {
                    savedItem = new SavedItems
                    {
                        ApplicationUserId = request.ApplicationUserId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        IsActive = true,
                        SavedOn = DateTime.UtcNow
                    };

                    _context.SavedItems.Add(savedItem);
                }

                await _context.SaveChangesAsync();
                //await transaction.CommitAsync();

                _logger.LogInformation("Item {ProductId} saved for user {ApplicationUserId}", 
                    request.ProductId, request.ApplicationUserId);

                // Load the saved item with related data
                var savedItemWithProduct = await _context.SavedItems
                    .Include(si => si.Product)
                        .ThenInclude(p => p.Category)
                    .Include(si => si.Product)
                        .ThenInclude(p => p.Merchant)
                    .FirstOrDefaultAsync(si => si.Id == savedItem.Id);

                return MapToSavedItemDto(savedItemWithProduct!);
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving item for user {ApplicationUserId}", request.ApplicationUserId);
                throw;
            }
        }

        public async Task<bool> RemoveSavedItemAsync(string applicationUserId, Guid productId)
        {
            try
            {
                var savedItem = await _context.SavedItems
                    .FirstOrDefaultAsync(si => si.ApplicationUserId == applicationUserId &&
                                             si.ProductId == productId &&
                                             si.IsActive);

                if (savedItem == null)
                    return false;

                savedItem.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Saved item {ProductId} removed for user {ApplicationUserId}", 
                    productId, applicationUserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing saved item for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<SavedItemDto?> GetSavedItemAsync(string applicationUserId, Guid productId)
        {
            try
            {
                var savedItem = await _context.SavedItems
                    .Include(si => si.Product)
                        .ThenInclude(p => p.Category)
                    .Include(si => si.Product)
                        .ThenInclude(p => p.Merchant)
                    .FirstOrDefaultAsync(si => si.ApplicationUserId == applicationUserId &&
                                             si.ProductId == productId &&
                                             si.IsActive);

                return savedItem != null ? MapToSavedItemDto(savedItem) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting saved item for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<bool> MoveToCartAsync(string applicationUserId, Guid productId)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var savedItem = await _context.SavedItems
                    .FirstOrDefaultAsync(si => si.ApplicationUserId == applicationUserId &&
                                             si.ProductId == productId &&
                                             si.IsActive);

                if (savedItem == null)
                    return false;

                // Add to cart
                var addToCartRequest = new AddToCartDto
                {
                    ApplicationUserId = applicationUserId,
                    ProductId = productId,
                    Quantity = savedItem.Quantity
                };

                await AddToCartAsync(addToCartRequest);

                // Remove from saved items
                savedItem.IsActive = false;
                await _context.SaveChangesAsync();
                //await transaction.CommitAsync();

                _logger.LogInformation("Item {ProductId} moved from saved to cart for user {ApplicationUserId}", 
                    productId, applicationUserId);

                return true;
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                _logger.LogError(ex, "Error moving item to cart for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<bool> MoveToSavedAsync(string applicationUserId, Guid productId)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var cart = await _context.Cart
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);

                if (cart == null)
                    return false;

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId &&
                                             ci.ProductId == productId &&
                                             ci.IsActive && 
                                             !ci.IsBought);

                if (cartItem == null)
                    return false;

                // Save item
                var saveItemRequest = new SaveItemRequestDto
                {
                    ApplicationUserId = applicationUserId,
                    ProductId = productId,
                    Quantity = cartItem.Quantity
                };

                await SaveItemAsync(saveItemRequest);

                // Remove from cart
                cartItem.IsActive = false;
                cartItem.UpdatedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                //await transaction.CommitAsync();

                _logger.LogInformation("Item {ProductId} moved from cart to saved for user {ApplicationUserId}", 
                    productId, applicationUserId);

                return true;
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                _logger.LogError(ex, "Error moving item to saved for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        #endregion

        #region Analytics and Reporting

        public async Task<int> GetCartItemCountAsync(string applicationUserId)
        {
            try
            {
                return await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Where(ci => ci.Cart.ApplicationUserId == applicationUserId &&
                               ci.IsActive && 
                               !ci.IsBought)
                    .SumAsync(ci => ci.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart item count for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<decimal> GetCartTotalAsync(string applicationUserId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Product)
                    .Where(ci => ci.Cart.ApplicationUserId == applicationUserId &&
                               ci.IsActive && 
                               !ci.IsBought && 
                               ci.Product != null)
                    .ToListAsync();

                return cartItems.Sum(ci => ci.Product!.Price * ci.Quantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart total for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<IEnumerable<CartItemDto>> GetRecentCartItemsAsync(string applicationUserId, int limit = 10)
        {
            try
            {
                var recentItems = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Category)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Merchant)
                    .Where(ci => ci.Cart.ApplicationUserId == applicationUserId &&
                               ci.IsActive && 
                               !ci.IsBought)
                    .OrderByDescending(ci => ci.UpdatedOn ?? ci.CreatedOn)
                    .Take(limit)
                    .ToListAsync();

                return recentItems.Select(MapToCartItemDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent cart items for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        public async Task<IEnumerable<CartItemDto>> GetBoughtItemsAsync(string applicationUserId)
        {
            try
            {
                var boughtItems = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Category)
                    .Include(ci => ci.Product)
                        .ThenInclude(p => p.Merchant)
                    .Where(ci => ci.Cart.ApplicationUserId == applicationUserId &&
                               ci.IsBought && 
                               ci.Product != null)
                    .OrderByDescending(ci => ci.UpdatedOn ?? ci.CreatedOn)
                    .ToListAsync();

                return boughtItems.Select(MapToCartItemDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bought items for user {ApplicationUserId}", applicationUserId);
                throw;
            }
        }

        #endregion

        #region Cleanup Operations

        public async Task<int> CleanupInactiveCartsAsync(int daysOld = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                
                var inactiveCartItems = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .Where(ci => ci.IsActive && 
                               !ci.IsBought && 
                               (ci.UpdatedOn ?? ci.CreatedOn) < cutoffDate)
                    .ToListAsync();

                foreach (var item in inactiveCartItems)
                {
                    item.IsActive = false;
                    item.UpdatedOn = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} inactive cart items older than {Days} days", 
                    inactiveCartItems.Count, daysOld);

                return inactiveCartItems.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up inactive carts");
                throw;
            }
        }

        public async Task<int> CleanupExpiredSavedItemsAsync(int daysOld = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                
                var expiredItems = await _context.SavedItems
                    .Where(si => si.IsActive && si.SavedOn < cutoffDate)
                    .ToListAsync();

                foreach (var item in expiredItems)
                {
                    item.IsActive = false;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired saved items older than {Days} days", 
                    expiredItems.Count, daysOld);

                return expiredItems.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired saved items");
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private async Task<Models.Cart> GetOrCreateCartAsync(string applicationUserId)
        {
            var cart = await _context.Cart
                .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);

            if (cart != null)
                return cart;

            var user = await _userManager.FindByIdAsync(applicationUserId);
            var userName = user?.DisplayName ?? user?.UserName ?? user?.Email ?? "Unknown User";

            cart = new Models.Cart
            {
                ApplicationUserId = applicationUserId,
                CartName = userName,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cart.Add(cart);
            await _context.SaveChangesAsync();

            return cart;
        }

        private static CartItemDto MapToCartItemDto(CartItem cartItem)
        {
            var product = cartItem.Product!;
            
            return new CartItemDto
            {
                CartItemId = cartItem.CartItemId,
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductImage = product.ImageUrls?.FirstOrDefault(),
                Price = product.Price,
                Discount = product.Discount,
                Quantity = cartItem.Quantity,
                InStock = product.StockQuantity > 0,
                AvailableStock = product.StockQuantity,
                MerchantId = product.MerchantID,
                MerchantName = product.Merchant?.MerchantName,
                CategoryName = product.Category?.Name,
                AddedOn = cartItem.CreatedOn ?? DateTime.UtcNow,
                UpdatedOn = cartItem.UpdatedOn
            };
        }

        private static SavedItemDto MapToSavedItemDto(SavedItems savedItem)
        {
            var product = savedItem.Product!;
            
            return new SavedItemDto
            {
                Id = savedItem.Id,
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductImage = product.ImageUrls?.FirstOrDefault(),
                Price = product.Price,
                Discount = product.Discount,
                Quantity = savedItem.Quantity,
                InStock = product.StockQuantity > 0,
                CategoryName = product.Category?.Name,
                MerchantId = product.MerchantID,
                MerchantName = product.Merchant?.MerchantName,
                SavedOn = savedItem.SavedOn
            };
        }

        #endregion
    }
}