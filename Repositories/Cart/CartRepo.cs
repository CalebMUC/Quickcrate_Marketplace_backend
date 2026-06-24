using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Minimart_Api.Repositories.Cart
{
    public class CartRepo : ICartRepo
    {
        private readonly MinimartDBContext _dbContext;
        private readonly ILogger<CartRepo> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartRepo(MinimartDBContext dBContext, ILogger<CartRepo> logger, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dBContext;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IEnumerable<CartResults>> GetCartItems(int userId)
        {
            // Convert legacy int userId to ApplicationUserId string
            var applicationUserId = await ConvertLegacyUserIdToApplicationUserId(userId);
            if (string.IsNullOrEmpty(applicationUserId))
            {
                return new List<CartResults>();
            }

            var cartItems = await _dbContext.CartItems.Where(ci => ci.Cart.ApplicationUserId == applicationUserId && ci.IsActive == true)
                .Select(ci => new { 
                    ProductIdGuid = ci.ProductId,
                    ProductName = ci.Product != null ? ci.Product.ProductName : "",
                    ProductImage = ci.Product != null ? (ci.Product.ImageUrls != null ? ci.Product.ImageUrls.FirstOrDefault() : "") : "",
                    MerchantId = ci.Product != null ? ci.Product.MerchantID : Guid.Empty,
                    Quantity = ci.Quantity,
                    Price = ci.Product != null ? ci.Product.Price : 0,
                    StockQuantity = ci.Product != null ? ci.Product.StockQuantity : 0,
                    ProductDescription = ci.Product != null ? (ci.Product.ProductDescription ?? "") : "",
                    Features = ci.Product != null ? (ci.Product.Features ?? "") : "",
                    ProductSpecification = ci.Product != null ? (ci.Product.ProductSpecification ?? "") : "",
                    BoxContents = ci.Product != null ? (ci.Product.BoxContents ?? "") : "",
                    CartId = ci.CartId ?? 0,
                    CartItemId = ci.CartItemId,
                })
                .ToListAsync();

            return cartItems.Select(ci => new CartResults
            {
                productID = ci.ProductIdGuid ?? Guid.Empty,
                ProductImage = ci.ProductImage,
                ProductName = ci.ProductName,
                MerchantId = ci.MerchantId,
                Quantity = ci.Quantity,
                price = ci.Price,
                InStock = ci.StockQuantity > 0,
                ProductDescription = ci.ProductDescription,
                KeyFeatures = ci.Features,
                Specification = ci.ProductSpecification,
                Box = ci.BoxContents,
                CartID = ci.CartId,
                CartItemID = ci.CartItemId,
            }).ToList();
        }

        public async Task<IEnumerable<CartResults>> GetBoughtItems(int userId)
        {
            // Convert legacy int userId to ApplicationUserId string
            var applicationUserId = await ConvertLegacyUserIdToApplicationUserId(userId);
            if (string.IsNullOrEmpty(applicationUserId))
            {
                return new List<CartResults>();
            }

            var cartItems = await _dbContext.CartItems.Where(ci => ci.Cart.ApplicationUserId == applicationUserId && ci.IsBought == true)
                .Select(ci => new { 
                    ProductIdGuid = ci.ProductId,
                    ProductName = ci.Product != null ? ci.Product.ProductName : "",
                    ProductImage = ci.Product != null ? (ci.Product.ImageUrls != null ? ci.Product.ImageUrls.FirstOrDefault() : "") : "",
                    MerchantId = ci.Product != null ? ci.Product.MerchantID : Guid.Empty,
                    Quantity = ci.Quantity,
                    Price = ci.Product != null ? ci.Product.Price : 0,
                    StockQuantity = ci.Product != null ? ci.Product.StockQuantity : 0,
                    ProductDescription = ci.Product != null ? (ci.Product.ProductDescription ?? "") : "",
                    Features = ci.Product != null ? (ci.Product.Features ?? "") : "",
                    ProductSpecification = ci.Product != null ? (ci.Product.ProductSpecification ?? "") : "",
                    BoxContents = ci.Product != null ? (ci.Product.BoxContents ?? "") : "",
                    CartId = ci.CartId ?? 0,
                    CartItemId = ci.CartItemId,
                })
                .ToListAsync();

            return cartItems.Select(ci => new CartResults
            {
                productID = ci.ProductIdGuid ?? Guid.Empty,
                ProductImage = ci.ProductImage,
                ProductName = ci.ProductName,
                MerchantId = ci.MerchantId,
                Quantity = ci.Quantity,
                price = ci.Price,
                InStock = ci.StockQuantity > 0,
                ProductDescription = ci.ProductDescription,
                KeyFeatures = ci.Features,
                Specification = ci.ProductSpecification,
                Box = ci.BoxContents,
                CartID = ci.CartId,
                CartItemID = ci.CartItemId,
            }).ToList();
        }

        public async Task<Status> AddToCart(string cartItemsJson)
        {
            // Parse the JSON input
            var json = JsonDocument.Parse(cartItemsJson);
            var userId = json.RootElement.GetProperty("UserID").GetInt32();
            var productId = json.RootElement.GetProperty("ProductID").GetGuid();
            var quantity = json.RootElement.GetProperty("Quantity").GetInt32();

            // Convert legacy int userId to ApplicationUserId string
            var applicationUserId = await ConvertLegacyUserIdToApplicationUserId(userId);
            if (string.IsNullOrEmpty(applicationUserId))
            {
                _logger.LogWarning("User not found for ID: {UserId}", userId);
                return new Status { ResponseCode = 400, ResponseMessage = "User not found" };
            }

            // Get the user for display name
            var identityUser = await _userManager.FindByIdAsync(applicationUserId);
            string userName = identityUser?.DisplayName ?? identityUser?.UserName ?? identityUser?.Email ?? "Unknown User";

            // Check if product exists
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                return new Status { ResponseCode = 400, ResponseMessage = "Product Doesn't Exist" };
            }

            // Get or create the cart for the user
            var cart = await _dbContext.Cart.FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);
            if (cart == null)
            {
                cart = new Models.Cart
                {
                    ApplicationUserId = applicationUserId,
                    CreatedAt = DateTime.UtcNow,
                    CartName = userName
                };
                _dbContext.Cart.Add(cart);
                await _dbContext.SaveChangesAsync(); // Save to get CartID
            }

            // Check if item is already in the cart
            var existingCartItem = await _dbContext.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            if (existingCartItem != null && existingCartItem.IsActive == true && existingCartItem.IsBought == false)
            {
                existingCartItem.Quantity = quantity;
                existingCartItem.UpdatedOn = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();
                return new Status { ResponseCode = 200, ResponseMessage = "Product Updated in cart Successfully" };
            }
            else if (existingCartItem != null && existingCartItem.IsActive == false && existingCartItem.IsBought == true)
            {
                existingCartItem.Quantity = quantity;
                existingCartItem.UpdatedOn = DateTime.UtcNow;
                // Reactivate the CartItem
                existingCartItem.IsActive = true;
                existingCartItem.IsBought = false; // Reset bought status

                await _dbContext.SaveChangesAsync();
                return new Status { ResponseCode = 200, ResponseMessage = "Product has been moved to cart Successfully" };
            }
            else
            {
                var newCartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity,
                    CreatedOn = DateTime.UtcNow,
                    IsActive = true,
                    IsBought = false
                };

                _dbContext.CartItems.Add(newCartItem);
                await _dbContext.SaveChangesAsync();
                return new Status { ResponseCode = 200, ResponseMessage = "Product Added to Cart Successfully" };
            }
        }

        public async Task<SavedItems> SaveItemAsync(SavedItems item)
        {
            // Check if item already exists
            var existingItem = await _dbContext.SavedItems
                .FirstOrDefaultAsync(s => s.ApplicationUserId == item.ApplicationUserId && s.ProductId == item.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity = item.Quantity;
                existingItem.IsActive = true;
                existingItem.SavedOn = DateTime.UtcNow;
            }
            else
            {
                item.SavedOn = DateTime.UtcNow;
                item.IsActive = true;
                _dbContext.SavedItems.Add(item);
            }

            await _dbContext.SaveChangesAsync();
            return item;
        }

        public async Task<bool> RemoveItemAsync(int userId, Guid productId)
        {
            // Convert legacy int userId to ApplicationUserId string
            var applicationUserId = await ConvertLegacyUserIdToApplicationUserId(userId);
            if (string.IsNullOrEmpty(applicationUserId))
            {
                return false;
            }

            var item = await _dbContext.SavedItems
                .FirstOrDefaultAsync(s => s.ApplicationUserId == applicationUserId && s.ProductId == productId);

            if (item != null)
            {
                item.IsActive = false;
                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<SavedItems>> GetSavedItemsAsync(int userId)
        {
            // Convert legacy int userId to ApplicationUserId string
            var applicationUserId = await ConvertLegacyUserIdToApplicationUserId(userId);
            if (string.IsNullOrEmpty(applicationUserId))
            {
                return new List<SavedItems>();
            }

            return await _dbContext.SavedItems
                .Include(s => s.Product)
                .Where(s => s.ApplicationUserId == applicationUserId && s.IsActive)
                .OrderByDescending(s => s.SavedOn)
                .ToListAsync();
        }

        public async Task<SavedItems> GetSavedItemAsync(int userId, Guid productId)
        {
            // Convert legacy int userId to ApplicationUserId string
            var applicationUserId = await ConvertLegacyUserIdToApplicationUserId(userId);
            if (string.IsNullOrEmpty(applicationUserId))
            {
                return null!;
            }

            return await _dbContext.SavedItems
                .FirstOrDefaultAsync(s => s.ApplicationUserId == applicationUserId && s.ProductId == productId && s.IsActive);
        }

        public async Task<Status> DeleteCartItems(CartItemsDTO cartItems)
        {
            try
            {
                // Convert string ProductID to Guid for comparison
                if (!Guid.TryParse(cartItems.ProductID, out Guid productGuid))
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "Invalid Product ID format"
                    };
                }

                var item = await _dbContext.CartItems.FirstOrDefaultAsync(c =>
                    c.CartItemId == cartItems.CartItemID &&
                    c.CartId == cartItems.CartID &&
                    c.ProductId == productGuid);

                // Check if the item exists
                if (item != null)
                {
                    _dbContext.CartItems.Remove(item);
                    await _dbContext.SaveChangesAsync();
                    
                    return new Status
                    {
                        ResponseCode = 200,
                        ResponseMessage = "Item Removed Successfully"
                    };
                }
                else
                {
                    return new Status
                    {
                        ResponseCode = 404,
                        ResponseMessage = "Item not found in cart"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cart item: {CartItemId}", cartItems.CartItemID);
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = $"Internal server error: {ex.Message}"
                };
            }
        }

        // Helper method to convert legacy int UserId to ApplicationUserId string
        private async Task<string?> ConvertLegacyUserIdToApplicationUserId(int legacyUserId)
        {
            try
            {
                // First try to find by legacy user ID (if you stored it in ApplicationUser)
                // Since we removed LegacyUserId, try to find by string conversion
                var userAsString = await _userManager.FindByIdAsync(legacyUserId.ToString());
                if (userAsString != null)
                {
                    return userAsString.Id;
                }

                // If that doesn't work, you might need a different approach
                // This depends on how you migrated your users
                _logger.LogWarning("Could not find ApplicationUser for legacy ID: {LegacyUserId}", legacyUserId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting legacy user ID {LegacyUserId} to ApplicationUserId", legacyUserId);
                return null;
            }
        }
    }
}
