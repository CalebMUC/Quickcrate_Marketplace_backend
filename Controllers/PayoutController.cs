using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Payouts;
using Minimart_Api.Services.CurrentUserServices;
using Minimart_Api.Services.Payouts;
using System.Security.Claims;

namespace Minimart_Api.Controllers
{
    /// <summary>
    /// Payout management controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PayoutController : ControllerBase
    {
        private readonly IPayoutService _payoutService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<PayoutController> _logger;

        public PayoutController(
            IPayoutService payoutService,
            ICurrentUserService currentUserService,
            ILogger<PayoutController> logger)
        {
            _payoutService = payoutService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        #region Merchant Endpoints

        /// <summary>
        /// Get payout statistics for the current merchant
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> GetMerchantPayoutStats()
        {
            try
            {
                var merchantId = await GetCurrentMerchantIdAsync();
                if (merchantId == null)
                {
                    return BadRequest(new { success = false, message = "Merchant not found" });
                }

                var stats = await _payoutService.GetMerchantPayoutStatsAsync(merchantId.Value);
                
                return Ok(new { 
                    success = true, 
                    message = "Payout statistics retrieved successfully", 
                    data = stats 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant payout stats");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving payout statistics" 
                });
            }
        }

        /// <summary>
        /// Get paginated list of payouts for the current merchant
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> GetMerchantPayouts(
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var merchantId = await GetCurrentMerchantIdAsync();
                if (merchantId == null)
                {
                    return BadRequest(new { success = false, message = "Merchant not found" });
                }

                var result = await _payoutService.GetMerchantPayoutsAsync(
                    merchantId.Value, status, startDate, endDate, page, pageSize);
                
                return Ok(new { 
                    success = true, 
                    message = "Payouts retrieved successfully", 
                    data = result.Data,
                    pagination = new
                    {
                        totalCount = result.TotalCount,
                        page = result.Page,
                        pageSize = result.PageSize,
                        totalPages = result.TotalPages,
                        hasNextPage = result.HasNextPage,
                        hasPreviousPage = result.HasPreviousPage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant payouts");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving payouts" 
                });
            }
        }

        /// <summary>
        /// Get detailed payout information by ID for the current merchant
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> GetMerchantPayout(Guid id)
        {
            try
            {
                var merchantId = await GetCurrentMerchantIdAsync();
                if (merchantId == null)
                {
                    return BadRequest(new { success = false, message = "Merchant not found" });
                }

                var payout = await _payoutService.GetPayoutByIdAsync(id, merchantId.Value);
                if (payout == null)
                {
                    return NotFound(new { success = false, message = "Payout not found" });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Payout retrieved successfully", 
                    data = payout 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout {PayoutId}", id);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving the payout" 
                });
            }
        }

        /// <summary>
        /// Get payout transactions for the current merchant
        /// </summary>
        [HttpGet("transactions")]
        [Authorize(Roles = "Merchant")]
        public async Task<IActionResult> GetMerchantTransactions(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? payoutStatus = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var merchantId = await GetCurrentMerchantIdAsync();
                if (merchantId == null)
                {
                    return BadRequest(new { success = false, message = "Merchant not found" });
                }

                var result = await _payoutService.GetMerchantTransactionsAsync(
                    merchantId.Value, startDate, endDate, payoutStatus, page, pageSize);
                
                return Ok(new { 
                    success = true, 
                    message = "Transactions retrieved successfully", 
                    data = result.Data,
                    pagination = new
                    {
                        totalCount = result.TotalCount,
                        page = result.Page,
                        pageSize = result.PageSize,
                        totalPages = result.TotalPages,
                        hasNextPage = result.HasNextPage,
                        hasPreviousPage = result.HasPreviousPage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant transactions");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving transactions" 
                });
            }
        }

        #endregion

        #region Admin Endpoints

        /// <summary>
        /// Get all payouts with filters (Admin only)
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPayouts([FromQuery] PayoutFilters filters)
        {
            try
            {
                var result = await _payoutService.GetAllPayoutsAsync(filters);
                
                return Ok(new { 
                    success = true, 
                    message = "Payouts retrieved successfully", 
                    data = result.Data,
                    pagination = new
                    {
                        totalCount = result.TotalCount,
                        page = result.Page,
                        pageSize = result.PageSize,
                        totalPages = result.TotalPages,
                        hasNextPage = result.HasNextPage,
                        hasPreviousPage = result.HasPreviousPage
                    },
                    filters = filters
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all payouts");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving payouts" 
                });
            }
        }

        /// <summary>
        /// Get detailed payout information by ID (Admin only)
        /// </summary>
        [HttpGet("admin/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPayoutById(Guid id)
        {
            try
            {
                var payout = await _payoutService.GetPayoutByIdAsync(id);
                if (payout == null)
                {
                    return NotFound(new { success = false, message = "Payout not found" });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Payout retrieved successfully", 
                    data = payout 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout {PayoutId}", id);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving the payout" 
                });
            }
        }

        /// <summary>
        /// Generate payouts for a specific period (Admin only)
        /// </summary>
        [HttpPost("admin/generate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GeneratePayouts([FromBody] GeneratePayoutsRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Invalid request data", 
                        errors = ModelState 
                    });
                }

                _logger.LogInformation("Admin generating payouts for period {Start} - {End}", 
                    request.PeriodStartDate, request.PeriodEndDate);

                var result = await _payoutService.GenerateWeeklyPayoutsAsync(request);
                
                if (result.Success)
                {
                    return Ok(new { 
                        success = true, 
                        message = result.Message, 
                        data = result 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = result.Message, 
                        errors = result.Errors 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payouts");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while generating payouts" 
                });
            }
        }

