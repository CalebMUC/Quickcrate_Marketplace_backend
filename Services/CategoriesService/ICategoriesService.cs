using Minimart_Api.DTOS.Cart;
using Minimart_Api.Models;

namespace Minimart_Api.Services.CategoriesService
{
    public interface ICategoriesService
    {
        // Read-only operations for the modern Category system
        Task<IEnumerable<Models.Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Models.Category>> GetNestedCategoriesAsync();
        Task<Models.Category?> GetCategoryByIdAsync(Guid categoryId);
        Task<IEnumerable<CartResults>> GetSubCategory(Guid categoryId);
        
        // Legacy support methods (read-only)
        Task<Models.Category?> GetCategoryByIdAsync(int categoryId); // Legacy support with int
        Task<IEnumerable<CartResults>> GetSubCategory(int categoryId); // Legacy support with int
        
        // REMOVED: Add, Update, Delete operations - handled in Merchant System
        // Task<Status> AddCategoriesAsync(CategoriesDto categories);
        // Task<Status> UpdateCategoriesAsync(CategoriesDto categories);
        // Task<Status> DeleteCategoryAsync(int CategoryId);
    }
}
