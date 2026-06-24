using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;

namespace Minimart_Api.Repositories.Cart
{
    /// <summary>
    /// Modern cart repository interface following best practices
    /// </summary>
    public interface ICartRepository
    {
        // Cart Operations
        Task<CartSummaryDto?> GetCartAsync(string applicationUserId);
        Task<CartSummaryDto> AddToCartAsync(AddToCartDto request);
        Task<CartSummaryDto> UpdateCartItemAsync(UpdateCartItemDto request);
        Task<bool> RemoveFromCartAsync(RemoveFromCartDto request);
        Task<bool> ClearCartAsync(string applicationUserId);
        Task<CartValidationResult> ValidateCartAsync(string applicationUserId);
        
        // Cart Item Operations
        Task<CartItemDto?> GetCartItemAsync(string applicationUserId, Guid productId);
        Task<bool> CartItemExistsAsync(string applicationUserId, Guid productId);
        
        // Saved Items Operations
        Task<IEnumerable<SavedItemDto>> GetSavedItemsAsync(string applicationUserId);
        Task<SavedItemDto> SaveItemAsync(SaveItemRequestDto request);
        Task<bool> RemoveSavedItemAsync(string applicationUserId, Guid productId);
        Task<SavedItemDto?> GetSavedItemAsync(string applicationUserId, Guid productId);
        Task<bool> MoveToCartAsync(string applicationUserId, Guid productId);
        Task<bool> MoveToSavedAsync(string applicationUserId, Guid productId);
        
        // Analytics and Reporting
        Task<int> GetCartItemCountAsync(string applicationUserId);
        Task<decimal> GetCartTotalAsync(string applicationUserId);
        Task<IEnumerable<CartItemDto>> GetRecentCartItemsAsync(string applicationUserId, int limit = 10);
        Task<IEnumerable<CartItemDto>> GetBoughtItemsAsync(string applicationUserId);
        
        // Cleanup Operations
        Task<int> CleanupInactiveCartsAsync(int daysOld = 30);
        Task<int> CleanupExpiredSavedItemsAsync(int daysOld = 90);
    }
}