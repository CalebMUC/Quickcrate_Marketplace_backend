using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.SubCategory;

namespace Minimart_Api.Repositories.Category
{
    /// <summary>
    /// Interface for Category repository operations
    /// Repository layer should only handle data access - no business logic
    /// </summary>
    public interface ICategoryRepo
    {
        #region Category Methods

        /// <summary>
        /// Get all categories with pagination and filtering
        /// </summary>
        /// <param name="query">Query parameters for filtering and pagination</param>
        /// <returns>Paged result of categories</returns>
        Task<PagedResultDto<CategoryResponseDto>> GetCategoriesAsync(CategoryQueryDto query);

        /// <summary>
        /// Get a specific category by ID
        /// </summary>
        /// <param name="categoryId">The category ID</param>
        /// <returns>Category details or null if not found</returns>
        Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid categoryId);

        /// <summary>
        /// Get categories by parent ID (for hierarchical display)
        /// </summary>
        /// <param name="parentId">Parent category ID</param>
        /// <returns>List of child categories</returns>
        Task<List<CategoryResponseDto>> GetCategoriesByParentIdAsync(Guid? parentId);

        /// <summary>
        /// Get root categories (categories without parent)
        /// </summary>
        /// <returns>List of root categories</returns>
        Task<List<CategoryResponseDto>> GetRootCategoriesAsync();

        /// <summary>
        /// Get categories by merchant ID
        /// </summary>
        /// <param name="merchantId">Merchant ID</param>
        /// <returns>List of merchant categories</returns>
        Task<List<CategoryResponseDto>> GetCategoriesByMerchantIdAsync(Guid merchantId);

        /// <summary>
        /// Get categories for a specific merchant with pagination and filtering
        /// </summary>
        /// <param name="merchantId">Merchant ID</param>
        /// <param name="query">Query parameters</param>
        /// <returns>Paged result of merchant categories</returns>
        Task<PagedResultDto<CategoryResponseDto>> GetCategoriesAsync(Guid merchantId, CategoryQueryDto query);

        /// <summary>
        /// Get a specific category by ID for a specific merchant
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <returns>Category details</returns>
        /// <exception cref="Exceptions.NotFoundException">When category not found</exception>
        Task<CategoryResponseDto> GetCategoryByIdAsync(Guid categoryId, Guid merchantId);

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="dto">Category creation data</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <param name="userId">User creating the category</param>
        /// <returns>Created category data</returns>
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto, Guid merchantId, string userId);

        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="categoryId">Category ID to update</param>
        /// <param name="dto">Update data</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <param name="userId">User updating the category</param>
        /// <returns>Updated category data</returns>
        /// <exception cref="Exceptions.NotFoundException">When category not found</exception>
        Task<CategoryResponseDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto dto, Guid merchantId, string userId);

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="categoryId">Category ID to delete</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <exception cref="Exceptions.NotFoundException">When category not found</exception>
        /// <exception cref="Exceptions.BadRequestException">When category has dependencies</exception>
        Task DeleteCategoryAsync(Guid categoryId, Guid merchantId);

        #endregion

        #region SubCategory Methods

        /// <summary>
        /// Get subcategories for a specific category and merchant
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <returns>List of subcategories</returns>
        Task<List<SubCategoryResponseDto>> GetSubCategoriesAsync(Guid categoryId, Guid merchantId);



        /// <summary>
        /// Get subcategories for a specific category and merchant
        /// </summary>
        /// <param name="merchantId">Merchant ID</param>
        /// <param name="includeProducts">Whether to include products data</param>
        /// <returns>List of subcategories</returns>
        Task<List<SubCategoryResponseDto>> GetAllSubCategoriesAsync(Guid merchantId, bool includeProducts = false);

        /// <summary>
        /// Get a specific subcategory by ID
        /// </summary>
        /// <param name="subCategoryId">SubCategory ID</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <returns>SubCategory details</returns>
        /// <exception cref="Exceptions.NotFoundException">When subcategory not found</exception>
        Task<SubCategoryResponseDto> GetSubCategoryByIdAsync(Guid subCategoryId, Guid merchantId);

        /// <summary>
        /// Create a new subcategory
        /// </summary>
        /// <param name="categoryId">Parent category ID</param>
        /// <param name="dto">SubCategory creation data</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <param name="userId">User creating the subcategory</param>
        /// <returns>Created subcategory data</returns>
        /// <exception cref="Exceptions.NotFoundException">When parent category not found</exception>
        Task<SubCategoryResponseDto> CreateSubCategoryAsync(Guid categoryId, CreateSubCategoryDto dto, Guid merchantId, string userId);

        /// <summary>
        /// Update an existing subcategory
        /// </summary>
        /// <param name="subCategoryId">SubCategory ID to update</param>
        /// <param name="dto">Update data</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <param name="userId">User updating the subcategory</param>
        /// <returns>Updated subcategory data</returns>
        /// <exception cref="Exceptions.NotFoundException">When subcategory not found</exception>
        Task<SubCategoryResponseDto> UpdateSubCategoryAsync(Guid subCategoryId, UpdateSubCategoryDto dto, Guid merchantId, string userId);

        /// <summary>
        /// Delete a subcategory
        /// </summary>
        /// <param name="subCategoryId">SubCategory ID to delete</param>
        /// <param name="merchantId">Merchant ID</param>
        /// <exception cref="Exceptions.NotFoundException">When subcategory not found</exception>
        /// <exception cref="Exceptions.BadRequestException">When subcategory has dependencies</exception>
        Task DeleteSubCategoryAsync(Guid subCategoryId, Guid merchantId);

        #endregion
    }
}
