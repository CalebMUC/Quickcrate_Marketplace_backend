using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Merchants;
using Minimart_Api.Repositories.Merchant;
using Minimart_Api.Services.Merchant;
using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MerchantController : ControllerBase
    {
        private readonly IMerchantService _merchantService;
        private readonly ILogger<MerchantController> _logger;

        public MerchantController(IMerchantService merchantService, ILogger<MerchantController> logger)
        {
            _merchantService = merchantService;
            _logger = logger;
        }

        #region Enhanced Endpoints (Frontend Compatible)

        /// <summary>
        /// Get all merchants with optional filters, pagination, and sorting
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<MerchantsListResponse>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetMerchants(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string sortBy = "businessName",
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting merchants with filters - Search: {Search}, Status: {Status}, Page: {Page}", 
                    search, status, page);

                var filters = new MerchantFilters
                {
                    Search = search,
                    Status = status,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    Page = page,
                    PageSize = pageSize
                };

                var result = await _merchantService.GetMerchantsAsync(filters);

                return Ok(ApiResponse<MerchantsListResponse>.CreateSuccess(result, "Merchants retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchants");
                return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get merchant by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<MerchantDetailDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetMerchantById(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting merchant by ID: {MerchantId}", id);

                var result = await _merchantService.GetMerchantByIdAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResponse.CreateError("Merchant not found"));
                }

                return Ok(ApiResponse<MerchantDetailDto>.CreateSuccess(result, "Merchant retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant {MerchantId}", id);
                return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get pending merchants for approval queue
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<List<MerchantDetailDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPendingMerchants()
        {
            try
            {
                _logger.LogInformation("Getting pending merchants");

                var result = await _merchantService.GetPendingMerchantsAsync();

                return Ok(ApiResponse<List<MerchantDetailDto>>.CreateSuccess(result, "Pending merchants retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending merchants");
                return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get merchant statistics
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<MerchantStatsDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetMerchantStats()
        {
            try
            {
                _logger.LogInformation("Getting merchant statistics");

                var result = await _merchantService.GetMerchantStatsAsync();

                return Ok(ApiResponse<MerchantStatsDto>.CreateSuccess(result, "Merchant statistics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant statistics");
                return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Register a new merchant (enhanced version)
        /// </summary>
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<MerchantDetailDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> RegisterMerchant([FromBody] MerchantRegistrationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse.CreateError("Invalid merchant data", 
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                _logger.LogInformation("Registering new merchant: {BusinessName}", dto.BusinessName);

                var result = await _merchantService.RegisterMerchantAsync(dto);

                return CreatedAtAction(nameof(GetMerchantById), new { id = result.Id }, 
                    ApiResponse<MerchantDetailDto>.CreateSuccess(result, "Merchant registered successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering merchant");
                return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Update merchant information (enhanced version)
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<MerchantDetailDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateMerchant(Guid id, [FromBody] UpdateMerchantDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse.CreateError("Invalid merchant data",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                _logger.LogInformation("Updating merchant: {MerchantId}", id);

                dto.Id = id; // Ensure ID consistency
                var result = await _merchantService.UpdateMerchantAsync(dto);

                if (result == null)
                {
                    return NotFound(ApiResponse.CreateError("Merchant not found"));
                }

                return Ok(ApiResponse<MerchantDetailDto>.CreateSuccess(result, "Merchant updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating merchant {MerchantId}", id);
                return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Approve or reject a merchant (enhanced version)
        /// </summary>
        //[HttpPost("{id:guid}/approve")]
        //[Authorize(Roles = "Admin")]
        //[ProducesResponseType(typeof(ApiResponse<MerchantDetailDto>), 200)]
        //[ProducesResponseType(typeof(ApiResponse), 400)]
        //[ProducesResponseType(typeof(ApiResponse), 404)]
        //[ProducesResponseType(typeof(ApiResponse), 500)]
        //public async Task<IActionResult> ApproveMerchant(Guid id, [FromBody] MerchantApprovalDto approvalData)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ApiResponse.CreateError("Invalid approval data",
        //                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        //        }

        //        _logger.LogInformation("Processing merchant approval: {MerchantId}, Status: {Status}", id, approvalData.Status);

        //        approvalData.MerchantId = id; // Ensure ID consistency
        //        var result = await _merchantService.ApproveMerchantAsync(approvalData);

        //        if (result == null)
        //        {
        //            return NotFound(ApiResponse.CreateError("Merchant not found"));
        //        }

        //        var message = approvalData.Status.ToLower() == "approved" 
        //            ? "Merchant approved successfully" 
        //            : "Merchant rejected successfully";

        //        return Ok(ApiResponse<MerchantDetailDto>.CreateSuccess(result, message));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error processing merchant approval for {MerchantId}", id);
        //        return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
        //    }
        //}

        /// <summary>
        /// Suspend a merchant
        /// </summary>
        [HttpPost("{id:guid}/suspend")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<MerchantDetailDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> SuspendMerchant(Guid id, [FromBody] SuspendMerchantDto suspensionData)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse.CreateError("Invalid suspension data",
                        ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
                }

                _logger.LogInformation("Suspending merchant: {MerchantId}", id);

                suspensionData.MerchantId = id;
                var result = await _merchantService.SuspendMerchantAsync(suspensionData);

                if (result == null)
                {
                    return NotFound(ApiResponse.CreateError("Merchant not found"));
                }

                return Ok(ApiResponse<MerchantDetailDto>.CreateSuccess(result, "Merchant suspended successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending merchant {MerchantId}", id);
                return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reactivate a suspended merchant
        /// </summary>
        [HttpPost("{id:guid}/reactivate")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<MerchantDetailDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ReactivateMerchant(Guid id)
        {
            try
            {
                _logger.LogInformation("Reactivating merchant: {MerchantId}", id);

                var result = await _merchantService.ReactivateMerchantAsync(id);

                if (result == null)
                {
                    return NotFound(ApiResponse.CreateError("Merchant not found"));
                }

                return Ok(ApiResponse<MerchantDetailDto>.CreateSuccess(result, "Merchant reactivated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating merchant {MerchantId}", id);
                return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Delete a merchant (soft delete)
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteMerchant(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting merchant: {MerchantId}", id);

                var result = await _merchantService.DeleteMerchantAsync(id);

                if (!result)
                {
                    return NotFound(ApiResponse.CreateError("Merchant not found"));
                }

                return Ok(ApiResponse.CreateSuccess("Merchant deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting merchant {MerchantId}", id);
                return StatusCode(500, ApiResponse.CreateError($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Legacy Endpoints (Backward Compatibility)

        /// <summary>
        /// Legacy: Add merchant (Maintained for backward compatibility)
        /// </summary>
        [HttpPost("AddMerchant")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddMerchant(MerchantDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Merchant data is null");
                }

                var response = await _merchantService.AddMerchantsAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Legacy: Edit merchant (Maintained for backward compatibility)
        /// </summary>
        [HttpPost("EditMerchant")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditMerchant(EditMerchantDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest("Merchant data is null");
                }

                var response = await _merchantService.EditMerchantAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Legacy: Approve merchant (Maintained for backward compatibility)
        /// </summary>
        [HttpPost("ApproveMerchant")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveMerchant([FromBody] ApproveMerchantRequest request)
        {
            try
            {
                var result = await _merchantService.ApproveMerchantAsync(request.MerchantID);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in ApproveMerchant endpoint for merchant {request.MerchantID}");
                return StatusCode(500, ApiResponse<object>.CreateError("Internal server error occurred"));
            }
        }

        /// <summary>
        /// Legacy: Get merchants (Maintained for backward compatibility)
        /// </summary>
        [HttpGet("GetMerchants")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMerchants()
        {
            try
            {
                var response = await _merchantService.GetMerchantsAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        #endregion
    }
}
