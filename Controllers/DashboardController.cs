using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Dashboard;
using Minimart_Api.DTOS.General;
using Minimart_Api.Services.CurrentUserServices;
using Minimart_Api.Services.Dashboard;

namespace Minimart_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _legacyDashboardService;
        private readonly IEnhancedDashboardService _enhancedDashboardService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DashboardController> _logger;
        
        public DashboardController(
            IDashboardService legacyDashboardService,
            IEnhancedDashboardService enhancedDashboardService,
            ICurrentUserService currentUserService,
            ILogger<DashboardController> logger)
        {
            _legacyDashboardService = legacyDashboardService;
            _enhancedDashboardService = enhancedDashboardService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        #region Enhanced Role-Based Dashboard Endpoints

        #region Admin Dashboard Endpoints

        /// <summary>
        /// Get admin dashboard summary - Admin only (UPDATED WITH MERCHANT FOCUS)
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DashboardResponse<AdminDashboardSummary>), 200)]
        [ProducesResponseType(typeof(DashboardResponse), 403)]
        [ProducesResponseType(typeof(DashboardResponse), 500)]
        public async Task<IActionResult> GetAdminDashboard()
        {
            try
            {
                _logger.LogInformation("Admin dashboard requested by user {UserId}", _currentUserService.UserId);
                var summary = await _enhancedDashboardService.GetAdminDashboardSummaryAsync();
                return Ok(DashboardResponse<AdminDashboardSummary>.CreateSuccess(summary, "Admin dashboard retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin dashboard");
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving admin dashboard"));
            }
        }

        #region NEW ADMIN MERCHANT-FOCUSED ENDPOINTS

        /// <summary>
        /// Get top performing merchants - Admin only
        /// </summary>
        //[HttpGet("admin/merchants/top")]
        //[Authorize(Roles = "Admin")]
        //[ProducesResponseType(typeof(DashboardResponse<List<TopMerchantDto>>), 200)]
        //public async Task<IActionResult> GetTopPerformingMerchants([FromQuery] int limit = 10, [FromQuery] string period = "month")
        //{
        //    try
        //    {
        //        var topMerchants = await _enhancedDashboardService.GetTopPerformingMerchantsAsync(limit, period);
        //        return Ok(DashboardResponse<List<TopMerchantDto>>.CreateSuccess(topMerchants, "Top performing merchants retrieved successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving top performing merchants with limit {Limit}, period {Period}", limit, period);
        //        return StatusCode(500, DashboardResponse.CreateError("Error retrieving top merchants"));
        //    }
        //}

        /// <summary>
        /// Get new merchants - Admin only
        /// </summary>
        [HttpGet("admin/merchants/new")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DashboardResponse<List<NewMerchantDto>>), 200)]
        public async Task<IActionResult> GetNewMerchants([FromQuery] int limit = 10, [FromQuery] int daysBack = 30)
        {
            try
            {
                var newMerchants = await _enhancedDashboardService.GetNewMerchantsAsync(limit, daysBack);
                return Ok(DashboardResponse<List<NewMerchantDto>>.CreateSuccess(newMerchants, "New merchants retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving new merchants with limit {Limit}, daysBack {DaysBack}", limit, daysBack);
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving new merchants"));
            }
        }

        /// <summary>
        /// Get merchant revenue statistics - Admin only
        /// </summary>
        //[HttpGet("admin/merchants/revenue-stats")]
        //[Authorize(Roles = "Admin")]
        //[ProducesResponseType(typeof(DashboardResponse<Dictionary<string, object>>), 200)]
        //public async Task<IActionResult> GetMerchantRevenueStats()
        //{
        //    try
        //    {
        //        var revenueStats = await _enhancedDashboardService.GetMerchantRevenueStatsAsync();
        //        return Ok(DashboardResponse<Dictionary<string, object>>.CreateSuccess(revenueStats, "Merchant revenue statistics retrieved successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving merchant revenue statistics");
        //        return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchant revenue stats"));
        //    }
        //}

        /// <summary>
        /// Get platform revenue data - Admin only
        /// </summary>
        [HttpGet("admin/platform/revenue")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DashboardResponse<List<SalesDataPoint>>), 200)]
        public async Task<IActionResult> GetPlatformRevenueData([FromQuery] string period = "month")
        {
            try
            {
                var platformRevenue = await _enhancedDashboardService.GetPlatformRevenueDataAsync(period);
                return Ok(DashboardResponse<List<SalesDataPoint>>.CreateSuccess(platformRevenue, "Platform revenue data retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving platform revenue data for period {Period}", period);
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving platform revenue data"));
            }
        }

        /// <summary>
        /// Get merchant status distribution - Admin only
        /// </summary>
        [HttpGet("admin/merchants/status")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DashboardResponse<Dictionary<string, int>>), 200)]
        public async Task<IActionResult> GetMerchantStatusDistribution()
        {
            try
            {
                var statusDistribution = await _enhancedDashboardService.GetMerchantStatusDistributionAsync();
                return Ok(DashboardResponse<Dictionary<string, int>>.CreateSuccess(statusDistribution, "Merchant status distribution retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant status distribution");
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchant status distribution"));
            }
        }

        /// <summary>
        /// Get merchants by growth rate - Admin only
        /// </summary>
        //[HttpGet("admin/merchants/growth")]
        //[Authorize(Roles = "Admin")]
        //[ProducesResponseType(typeof(DashboardResponse<List<TopMerchantDto>>), 200)]
        //public async Task<IActionResult> GetMerchantsByGrowth([FromQuery] int limit = 10, [FromQuery] string period = "month")
        //{
        //    try
        //    {
        //        var growthMerchants = await _enhancedDashboardService.GetMerchantsByGrowthAsync(limit, period);
        //        return Ok(DashboardResponse<List<TopMerchantDto>>.CreateSuccess(growthMerchants, "Merchants by growth retrieved successfully"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving merchants by growth with limit {Limit}, period {Period}", limit, period);
        //        return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchants by growth"));
        //    }
        //}

        #endregion

        #endregion

        #region Merchant Dashboard Endpoints

        /// <summary>
        /// Get merchant dashboard summary - Merchant only
        /// </summary>
        [HttpGet("merchant")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(DashboardResponse<MerchantDashboardSummary>), 200)]
        [ProducesResponseType(typeof(DashboardResponse), 403)]
        [ProducesResponseType(typeof(DashboardResponse), 500)]
        public async Task<IActionResult> GetMerchantDashboard()
        {
            try
            {
                _logger.LogInformation("Merchant dashboard requested by user {UserId}", _currentUserService.UserId);
                
                if (!_currentUserService.IsMerchant)
                {
                    return Forbid("Access denied. Merchant role required.");
                }

                var merchantId = _currentUserService.MerchantId;
                if (merchantId == Guid.Empty)
                {
                    return BadRequest(DashboardResponse.CreateError("Merchant ID not found for user"));
                }

                var summary = await _enhancedDashboardService.GetMerchantDashboardSummaryAsync(merchantId);
                return Ok(DashboardResponse<MerchantDashboardSummary>.CreateSuccess(summary, "Merchant dashboard retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant dashboard");
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchant dashboard"));
            }
        }

        /// <summary>
        /// Get merchant metrics - Merchant only
        /// </summary>
        [HttpGet("merchant/metrics")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(DashboardResponse<Dictionary<string, object>>), 200)]
        public async Task<IActionResult> GetMerchantMetrics()
        {
            try
            {
                var merchantId = _currentUserService.MerchantId;
                if (merchantId == Guid.Empty)
                {
                    return BadRequest(DashboardResponse.CreateError("Merchant ID not found for user"));
                }

                var metrics = await _enhancedDashboardService.GetMerchantMetricsAsync(merchantId);
                return Ok(DashboardResponse<Dictionary<string, object>>.CreateSuccess(metrics, "Merchant metrics retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant metrics");
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchant metrics"));
            }
        }

        /// <summary>
        /// Get merchant sales data - Merchant only
        /// </summary>
        [HttpGet("merchant/sales")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(DashboardResponse<List<SalesDataPoint>>), 200)]
        public async Task<IActionResult> GetMerchantSalesData([FromQuery] string period = "month")
        {
            try
            {
                var merchantId = _currentUserService.MerchantId;
                if (merchantId == Guid.Empty)
                {
                    return BadRequest(DashboardResponse.CreateError("Merchant ID not found for user"));
                }

                var salesData = await _enhancedDashboardService.GetMerchantSalesDataAsync(merchantId, period);
                return Ok(DashboardResponse<List<SalesDataPoint>>.CreateSuccess(salesData, "Merchant sales data retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant sales data for period {Period}", period);
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchant sales data"));
            }
        }

        /// <summary>
        /// Get merchant order status distribution - Merchant only
        /// </summary>
        [HttpGet("merchant/orders/status-distribution")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(DashboardResponse<List<OrderStatusCount>>), 200)]
        public async Task<IActionResult> GetMerchantOrderStatusDistribution()
        {
            try
            {
                var merchantId = _currentUserService.MerchantId;
                if (merchantId == Guid.Empty)
                {
                    return BadRequest(DashboardResponse.CreateError("Merchant ID not found for user"));
                }

                var statusDistribution = await _enhancedDashboardService.GetMerchantOrderStatusDistributionAsync(merchantId);
                return Ok(DashboardResponse<List<OrderStatusCount>>.CreateSuccess(statusDistribution, "Merchant order status distribution retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant order status distribution");
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchant order status distribution"));
            }
        }

        /// <summary>
        /// Get merchant recent orders - Merchant only
        /// </summary>
        [HttpGet("merchant/orders/recent")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(DashboardResponse<List<RecentOrderDto>>), 200)]
        public async Task<IActionResult> GetMerchantRecentOrders([FromQuery] int limit = 10)
        {
            try
            {
                var merchantId = _currentUserService.MerchantId;
                if (merchantId == Guid.Empty)
                {
                    return BadRequest(DashboardResponse.CreateError("Merchant ID not found for user"));
                }

                var recentOrders = await _enhancedDashboardService.GetMerchantRecentOrdersAsync(merchantId, limit);
                return Ok(DashboardResponse<List<RecentOrderDto>>.CreateSuccess(recentOrders, "Merchant recent orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant recent orders with limit {Limit}", limit);
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchant recent orders"));
            }
        }

        /// <summary>
        /// Get merchant top products - Merchant only
        /// </summary>
        [HttpGet("merchant/products/top")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(DashboardResponse<List<TopProductDto>>), 200)]
        public async Task<IActionResult> GetMerchantTopProducts([FromQuery] int limit = 5, [FromQuery] string period = "month")
        {
            try
            {
                var merchantId = _currentUserService.MerchantId;
                if (merchantId == Guid.Empty)
                {
                    return BadRequest(DashboardResponse.CreateError("Merchant ID not found for user"));
                }

                var topProducts = await _enhancedDashboardService.GetMerchantTopProductsAsync(merchantId, limit, period);
                return Ok(DashboardResponse<List<TopProductDto>>.CreateSuccess(topProducts, "Merchant top products retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant top products with limit {Limit}, period {Period}", limit, period);
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchant top products"));
            }
        }

        /// <summary>
        /// Get merchant payment methods distribution - Merchant only
        /// </summary>
        [HttpGet("merchant/payments/distribution")]
        [Authorize(Roles = "Merchant")]
        [ProducesResponseType(typeof(DashboardResponse<List<PaymentMethodStats>>), 200)]
        public async Task<IActionResult> GetMerchantPaymentMethodsDistribution()
        {
            try
            {
                var merchantId = _currentUserService.MerchantId;
                if (merchantId == Guid.Empty)
                {
                    return BadRequest(DashboardResponse.CreateError("Merchant ID not found for user"));
                }

                var paymentDistribution = await _enhancedDashboardService.GetMerchantPaymentMethodsDistributionAsync(merchantId);
                return Ok(DashboardResponse<List<PaymentMethodStats>>.CreateSuccess(paymentDistribution, "Merchant payment methods distribution retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant payment methods distribution");
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving merchant payment methods distribution"));
            }
        }

        #endregion

        #region Staff Dashboard Endpoints

        /// <summary>
        /// Get staff dashboard summary - Staff only
        /// </summary>
        [HttpGet("staff")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(DashboardResponse<MerchantDashboardSummary>), 200)]
        [ProducesResponseType(typeof(DashboardResponse), 403)]
        [ProducesResponseType(typeof(DashboardResponse), 500)]
        public async Task<IActionResult> GetStaffDashboard()
        {
            try
            {
                _logger.LogInformation("Staff dashboard requested by user {UserId}", _currentUserService.UserId);
                var summary = await _enhancedDashboardService.GetStaffDashboardSummaryAsync();
                return Ok(DashboardResponse<MerchantDashboardSummary>.CreateSuccess(summary, "Staff dashboard retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff dashboard");
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving staff dashboard"));
            }
        }

        /// <summary>
        /// Get staff recent orders requiring attention - Staff only
        /// </summary>
        [HttpGet("staff/orders/attention")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(DashboardResponse<List<RecentOrderDto>>), 200)]
        public async Task<IActionResult> GetStaffRecentOrders([FromQuery] int limit = 10)
        {
            try
            {
                var recentOrders = await _enhancedDashboardService.GetStaffRecentOrdersAsync(limit);
                return Ok(DashboardResponse<List<RecentOrderDto>>.CreateSuccess(recentOrders, "Staff recent orders retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff recent orders with limit {Limit}", limit);
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving staff recent orders"));
            }
        }

        /// <summary>
        /// Get staff order status distribution - Staff only
        /// </summary>
        [HttpGet("staff/orders/status-distribution")]
        [Authorize(Roles = "Staff")]
        [ProducesResponseType(typeof(DashboardResponse<List<OrderStatusCount>>), 200)]
        public async Task<IActionResult> GetStaffOrderStatusDistribution()
        {
            try
            {
                var statusDistribution = await _enhancedDashboardService.GetStaffOrderStatusDistributionAsync();
                return Ok(DashboardResponse<List<OrderStatusCount>>.CreateSuccess(statusDistribution, "Staff order status distribution retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff order status distribution");
                return StatusCode(500, DashboardResponse.CreateError("Error retrieving staff order status distribution"));
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Health check endpoint for dashboard service - Enhanced with new admin features
        /// </summary>
        /// <returns>Service health status</returns>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck()
        {
            return Ok(DashboardResponse<object>.CreateSuccess(new
            {
                service = "Enhanced Unified Dashboard API",
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                version = "3.0.0",
                features = new[] { 
                    "Role-based access", 
                    "Legacy compatibility", 
                    "Admin merchant analytics",
                    "Merchant performance tracking",
                    "Platform revenue insights",
                    "Merchant growth analytics",
                    "Staff order management"
                },
                adminFeatures = new[] {
                    "Top performing merchants",
                    "New merchants tracking", 
                    "Merchant revenue statistics",
                    "Platform revenue analytics",
                    "Merchant status distribution",
                    "Merchant growth tracking"
                },
                merchantFeatures = new[] {
                    "Merchant dashboard summary",
                    "Sales data analytics",
                    "Order status distribution",
                    "Recent orders tracking",
                    "Top products analytics",
                    "Payment methods distribution"
                },
                staffFeatures = new[] {
                    "Staff dashboard summary",
                    "Orders requiring attention",
                    "Order status monitoring"
                },
                endpoints = new {
                    admin = new[] {
                        "/api/dashboard/admin",
                        "/api/dashboard/admin/merchants/top",
                        "/api/dashboard/admin/merchants/new",
                        "/api/dashboard/admin/merchants/revenue-stats",
                        "/api/dashboard/admin/platform/revenue",
                        "/api/dashboard/admin/merchants/status",
                        "/api/dashboard/admin/merchants/growth"
                    },
                    merchant = new[] {
                        "/api/dashboard/merchant",
                        "/api/dashboard/merchant/metrics",
                        "/api/dashboard/merchant/sales",
                        "/api/dashboard/merchant/orders/status-distribution",
                        "/api/dashboard/merchant/orders/recent",
                        "/api/dashboard/merchant/products/top",
                        "/api/dashboard/merchant/payments/distribution"
                    },
                    staff = new[] {
                        "/api/dashboard/staff",
                        "/api/dashboard/staff/orders/attention",
                        "/api/dashboard/staff/orders/status-distribution"
                    }
                },
                authInfo = new
                {
                    userId = _currentUserService.UserId,
                    isAuthenticated = _currentUserService.IsAuthenticated,
                    role = _currentUserService.Role,
                    hasAdminAccess = _currentUserService.IsAdmin,
                    hasMerchantAccess = _currentUserService.IsMerchant,
                    hasStaffAccess = _currentUserService.IsStaff,
                    merchantId = _currentUserService.IsMerchant ? _currentUserService.MerchantId : Guid.Empty
                }
            }, "Enhanced dashboard service with comprehensive analytics for all roles is healthy"));
        }
    }
}