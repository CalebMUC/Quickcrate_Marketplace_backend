using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.SubCategory;
using Minimart_Api.Services.Category;
using Minimart_Api.Services.CurrentUserServices;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger,ICurrentUserService currentUserService)
        {
            _categoryService = categoryService;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Get all categories with pagination and filtering
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResultDto<CategoryResponseDto>>>> GetCategories(
            [FromQuery] CategoryQueryDto query)
        {
            try
            {
                var result = await _categoryService.GetCategoriesAsync(query);
                return Ok(ApiResponse<PagedResultDto<CategoryResponseDto>>.CreateSuccess(
                    result, 
                    "Categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving categories"));
            }
        }

        /// <summary>
        /// Get a specific category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetCategoryById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    return BadRequest(ApiResponse.CreateError("Invalid category ID"));
                }

                var result = await _categoryService.GetCategoryByIdAsync(id);
                
                if (result == null)
                {
                    return NotFound(ApiResponse.CreateError($"Category with ID {id} not found"));
                }

                return Ok(ApiResponse<CategoryResponseDto>.CreateSuccess(
                    result, 
                    "Category retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving the category"));
            }
        }

        /// <summary>
        /// Get root categories (categories without parent)
        /// </summary>
        [HttpGet("root")]
        public async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetRootCategories()
        {
            try
            {
                var result = await _categoryService.GetRootCategoriesAsync();
                return Ok(ApiResponse<List<CategoryResponseDto>>.CreateSuccess(
                    result, 
                    "Root categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving root categories");
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving root categories"));
            }
        }

        /// <summary>
        /// Get categories by parent ID
        /// </summary>
        /// <param name="parentId">Parent category ID</param>
        /// <returns>List of child categories</returns>
        [HttpGet("by-parent/{parentId:guid}")]
        public async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetCategoriesByParentId(Guid parentId)
        {
            try
            {
                if (parentId == Guid.Empty)
                {
                    return BadRequest(ApiResponse.CreateError("Invalid parent category ID"));
                }

                var result = await _categoryService.GetCategoriesByParentIdAsync(parentId);
                return Ok(ApiResponse<List<CategoryResponseDto>>.CreateSuccess(
                    result, 
                    "Categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories by parent {ParentId}", parentId);
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving categories"));
            }
        }

        /// <summary>
        /// Get categories by merchant ID
        /// </summary>
        /// <param name="merchantId">Merchant ID</param>
        /// <returns>List of merchant categories</returns>
        [HttpGet("by-merchant/{merchantId:guid}")]
        public async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetCategoriesByMerchantId(Guid merchantId)
        {
            try
            {
                if (merchantId == Guid.Empty)
                {
                    return BadRequest(ApiResponse.CreateError("Invalid merchant ID"));
                }

                var result = await _categoryService.GetCategoriesByMerchantIdAsync(merchantId);
                return Ok(ApiResponse<List<CategoryResponseDto>>.CreateSuccess(
                    result, 
                    "Merchant categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories by merchant {MerchantId}", merchantId);
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving merchant categories"));
            }
        }

        // Additional endpoints (Create, Update, Delete) can be added here as needed

    

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> CreateCategory(
            [FromBody] CreateCategoryDto dto)
        {
            var merchantId = _currentUserService.MerchantId;
            var userId = _currentUserService.UserId;
            var result = await _categoryService.CreateCategoryAsync(dto, merchantId, userId);

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = result.CategoryId },
                ApiResponse<CategoryResponseDto>.CreateSuccess(result));
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPost("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        public async Task<ActionResult<ApiResponse<CategoryResponseDto>>> UpdateCategory(
            Guid id,
            UpdateCategoryDto dto)
        {
            var merchantId = _currentUserService.MerchantId;
            var userId = _currentUserService.UserId;
            var result = await _categoryService.UpdateCategoryAsync(id, dto, merchantId, userId);
            return Ok(ApiResponse<CategoryResponseDto>.CreateSuccess(result));
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        public async Task<ActionResult<ApiResponse>> DeleteCategory(Guid id)
        {
            var merchantId = _currentUserService.MerchantId;
            await _categoryService.DeleteCategoryAsync(id, merchantId);
            return Ok(ApiResponse.CreateSuccessResponse("Category deleted successfully"));
        }

        /// <summary>
        /// Get subcategories for a specific category
        /// </summary>
        /// 
        [HttpGet("{categoryId}/subcategories")]
        [ProducesResponseType(typeof(ApiResponse<List<SubCategoryResponseDto>>), 200)]
        public async Task<ActionResult<ApiResponse<List<SubCategoryResponseDto>>>> GetSubCategories(
            Guid categoryId)
        {
            var merchantId = _currentUserService.MerchantId;
            var result = await _categoryService.GetSubCategoriesAsync(categoryId, merchantId);
            return Ok(ApiResponse<List<SubCategoryResponseDto>>.CreateSuccess(result));
        }

        /// <summary>
        /// Get All SubCategories for the merchant
        /// </summary>
        /// 
        [HttpGet("subcategories")]
        [ProducesResponseType(typeof(ApiResponse<List<SubCategoryResponseDto>>), 200)]
        public async Task<ActionResult<ApiResponse<List<SubCategoryResponseDto>>>> GetAllSubCategories()
        {

            try
            {
                bool includeProducts = false;
                var merchantId = _currentUserService.MerchantId;
                
                    if (merchantId == Guid.Empty)
                {
                    return BadRequest(ApiResponse.CreateError("Invalid merchant ID"));
                }

                var result = await _categoryService.GetAllSubCategoriesAsync(merchantId, includeProducts);
                return Ok(ApiResponse<List<SubCategoryResponseDto>>.CreateSuccess(
                    result, 
                    "All subcategories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all subcategories for merchant");
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving subcategories"));
            }
        }

        /// <summary>
        /// Create a new subcategory
        /// </summary>
        /// 
        [HttpPost("{categoryId}/subcategories")]
        [ProducesResponseType(typeof(ApiResponse<SubCategoryResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        public async Task<ActionResult<ApiResponse<SubCategoryResponseDto>>> CreateSubCategory(
            Guid categoryId,
            [FromBody] CreateSubCategoryDto dto)
        {
            var merchantId = _currentUserService.MerchantId;
            var userId = _currentUserService.UserId;
            var result = await _categoryService.CreateSubCategoryAsync(categoryId, dto, merchantId, userId);
            return CreatedAtAction(
                nameof(GetSubCategories),
                new { categoryId = categoryId },
                ApiResponse<SubCategoryResponseDto>.CreateSuccess(result));
        }

        /// <summary>
        /// Update an existing subcategory
        /// </summary>
        [HttpPut("{categoryId}/subcategories/{subCategoryId}")]
        [ProducesResponseType(typeof(ApiResponse<SubCategoryResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        public async Task<ActionResult<ApiResponse<SubCategoryResponseDto>>> UpdateSubCategory(
            Guid categoryId,
            Guid subCategoryId,
            [FromBody] UpdateSubCategoryDto dto)
        {
            var merchantId = _currentUserService.MerchantId;
            var userId = _currentUserService.UserId;
            var result = await _categoryService.UpdateSubCategoryAsync(subCategoryId, dto, merchantId, userId);
            return Ok(ApiResponse<SubCategoryResponseDto>.CreateSuccess(result));
        }

        /// <summary>
        /// Delete (soft delete) a subcategory
        /// </summary>
        /// <param name="subCategoryId">The ID of the subcategory to delete</param>
        /// <returns>Success response if deletion is successful</returns>
        /// <remarks>
        /// This endpoint performs a hard delete of a subcategory.
        /// 
        /// **Validation Rules:**
        /// - SubCategory must exist and belong to the current merchant
        /// - SubCategory cannot have associated products
        /// - SubCategory cannot have child sub-subcategories
        /// 
        /// **Sample Request:**
        /// 
        ///     DELETE /api/category/subcategories/{subCategoryId}
        /// 
        /// **Error Responses:**
        /// - 400 Bad Request: Invalid subcategory ID or subcategory has dependencies
        /// - 404 Not Found: SubCategory not found
        /// - 500 Internal Server Error: Unexpected error occurred
        /// </remarks>
        [HttpDelete("subcategories/{subCategoryId}")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<ActionResult<ApiResponse>> DeleteSubCategory(Guid subCategoryId)
        {
            try
            {
                // Validate subcategory ID
                if (subCategoryId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid subcategory ID provided: {SubCategoryId}", subCategoryId);
                    return BadRequest(ApiResponse.CreateError("Invalid subcategory ID"));
                }

                // Get merchant ID from current user
                var merchantId = _currentUserService.MerchantId;

                if (merchantId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid merchant ID for user");
                    return BadRequest(ApiResponse.CreateError("Invalid merchant ID"));
                }

                _logger.LogInformation(
                    "Attempting to delete subcategory {SubCategoryId} for merchant {MerchantId}",
                    subCategoryId,
                    merchantId);

                // Call service to delete subcategory
                await _categoryService.DeleteSubCategoryAsync(subCategoryId, merchantId);

                _logger.LogInformation(
                    "Successfully deleted subcategory {SubCategoryId} for merchant {MerchantId}",
                    subCategoryId,
                    merchantId);

                return Ok(ApiResponse.CreateSuccessResponse("Subcategory deleted successfully"));
            }
            catch (Exceptions.NotFoundException ex)
            {
                _logger.LogWarning(
                    ex,
                    "SubCategory not found. SubCategoryId: {SubCategoryId}",
                    subCategoryId);

                return NotFound(ApiResponse.CreateError(ex.Message));
            }
            catch (Exceptions.BadRequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Bad request when deleting subcategory. SubCategoryId: {SubCategoryId}",
                    subCategoryId);

                return BadRequest(ApiResponse.CreateError(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error deleting subcategory {SubCategoryId}",
                    subCategoryId);

                return StatusCode(500, ApiResponse.CreateError(
                    "An unexpected error occurred while deleting the subcategory"));
            }
        }

    }
}
