using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;

namespace Minimart_Api.Services.Cart
{
    public interface ICartService
    {
        // Legacy methods (for backward compatibility)
        Task<Status> AddToCart(string CartItems);
        Task<Status> DeleteCartItems(CartItemsDTO CartItems);
        Task<IEnumerable<CartResults>> GetCartItems(string applicationUserId);
        Task<IEnumerable<CartResults>> GetBoughtItems(string applicationUserId);
        Task<SavedProductsDto> SaveItemAsync(SaveItemDto itemDto);
        Task<bool> RemoveItemAsync(string applicationUserId, string productId);
        Task<IEnumerable<SavedProductsDto>> GetSavedItemsAsync(string applicationUserId);

        // New modern methods
        Task<ApiResponse<CartSummaryDto>> GetCartAsync(string applicationUserId);
        Task<ApiResponse<IEnumerable<CartItemDto>>> GetBoughtItemsAsync(string applicationUserId);
        Task<ApiResponse<CartSummaryDto>> AddToCartAsync(AddToCartDto request);
        Task<ApiResponse<CartSummaryDto>> UpdateCartItemAsync(UpdateCartItemDto request);
        Task<ApiResponse<bool>> RemoveFromCartAsync(RemoveFromCartDto request);
        Task<ApiResponse<bool>> ClearCartAsync(string applicationUserId);
        Task<ApiResponse<CartValidationResult>> ValidateCartAsync(string applicationUserId);
        
        // Quick Cart Operations
        Task<ApiResponse<int>> GetCartItemCountAsync(string applicationUserId);
        Task<ApiResponse<decimal>> GetCartTotalAsync(string applicationUserId);
        Task<ApiResponse<bool>> HasItemInCartAsync(string applicationUserId, Guid productId);
        
        // Modern Saved Items Operations
        Task<ApiResponse<IEnumerable<SavedItemDto>>> GetSavedItemsModernAsync(string applicationUserId);
        Task<ApiResponse<SavedItemDto>> SaveItemModernAsync(SaveItemRequestDto request);
        Task<ApiResponse<bool>> RemoveSavedItemAsync(string applicationUserId, Guid productId);
        Task<ApiResponse<bool>> MoveToCartAsync(string applicationUserId, Guid productId);
        Task<ApiResponse<bool>> MoveToSavedAsync(string applicationUserId, Guid productId);
        
        // Bulk Operations
        Task<ApiResponse<CartSummaryDto>> AddMultipleToCartAsync(string applicationUserId, IEnumerable<AddToCartDto> items);
        Task<ApiResponse<bool>> RemoveMultipleFromCartAsync(string applicationUserId, IEnumerable<Guid> productIds);
        Task<ApiResponse<CartSummaryDto>> UpdateMultipleCartItemsAsync(string applicationUserId, IEnumerable<UpdateCartItemDto> updates);
        
        // Business Logic Operations
        Task<ApiResponse<CartSummaryDto>> MergeCartAsync(string sourceUserId, string targetUserId);
        Task<ApiResponse<bool>> TransferCartAsync(string fromUserId, string toUserId);
        Task<ApiResponse<IEnumerable<CartItemDto>>> GetRecommendedItemsAsync(string applicationUserId);
        
        // Analytics Operations
        Task<ApiResponse<IEnumerable<CartItemDto>>> GetRecentCartItemsAsync(string applicationUserId, int limit = 10);
        Task<ApiResponse<Dictionary<string, object>>> GetCartAnalyticsAsync(string applicationUserId);
    }
}
