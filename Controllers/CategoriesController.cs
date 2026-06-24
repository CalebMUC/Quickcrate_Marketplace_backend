using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.General;
using Minimart_Api.Services.Category;

namespace Minimart_Api.Controllers
{
    /// <summary>
    /// Controller for managing categories
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories with pagination and filtering
        /// </summary>
        /// <param name="query">Query parameters for filtering and pagination</param>
        /// <returns>Paginated list of categories</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResultDto<CategoryResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        private async Task<ActionResult<ApiResponse<PagedResultDto<CategoryResponseDto>>>> GetCategories(
            [FromQuery] CategoryQueryDto query)
        {
            try
            {
                _logger.LogInformation("Getting categories with query: {@Query}", query);

                var result = await _categoryService.GetCategoriesAsync(query);
                
                return Ok(ApiResponse<PagedResultDto<CategoryResponseDto>>.CreateSuccess(
                    result, 
                    "Categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting categories");
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving categories"));
            }
        }

        /// <summary>
        /// Get a specific category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        private async Task<ActionResult<ApiResponse<CategoryResponseDto>>> GetCategoryById(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting category by ID: {CategoryId}", id);

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
                _logger.LogError(ex, "Error occurred while getting category {CategoryId}", id);
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving the category"));
            }
        }

        /// <summary>
        /// Get root categories (categories without parent)
        /// </summary>
        /// <returns>List of root categories</returns>
        [HttpGet("root")]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        private async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetRootCategories()
        {
            try
            {
                _logger.LogInformation("Getting root categories");

                var result = await _categoryService.GetRootCategoriesAsync();
                
                return Ok(ApiResponse<List<CategoryResponseDto>>.CreateSuccess(
                    result, 
                    "Root categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting root categories");
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving root categories"));
            }
        }

        /// <summary>
        /// Get categories by parent ID
        /// </summary>
        /// <param name="parentId">Parent category ID</param>
        /// <returns>List of child categories</returns>
        [HttpGet("by-parent/{parentId}")]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryResponseDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        private async Task<ActionResult<ApiResponse<List<CategoryResponseDto>>>> GetCategoriesByParentId(Guid parentId)
        {
            try
            {
                _logger.LogInformation("Getting categories by parent ID: {ParentId}", parentId);

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
                _logger.LogError(ex, "Error occurred while getting categories by parent ID {ParentId}", parentId);
                return StatusCode(500, ApiResponse.CreateError("An error occurred while retrieving categories"));
            }
        }
    }
}