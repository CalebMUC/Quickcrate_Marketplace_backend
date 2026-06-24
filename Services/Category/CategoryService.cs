using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.SubCategory;
using Minimart_Api.Repositories.Category;

namespace Minimart_Api.Services.Category
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepo _categoryRepo;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepo categoryRepo, ILogger<CategoryService> logger)
        {
            _categoryRepo = categoryRepo;
            _logger = logger;
        }

        public async Task<PagedResultDto<CategoryResponseDto>> GetCategoriesAsync(CategoryQueryDto query)
        {
            try
            {
                return await _categoryRepo.GetCategoriesAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetCategoriesAsync");
                return new PagedResultDto<CategoryResponseDto>();
            }
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid categoryId)
        {
            try
            {
                return await _categoryRepo.GetCategoryByIdAsync(categoryId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetCategoryByIdAsync for categoryId: {CategoryId}", categoryId);
                return null;
            }
        }

        public async Task<List<CategoryResponseDto>> GetCategoriesByParentIdAsync(Guid? parentId)
        {
            try
            {
                return await _categoryRepo.GetCategoriesByParentIdAsync(parentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetCategoriesByParentIdAsync for parentId: {ParentId}", parentId);
                return new List<CategoryResponseDto>();
            }
        }

        public async Task<List<CategoryResponseDto>> GetRootCategoriesAsync()
        {
            try
            {
                return await _categoryRepo.GetRootCategoriesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetRootCategoriesAsync");
                return new List<CategoryResponseDto>();
            }
        }

        public async Task<List<CategoryResponseDto>> GetCategoriesByMerchantIdAsync(Guid merchantId)
        {
            try
            {
                return await _categoryRepo.GetCategoriesByMerchantIdAsync(merchantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetCategoriesByMerchantIdAsync for merchantId: {MerchantId}", merchantId);
                return new List<CategoryResponseDto>();
            }
        }

        public async Task<PagedResultDto<CategoryResponseDto>> GetCategoriesAsync(Guid merchantId, CategoryQueryDto query)
        {
            try
            {
                return await _categoryRepo.GetCategoriesAsync(merchantId, query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetCategoriesAsync for merchantId: {MerchantId}", merchantId);
                return new PagedResultDto<CategoryResponseDto>();
            }
        }

        public async Task<CategoryResponseDto> GetCategoryByIdAsync(Guid categoryId, Guid merchantId)
        {
            try
            {
                return await _categoryRepo.GetCategoryByIdAsync(categoryId, merchantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetCategoryByIdAsync for categoryId: {CategoryId}, merchantId: {MerchantId}", categoryId, merchantId);
                throw;
            }
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto, Guid merchantId, string userId)
        {
            try
            {
                return await _categoryRepo.CreateCategoryAsync(dto, merchantId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.CreateCategoryAsync for merchantId: {MerchantId}", merchantId);
                throw;
            }
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDto dto, Guid merchantId, string userId)
        {
            try
            {
                return await _categoryRepo.UpdateCategoryAsync(categoryId, dto, merchantId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.UpdateCategoryAsync for categoryId: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task DeleteCategoryAsync(Guid categoryId, Guid merchantId)
        {
            try
            {
                await _categoryRepo.DeleteCategoryAsync(categoryId, merchantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.DeleteCategoryAsync for categoryId: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<List<SubCategoryResponseDto>> GetSubCategoriesAsync(Guid categoryId, Guid merchantId)
        {
            try
            {
                return await _categoryRepo.GetSubCategoriesAsync(categoryId, merchantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetSubCategoriesAsync for categoryId: {CategoryId}", categoryId);
                return new List<SubCategoryResponseDto>();
            }
        }

        public async Task<List<SubCategoryResponseDto>> GetAllSubCategoriesAsync(Guid merchantId, bool includeProducts = false)
        {
            try
            {
                return await _categoryRepo.GetAllSubCategoriesAsync(merchantId, includeProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetAllSubCategoriesAsync for merchantId: {MerchantId}", merchantId);
                return new List<SubCategoryResponseDto>();
            }
        }

        public async Task<SubCategoryResponseDto> GetSubCategoryByIdAsync(Guid subCategoryId, Guid merchantId)
        {
            try
            {
                return await _categoryRepo.GetSubCategoryByIdAsync(subCategoryId, merchantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.GetSubCategoryByIdAsync for subCategoryId: {SubCategoryId}", subCategoryId);
                throw;
            }
        }

        public async Task<SubCategoryResponseDto> CreateSubCategoryAsync(Guid categoryId, CreateSubCategoryDto dto, Guid merchantId, string userId)
        {
            try
            {
                return await _categoryRepo.CreateSubCategoryAsync(categoryId, dto, merchantId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.CreateSubCategoryAsync for categoryId: {CategoryId}", categoryId);
                throw;
            }
        }

        public async Task<SubCategoryResponseDto> UpdateSubCategoryAsync(Guid subCategoryId, UpdateSubCategoryDto dto, Guid merchantId, string userId)
        {
            try
            {
                return await _categoryRepo.UpdateSubCategoryAsync(subCategoryId, dto, merchantId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.UpdateSubCategoryAsync for subCategoryId: {SubCategoryId}", subCategoryId);
                throw;
            }
        }

        public async Task DeleteSubCategoryAsync(Guid subCategoryId, Guid merchantId)
        {
            try
            {
                await _categoryRepo.DeleteSubCategoryAsync(subCategoryId, merchantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CategoryService.DeleteSubCategoryAsync for subCategoryId: {SubCategoryId}", subCategoryId);
                throw;
            }
        }
    }
}
