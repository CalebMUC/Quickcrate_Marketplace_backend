using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.Models;
using System.Linq;

namespace Minimart_Api.Repositories.CategoriesRepository
{
    public class CategoryRepos : ICategoryRepos
    {
        private readonly MinimartDBContext _dbContext;
        private readonly ILogger<CategoryRepos> _logger;

        public CategoryRepos(MinimartDBContext dBContext, ILogger<CategoryRepos> logger)
        {
            _dbContext = dBContext;
            _logger = logger;
        }

        public async Task<IEnumerable<Models.Category>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories.Select(c => new Models.Category
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive,
                ParentId = c.ParentId,
                MerchantID = c.MerchantID,
                ImageUrl = c.ImageUrl,
                MetaTitle = c.MetaTitle,
                MetaDescription = c.MetaDescription,
                SortOrder = c.SortOrder,
                ProductCount = c.ProductCount,
                CreatedOn = c.CreatedOn,
                CreatedBy = c.CreatedBy,
                UpdatedOn = c.UpdatedOn,
                UpdatedBy = c.UpdatedBy
            }).ToListAsync();
        }

        public async Task<Models.Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            return await _dbContext.Categories
                .Where(c => c.CategoryId == categoryId)
                .Select(c => new Models.Category
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Slug = c.Slug,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    ParentId = c.ParentId,
                    MerchantID = c.MerchantID,
                    ImageUrl = c.ImageUrl,
                    MetaTitle = c.MetaTitle,
                    MetaDescription = c.MetaDescription,
                    SortOrder = c.SortOrder,
                    ProductCount = c.ProductCount,
                    CreatedOn = c.CreatedOn,
                    CreatedBy = c.CreatedBy,
                    UpdatedOn = c.UpdatedOn,
                    UpdatedBy = c.UpdatedBy
                })
                .FirstOrDefaultAsync();
        }

        // Legacy method for backward compatibility with int parameter
        public async Task<Models.Category?> GetCategoryByIdAsync(int categoryId)
        {
            // Return null for legacy int IDs since we now use Guid
            _logger.LogWarning("Legacy GetCategoryByIdAsync called with int ID: {CategoryId}. This method is deprecated.", categoryId);
            return null;
        }

        public async Task<IEnumerable<Models.Category>> GetNestedCategoriesAsync()
        {
            try
            {
                // Step 1: Fetch all Categories at once
                var allCategories = await _dbContext.Categories
                    .Select(c => new Models.Category
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name,
                        Slug = c.Slug,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        ParentId = c.ParentId,
                        MerchantID = c.MerchantID,
                        ImageUrl = c.ImageUrl,
                        MetaTitle = c.MetaTitle,
                        MetaDescription = c.MetaDescription,
                        SortOrder = c.SortOrder,
                        ProductCount = c.ProductCount,
                        CreatedOn = c.CreatedOn,
                        CreatedBy = c.CreatedBy,
                        UpdatedOn = c.UpdatedOn,
                        UpdatedBy = c.UpdatedBy,
                        Children = new List<Models.Category>() // Initialize empty list for nested categories
                    })
                    .ToListAsync();

                // Step 2: Build the category tree in-memory
                var categoryDictionary = allCategories.ToDictionary(c => c.CategoryId);

                foreach (var category in allCategories)
                {
                    if (category.ParentId.HasValue && categoryDictionary.ContainsKey(category.ParentId.Value))
                    {
                        categoryDictionary[category.ParentId.Value].Children.Add(category);
                    }
                }

                // Step 3: Return only the top-level Categories (where ParentId is NULL)
                return allCategories.Where(c => c.ParentId == null).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching nested Categories.");
                throw;
            }
        }

        public async Task<IEnumerable<CartResults>> GetSubCategory(Guid CategoryId)
        {
            return await _dbContext.Products
                .Where(tp => tp.CategoryId == CategoryId)
                .Select(tp => new CartResults
                {
                    productID = tp.ProductId,
                    ProductName = tp.ProductName,
                    ProductImage = tp.ImageUrls.FirstOrDefault() ?? "",
                    InStock = tp.StockQuantity > 0,
                    price = tp.Price,
                    MerchantId = tp.MerchantID
                })
                .ToListAsync();
        }

        // Legacy support for int parameter
        public async Task<IEnumerable<CartResults>> GetSubCategory(int CategoryId)
        {
            // For legacy support, return a limited set of products since we can't match int to Guid
            var products = await _dbContext.Products
                .Take(50) // Limit for performance
                .Select(tp => new CartResults
                {
                    productID = tp.ProductId,
                    ProductName = tp.ProductName,
                    ProductImage = tp.ImageUrls.FirstOrDefault() ?? "",
                    InStock = tp.StockQuantity > 0,
                    price = tp.Price,
                    MerchantId = tp.MerchantID
                })
                .ToListAsync();

            return products;
        }

        // REMOVED: Add, Update, Delete methods - handled in Merchant System
        /*
        public async Task<Status> AddCategoriesAsync(CategoriesDto categories) { ... }
        public async Task<Status> UpdateCategoriesAsync(CategoriesDto categories) { ... } 
        public async Task<Status> DeleteCategoryAsync(int categoryId) { ... }
        */
    }
}