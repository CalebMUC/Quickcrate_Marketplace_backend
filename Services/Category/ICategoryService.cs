using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.SubCategory;

namespace Minimart_Api.Services.Category
{
    /// <summary>
    /// Interface for Category service that fetches data from external merchant system
    /// </summary>
    public interface ICategoryService
    {
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
        /// <returns>Category details</returns>
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

        Task<PagedResultDto<CategoryResponseDto>> GetCategoriesAsync(
    Guid merchantId,
    CategoryQueryDto query);

        Task<CategoryResponseDto> GetCategoryByIdAsync(Guid CategoryId, Guid merchantId);

        Task<CategoryResponseDto> CreateCategoryAsync(
            CreateCategoryDto dto,
            Guid merchantId,
            string userId);

        Task<CategoryResponseDto> UpdateCategoryAsync(
            Guid categoryId,
            UpdateCategoryDto dto,
            Guid merchantId,
            string userId);

        Task DeleteCategoryAsync(Guid categoryId, Guid merchantId);

        Task<List<SubCategoryResponseDto>> GetSubCategoriesAsync(
            Guid categoryId,
            Guid merchantId);

        Task<List<SubCategoryResponseDto>> GetAllSubCategoriesAsync(Guid merchantId, bool includeProducts = false);

        Task<SubCategoryResponseDto> GetSubCategoryByIdAsync(Guid subCategoryId, Guid merchantId);

        Task<SubCategoryResponseDto> CreateSubCategoryAsync(
            Guid categoryId,
            CreateSubCategoryDto dto,
            Guid merchantId,
            string userId);

        Task<SubCategoryResponseDto> UpdateSubCategoryAsync(
            Guid subCategoryId,
            UpdateSubCategoryDto dto,
            Guid merchantId,
            string userId);

        Task DeleteSubCategoryAsync(Guid subCategoryId, Guid merchantId);
    }
}