        /// <summary>
        /// Update payout status (Admin only)
        /// </summary>
        [HttpPatch("admin/{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePayoutStatus(Guid id, [FromBody] UpdatePayoutStatusRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Invalid request data", 
                        errors = ModelState 
                    });
                }

                var userId = GetCurrentUserId();
                var success = await _payoutService.UpdatePayoutStatusAsync(id, request, userId);
                
                if (success)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Payout status updated successfully" 
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Failed to update payout status. Please check the payout exists and status transition is valid." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payout status for {PayoutId}", id);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while updating payout status" 
                });
            }
        }

        /// <summary>
        /// Get merchant payout summaries (Admin dashboard)
        /// </summary>
        [HttpGet("admin/merchant-summaries")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMerchantPayoutSummaries(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var result = await _payoutService.GetMerchantPayoutSummariesAsync(page, pageSize, searchTerm);
                
                return Ok(new { 
                    success = true, 
                    message = "Merchant payout summaries retrieved successfully", 
                    data = result.Data,
                    pagination = new
                    {
                        totalCount = result.TotalCount,
                        page = result.Page,
                        pageSize = result.PageSize,
                        totalPages = result.TotalPages,
                        hasNextPage = result.HasNextPage,
                        hasPreviousPage = result.HasPreviousPage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant payout summaries");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving merchant payout summaries" 
                });
            }
        }

        #endregion

        #region Utility Endpoints

        /// <summary>
        /// Get available payout statuses
        /// </summary>
        [HttpGet("statuses")]
        public IActionResult GetPayoutStatuses()
        {
            var statuses = new[]
            {
                new { value = "Pending", label = "Pending", description = "Generated, not yet scheduled" },
                new { value = "Scheduled", label = "Scheduled", description = "Payment date assigned" },
                new { value = "Processing", label = "Processing", description = "Payment execution started" },
                new { value = "Completed", label = "Completed", description = "Funds successfully sent" },
                new { value = "Failed", label = "Failed", description = "Payment failed" },
                new { value = "Cancelled", label = "Cancelled", description = "Admin-cancelled payout" }
            };

            return Ok(new { 
                success = true, 
                message = "Payout statuses retrieved successfully", 
                data = statuses 
            });
        }

        #endregion

        #region Private Helper Methods

        private async Task<Guid?> GetCurrentMerchantIdAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return null;

                var merchantId = _currentUserService.MerchantId;
                return merchantId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current merchant ID");
                return null;
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("userId")?.Value ?? 
                   User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                   string.Empty;
        }

        #endregion
    }
}