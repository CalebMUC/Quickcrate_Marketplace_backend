using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;

namespace Minimart_Api.Repositories.Cart
{
    public interface ICartRepo
    {
        Task<IEnumerable<CartResults>> GetCartItems(int UserID);

        Task<IEnumerable<CartResults>> GetBoughtItems(int userId);
        Task<Status> AddToCart(string CartItems); 

        Task<Status> DeleteCartItems(CartItemsDTO CartItems);

        Task<SavedItems> SaveItemAsync(SavedItems item);
        Task<bool> RemoveItemAsync(int userId, Guid productId);
        Task<IEnumerable<SavedItems>> GetSavedItemsAsync(int userId);
        Task<SavedItems> GetSavedItemAsync(int userId, Guid productId);
        //Task<IEnumerable<Products>> GetSavedItems();
        //Task<Status> SaveItems(SaveItemsDTO saveItems);
    }
}
