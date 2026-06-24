using Minimart_Api.DTOS.Cart;
using Minimart_Api.Models;

namespace Minimart_Api.Repositories.CategoriesRepository
{
    public interface ICategoryRepos
    {
        // Read-only operations for the modern Category system
        Task<IEnumerable<Models.Category>> GetAllCategoriesAsync();
        Task<Models.Category?> GetCategoryByIdAsync(Guid categoryId);  // Primary method with Guid
        Task<Models.Category?> GetCategoryByIdAsync(int categoryId);   // Legacy support with int
        Task<IEnumerable<Models.Category>> GetNestedCategoriesAsync();

        // Support both Guid and int for subcategory lookup
        Task<IEnumerable<CartResults>> GetSubCategory(Guid categoryId);
        Task<IEnumerable<CartResults>> GetSubCategory(int categoryId);   // Legacy support
        
        // REMOVED: Add, Update, Delete operations - handled in Merchant System
        // Task<Status> AddCategoriesAsync(CategoriesDto categories);
        // Task<Status> UpdateCategoriesAsync(CategoriesDto categories);
        // Task<Status> DeleteCategoryAsync(int categoryId);
    }
}
