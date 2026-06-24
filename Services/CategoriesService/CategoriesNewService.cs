using Minimart_Api.DTOS.Cart;
using Minimart_Api.Repositories.CategoriesRepository;
using Minimart_Api.Models;

namespace Minimart_Api.Services.CategoriesService
{
    public class CategoriesNewService : ICategoriesService
    {
        private readonly ICategoryRepos _categoryRepos;
        
        public CategoriesNewService(ICategoryRepos categoryRepos) 
        {
            _categoryRepos = categoryRepos;
        }

        public async Task<IEnumerable<Models.Category>> GetAllCategoriesAsync() 
        { 
            return await _categoryRepos.GetAllCategoriesAsync();
        }

        public async Task<IEnumerable<Models.Category>> GetNestedCategoriesAsync()
        {
            return await _categoryRepos.GetNestedCategoriesAsync();
        }

        public async Task<Models.Category?> GetCategoryByIdAsync(Guid categoryId) 
        { 
            return await _categoryRepos.GetCategoryByIdAsync(categoryId);
        }

        // Legacy support method with int parameter
        public async Task<Models.Category?> GetCategoryByIdAsync(int categoryId) 
        { 
            return await _categoryRepos.GetCategoryByIdAsync(categoryId);
        }

        public async Task<IEnumerable<CartResults>> GetSubCategory(Guid categoryId)
        {
            return await _categoryRepos.GetSubCategory(categoryId);
        }

        // Legacy support method with int parameter
        public async Task<IEnumerable<CartResults>> GetSubCategory(int categoryId)
        {
            return await _categoryRepos.GetSubCategory(categoryId);
        }

        // REMOVED: Add, Update, Delete operations - handled in Merchant System
        /*
        public async Task<Status> AddCategoriesAsync(CategoriesDto categories) { ... }
        public async Task<Status> UpdateCategoriesAsync(CategoriesDto categories) { ... }
        public async Task<Status> DeleteCategoryAsync(int categoryId) { ... }
        */
    }
}
