using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Dashboard;
using Minimart_Api.Models;

namespace Minimart_Api.Services.Dashboard
{
    public class EnhancedDashboardService : IEnhancedDashboardService
    {
        private readonly MinimartDBContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<EnhancedDashboardService> _logger;

        // Cache keys
        private const string ADMIN_DASHBOARD_KEY = "admin_dashboard_summary";
        private const string MERCHANT_DASHBOARD_KEY = "merchant_dashboard_{0}";
        private const string STAFF_DASHBOARD_KEY = "staff_dashboard_summary";
        
        // Cache duration
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        public EnhancedDashboardService(
            MinimartDBContext context,
            IMemoryCache cache,
            ILogger<EnhancedDashboardService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        #region Admin Dashboard Methods

        public async Task<AdminDashboardSummary> GetAdminDashboardSummaryAsync()
        {
            return await _cache.GetOrCreateAsync("admin_dashboard_enhanced", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                
                _logger.LogInformation("Generating enhanced admin dashboard summary with merchant focus");

                var currentDate = DateTime.UtcNow;
                var lastMonth = currentDate.AddDays(-30);
                var previousMonth = currentDate.AddDays(-60);

                // Platform Revenue Stats
                var totalRevenue = await _context.Orders.SumAsync(o => o.TotalPaymentAmount);
                var monthlyRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= lastMonth)
                    .SumAsync(o => o.TotalPaymentAmount);
                var previousMonthRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= previousMonth && o.OrderDate < lastMonth)
                    .SumAsync(o => o.TotalPaymentAmount);
                var dailyRevenue = await _context.Orders
                 .Where(o => o.OrderDate >= DateTime.UtcNow.Date)
                 .SumAsync(o => o.TotalPaymentAmount);

                // Merchant Stats
                var totalMerchants = await _context.Merchants.CountAsync();
                //var activeMerchants = await _context.Merchants
                //    .Where(m => m.Orders.Any(o => o.OrderDate >= lastMonth))
                //    .CountAsync();
                var newMerchantsThisMonth = await _context.Merchants
                    .Where(m => m.RegistrationDate >= lastMonth)
                    .CountAsync();
                var newMerchantsToday = await _context.Merchants
                  .Where(m => m.RegistrationDate >= DateTime.UtcNow.Date)
                   .CountAsync();

                //var topMerchant = await _context.Merchants
                //    .Where(m => m.Orders.Any())
                //    .OrderByDescending(m => m.Orders.Sum(o => o.TotalPaymentAmount))
                //    .Select(m => new { m.MerchantID, Revenue = m.Orders.Sum(o => o.TotalPaymentAmount) })
                //    .FirstOrDefaultAsync();

                // Order Stats
                var totalOrders = await _context.Orders.CountAsync();
                var monthlyOrders = await _context.Orders.CountAsync(o => o.OrderDate >= lastMonth);
                var dailyOrders = await _context.Orders.CountAsync(o => o.OrderDate >= DateTime.UtcNow.Date);
                var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
                var completedOrders = await _context.Orders.CountAsync(o => o.Status == "Completed" || o.Status == "Delivered");
                var cancelledOrders = await _context.Orders.CountAsync(o => o.Status == "Cancelled");

                // Product Stats
                var totalProducts = await _context.Products.CountAsync(p => !p.IsDeleted);
                var activeProducts = await _context.Products.CountAsync(p => p.IsActive && !p.IsDeleted);
                var newProductsThisMonth = await _context.Products.CountAsync(p => p.CreatedOn >= lastMonth && !p.IsDeleted);
                var pendingProducts = await _context.Products.CountAsync(p => p.Status == "Pending" && !p.IsDeleted);
                var outOfStockProducts = await _context.Products.CountAsync(p => p.StockQuantity <= 0 && !p.IsDeleted);

                // Growth calculations
                var revenueGrowth = previousMonthRevenue > 0 ? (double)((monthlyRevenue - previousMonthRevenue) / previousMonthRevenue * 100) : 0;
                var averageRevenuePerMerchant = totalMerchants > 0 ? totalRevenue / totalMerchants : 0;

                return new AdminDashboardSummary
                {
                    Revenue = new PlatformRevenueStats
                    {
                        TotalRevenue = totalRevenue,
                        MonthlyRevenue = monthlyRevenue,
                        DailyRevenue = dailyRevenue,
                        AverageRevenuePerMerchant = averageRevenuePerMerchant,
                        HighestMonthlyRevenue = monthlyRevenue, // Can be enhanced to get actual highest
                        RevenueGrowthPercentage = (decimal)revenueGrowth,
                        PlatformCommission = totalRevenue * 0.05m // Assuming 5% commission
                    },
                    Merchants = new MerchantPlatformStats
                    {
                        TotalMerchants = totalMerchants,
                        //ActiveMerchants = activeMerchants,
                        NewMerchantsThisMonth = newMerchantsThisMonth,
                        NewMerchantsToday = newMerchantsToday,
                        MerchantGrowthPercentage = totalMerchants > 0 ? (decimal)(newMerchantsThisMonth * 100.0 / totalMerchants) : 0,
                        PendingApprovalMerchants = await _context.Merchants.CountAsync(m => m.Status == "Pending"),
                        SuspendedMerchants = await _context.Merchants.CountAsync(m => m.Status == "Suspended"),
                        AverageProductsPerMerchant = totalMerchants > 0 ? (decimal)(totalProducts * 1.0 / totalMerchants) : 0,
                        //TopPerformingMerchantId = topMerchant?.MerchantID,
                        //TopMerchantRevenue = topMerchant?.Revenue ?? 0
                    },
                    Orders = new PlatformOrderStats
                    {
                        TotalOrders = totalOrders,
                        MonthlyOrders = monthlyOrders,
                        DailyOrders = dailyOrders,
                        PendingOrders = pendingOrders,
                        CompletedOrders = completedOrders,
                        CancelledOrders = cancelledOrders,
                        AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0,
                        OrderCompletionRate = totalOrders > 0 ? (decimal)(completedOrders * 100.0 / totalOrders) : 0
                    },
                    Products = new PlatformProductStats
                    {
                        TotalProducts = totalProducts,
                        ActiveProducts = activeProducts,
                        NewProductsThisMonth = newProductsThisMonth,
                        PendingApprovalProducts = pendingProducts,
                        OutOfStockProducts = outOfStockProducts,
                        MostPopularCategory = await GetMostPopularCategoryAsync(),
                        TotalCategories = await _context.Categories.CountAsync()
                    },
                    Growth = new PlatformGrowthStats
                    {
                        UserGrowthPercentage = (decimal)revenueGrowth, // Simplified
                        OrderGrowthPercentage = (decimal)revenueGrowth, // Simplified 
                        ProductGrowthPercentage = (decimal)revenueGrowth, // Simplified
                        RevenueGrowthPercentage = (decimal)revenueGrowth,
                        CustomerRetentionRate = 85.0m, // Placeholder - would need complex calculation
                        //PlatformHealthScore = CalculatePlatformHealthScore(completedOrders, totalOrders, activeMerchants, totalMerchants)
                        PlatformHealthScore = CalculatePlatformHealthScore(completedOrders, totalOrders, 5, totalMerchants)
                    }
                };
            }) ?? new AdminDashboardSummary();
        }

        public async Task<Dictionary<string, object>> GetAdminMetricsAsync()
        {
            var summary = await GetAdminDashboardSummaryAsync();
            return new Dictionary<string, object>
            {
                ["revenue"] = summary.Revenue,
                ["merchants"] = summary.Merchants,
                ["orders"] = summary.Orders,
                ["products"] = summary.Products,
                ["growth"] = summary.Growth
            };
        }

        public async Task<List<SalesDataPoint>> GetAdminSalesDataAsync(string period = "month")
        {
            var (startDate, groupBy) = GetPeriodParams(period);
            
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SalesDataPoint
                {
                    Date = g.Key,
                    Amount = g.Sum(o => o.TotalPaymentAmount),
                    OrderCount = g.Count(),
                    Label = g.Key.ToString("MMM dd")
                })
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<List<OrderStatusCount>> GetAdminOrderStatusDistributionAsync()
        {
            var totalOrders = await _context.Orders.CountAsync();
            
            return await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusCount
                {
                    Status = g.Key ?? "Unknown",
                    Count = g.Count(),
                    Percentage = totalOrders > 0 ? (double)g.Count() / totalOrders * 100 : 0
                })
                .ToListAsync();
        }

        public async Task<List<RecentOrderDto>> GetAdminRecentOrdersAsync(int limit = 10)
        {
            return await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(limit)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.OrderID,
                    CustomerName = o.OrderedBy ?? "Unknown",
                    TotalAmount = o.TotalPaymentAmount,
                    Status = o.Status ?? "Unknown",
                    OrderDate = o.OrderDate,
                    ItemCount = o.OrderProducts.Count
                })
                .ToListAsync();
        }

        public async Task<List<TopProductDto>> GetAdminTopProductsAsync(int limit = 5, string period = "month")
        {
            var (startDate, _) = GetPeriodParams(period);

            // First, get the aggregated data
            var topProductsData = await _context.OrderProducts
                .Include(op => op.Product)
                .Include(op => op.Order)
                .Where(op => op.Order.OrderDate >= startDate)
                .GroupBy(op => op.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.ProductName,
                    Price = g.First().Product.Price,
                    ImageUrls = g.First().Product.ImageUrls,
                    QuantitySold = g.Sum(op => op.Quantity),
                    Revenue = g.Sum(op => op.TotalPrice),
                    OrderCount = g.Count()
                })
                .OrderByDescending(p => p.Revenue)
                .Take(limit)
                .ToListAsync();

            // Then, map to TopProductDto with safe image URL extraction
            return topProductsData.Select(p => new TopProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName ?? "Unknown",
                Price = p.Price,
                QuantitySold = p.QuantitySold,
                Revenue = p.Revenue,
                OrderCount = p.OrderCount,
                ImageUrl = p.ImageUrls != null && p.ImageUrls.Any() ? p.ImageUrls[0] : ""
            }).ToList();
        }

        public async Task<List<PaymentMethodStats>> GetAdminPaymentMethodsDistributionAsync()
        {
            var totalTransactions = await _context.PaymentDetails.CountAsync();
            
            return await _context.PaymentDetails
                .Join(_context.PaymentMethods,
                    pd => pd.PaymentMethodID,
                    pm => pm.PaymentMethodID,
                    (pd, pm) => new { PaymentDetail = pd, PaymentMethod = pm })
                .GroupBy(x => new { x.PaymentMethod.PaymentMethodID, x.PaymentMethod.Name })
                .Select(g => new PaymentMethodStats
                {
                    PaymentMethodId = g.Key.PaymentMethodID,
                    Name = g.Key.Name ?? "Unknown",
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(x => x.PaymentDetail.Amount),
                    Percentage = totalTransactions > 0 ? (double)g.Count() / totalTransactions * 100 : 0
                })
                .OrderByDescending(p => p.TotalAmount)
                .ToListAsync();
        }

        // NEW ADMIN MERCHANT-FOCUSED METHODS
        //public async Task<List<TopMerchantDto>> GetTopPerformingMerchantsAsync(int limit = 10, string period = "month")
        //{
        //    var (startDate, _) = GetPeriodParams(period);
            
        //    return await _context.Merchants
        //        .Select(m => new
        //        {
        //            Merchant = m,
        //            Revenue = m.Orders
        //                .Where(o => o.OrderDate >= startDate)
        //                .Sum(o => o.TotalPaymentAmount),
        //            OrderCount = m.Orders
        //                .Where(o => o.OrderDate >= startDate)
        //                .Count(),
        //            ProductCount = m.Products.Count(p => !p.IsDeleted),
        //            ActiveProducts = m.Products.Count(p => p.IsActive && !p.IsDeleted),
        //            Customers = m.Orders
        //                .Where(o => o.OrderDate >= startDate)
        //                .Select(o => o.OrderedBy)
        //                .Distinct()
        //                .Count()
        //        })
        //        .Where(x => x.Revenue > 0)
        //        .OrderByDescending(x => x.Revenue)
        //        .Take(limit)
        //        .Select(x => new TopMerchantDto
        //        {
        //            MerchantId = x.Merchant.MerchantID,
        //            BusinessName = x.Merchant.BusinessName ?? "Unknown Business",
        //            MerchantName = x.Merchant.MerchantName ?? "Unknown Merchant",
        //            BusinessCategory = x.Merchant.BusinessCategory ?? "General",
        //            TotalRevenue = x.Revenue,
        //            TotalOrders = x.OrderCount,
        //            TotalProducts = x.ProductCount,
        //            AverageOrderValue = x.OrderCount > 0 ? x.Revenue / x.OrderCount : 0,
        //            ActiveProducts = x.ActiveProducts,
        //            JoinedDate = x.Merchant.RegistrationDate ?? DateTime.MinValue,
        //            LastOrderDate = x.Merchant.Orders.Max(o => (DateTime?)o.OrderDate),
        //            Status = x.Merchant.Status ?? "Active",
        //            GrowthPercentage = 0, // Would need previous period data for comparison
        //            CustomersServed = x.Customers
        //        })
        //        .ToListAsync();
        //}

        public async Task<List<NewMerchantDto>> GetNewMerchantsAsync(int limit = 10, int daysBack = 30)
        {
            var startDate = DateTime.UtcNow.AddDays(-daysBack);
            
            return await _context.Merchants
                .Where(m => m.RegistrationDate >= startDate)
                .OrderByDescending(m => m.RegistrationDate)
                .Take(limit)
                .Select(m => new NewMerchantDto
                {
                    MerchantId = m.MerchantID,
                    BusinessName = m.BusinessName ?? "Unknown Business",
                    MerchantName = m.MerchantName ?? "Unknown Merchant",
                    BusinessCategory = m.BusinessCategory ?? "General",
                    BusinessType = m.BusinessType ?? "Retail",
                    RegistrationDate = m.RegistrationDate ?? DateTime.MinValue,
                    Status = m.Status ?? "Pending",
                    Email = m.Email ?? "",
                    Phone = m.Phone ?? "",
                    Address = m.Address ?? "",
                    DaysSinceRegistration = (int)(DateTime.UtcNow - (m.RegistrationDate ?? DateTime.UtcNow)).TotalDays,
                    ProfileComplete = !string.IsNullOrEmpty(m.BusinessName) && !string.IsNullOrEmpty(m.Email),
                    ProductsAdded = m.Products.Count(p => !p.IsDeleted),
                    //HasMadeSales = m.Orders.Any()
                })
                .ToListAsync();
        }

        //public async Task<Dictionary<string, object>> GetMerchantRevenueStatsAsync()
        //{
        //    var totalRevenue = await _context.Orders.SumAsync(o => o.TotalPaymentAmount);
        //    var merchantCount = await _context.Merchants.CountAsync();
            
        //    //var revenueByMerchant = await _context.Merchants
        //    //    .Select(m => new
        //    //    {
        //    //        MerchantId = m.MerchantID,
        //    //        Revenue = m.Orders.Sum(o => o.TotalPaymentAmount)
        //    //    })
        //    //    .Where(x => x.Revenue > 0)
        //    //    .ToListAsync();

        //    //var topRevenue = revenueByMerchant.Any() ? revenueByMerchant.Max(x => x.Revenue) : 0;
        //    //var averageRevenue = revenueByMerchant.Any() ? revenueByMerchant.Average(x => x.Revenue) : 0;
        //    //var medianRevenue = CalculateMedian(revenueByMerchant.Select(x => x.Revenue).ToList());

        //    return new Dictionary<string, object>
        //    {
        //        ["totalPlatformRevenue"] = totalRevenue,
        //        //["totalActiveMerchants"] = revenueByMerchant.Count,
        //        ["averageRevenuePerMerchant"] = averageRevenue,
        //        ["medianRevenuePerMerchant"] = medianRevenue,
        //        ["topMerchantRevenue"] = topRevenue,
        //        ["revenueDistribution"] = new Dictionary<string, int>
        //        {
        //            ["under1000"] = revenueByMerchant.Count(x => x.Revenue < 1000),
        //            ["1000to5000"] = revenueByMerchant.Count(x => x.Revenue >= 1000 && x.Revenue < 5000),
        //            ["5000to10000"] = revenueByMerchant.Count(x => x.Revenue >= 5000 && x.Revenue < 10000),
        //            ["over10000"] = revenueByMerchant.Count(x => x.Revenue >= 10000)
        //        },
        //        ["platformCommission"] = totalRevenue * 0.05m // 5% commission
        //    };
        //}

        public async Task<List<SalesDataPoint>> GetPlatformRevenueDataAsync(string period = "month")
        {
            var (startDate, groupBy) = GetPeriodParams(period);
            
            return await _context.Orders
                .Where(o => o.OrderDate >= startDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SalesDataPoint
                {
                    Date = g.Key,
                    Amount = g.Sum(o => o.TotalPaymentAmount),
                    OrderCount = g.Count(),
                    Label = g.Key.ToString("MMM dd")
                })
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetMerchantStatusDistributionAsync()
        {
            return await _context.Merchants
                .GroupBy(m => m.Status ?? "Unknown")
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        //public async Task<List<TopMerchantDto>> GetMerchantsByGrowthAsync(int limit = 10, string period = "month")
        //{
        //    var (startDate, _) = GetPeriodParams(period);
        //    var previousPeriodStart = period.ToLower() switch
        //    {
        //        "week" => startDate.AddDays(-7),
        //        "month" => startDate.AddDays(-30),
        //        "year" => startDate.AddYears(-1),
        //        _ => startDate.AddDays(-30)
        //    };

            //var merchantGrowthData = await _context.Merchants
            //    .Select(m => new
            //    {
            //        Merchant = m,
            //        CurrentRevenue = m.Orders
            //            .Where(o => o.OrderDate >= startDate)
            //            .Sum(o => o.TotalPaymentAmount),
            //        PreviousRevenue = m.Orders
            //            .Where(o => o.OrderDate >= previousPeriodStart && o.OrderDate < startDate)
            //            .Sum(o => o.TotalPaymentAmount)
            //    })
            //    .Where(x => x.CurrentRevenue > 0 || x.PreviousRevenue > 0)
            //    .ToListAsync();

            //return merchantGrowthData
        //        .Select(x => new TopMerchantDto
        //        {
        //            MerchantId = x.Merchant.MerchantID,
        //            BusinessName = x.Merchant.BusinessName ?? "Unknown Business",
        //            MerchantName = x.Merchant.MerchantName ?? "Unknown Merchant",
        //            BusinessCategory = x.Merchant.BusinessCategory ?? "General",
        //            TotalRevenue = x.CurrentRevenue,
        //            TotalOrders = x.Merchant.Orders.Count(o => o.OrderDate >= startDate),
        //            TotalProducts = x.Merchant.Products.Count(p => !p.IsDeleted),
        //            AverageOrderValue = x.Merchant.Orders.Any(o => o.OrderDate >= startDate) ? 
        //                x.CurrentRevenue / x.Merchant.Orders.Count(o => o.OrderDate >= startDate) : 0,
        //            ActiveProducts = x.Merchant.Products.Count(p => p.IsActive && !p.IsDeleted),
        //            JoinedDate = x.Merchant.RegistrationDate ?? DateTime.MinValue,
        //            LastOrderDate = x.Merchant.Orders.Max(o => (DateTime?)o.OrderDate),
        //            Status = x.Merchant.Status ?? "Active",
        //            GrowthPercentage = x.PreviousRevenue > 0 ? 
        //                (x.CurrentRevenue - x.PreviousRevenue) / x.PreviousRevenue * 100 : 
        //                (x.CurrentRevenue > 0 ? 100 : 0),
        //            CustomersServed = x.Merchant.Orders
        //                .Where(o => o.OrderDate >= startDate)
        //                .Select(o => o.OrderedBy)
        //                .Distinct()
        //                .Count()
        //        })
        //        .OrderByDescending(x => x.GrowthPercentage)
        //        .Take(limit)
        //        .ToList();
        //}

        #endregion

        #region Merchant Dashboard Methods

        public async Task<MerchantDashboardSummary> GetMerchantDashboardSummaryAsync(Guid merchantId)
        {
            var cacheKey = string.Format(MERCHANT_DASHBOARD_KEY, merchantId);
            
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                
                _logger.LogInformation("Generating merchant dashboard summary for {MerchantId}", merchantId);

                // Get merchant-specific orders
                var merchantOrdersQuery = _context.Orders.Where(o => o.OrderProducts.Any(op => op.MerchantID == merchantId));

                var totalRevenue = await merchantOrdersQuery.SumAsync(o => o.TotalPaymentAmount);
                var lastWeekRevenue = await merchantOrdersQuery
                    .Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-14) && o.OrderDate < DateTime.UtcNow.AddDays(-7))
                    .SumAsync(o => o.TotalPaymentAmount);
                var thisWeekRevenue = await merchantOrdersQuery
                    .Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-7))
                    .SumAsync(o => o.TotalPaymentAmount);

                // Get merchant products
                var merchantProductsQuery = _context.Products.Where(p => p.MerchantID == merchantId && !p.IsDeleted);
                var totalProducts = await merchantProductsQuery.CountAsync();
                var activeProducts = await merchantProductsQuery.CountAsync(p => p.IsActive);
                var pendingProducts = await merchantProductsQuery.CountAsync(p => p.Status == "Pending");

                var totalOrders = await merchantOrdersQuery.CountAsync();
                var pendingOrders = await merchantOrdersQuery.CountAsync(o => o.Status == "Pending");
                var processingOrders = await merchantOrdersQuery.CountAsync(o => o.Status == "Processing");
                var todayOrders = await merchantOrdersQuery.CountAsync(o => o.OrderDate >= DateTime.UtcNow.Date);

                return new MerchantDashboardSummary
                {
                    Revenue = new RevenueStats
                    {
                        Total = totalRevenue,
                        Growth = lastWeekRevenue > 0 ? (double)((thisWeekRevenue - lastWeekRevenue) / lastWeekRevenue * 100) : 0,
                        PreviousPeriod = lastWeekRevenue
                    },
                    Products = new ProductStats
                    {
                        Total = totalProducts,
                        Active = activeProducts,
                        Pending = pendingProducts,
                        NewThisWeek = await merchantProductsQuery.CountAsync(p => p.CreatedOn >= DateTime.UtcNow.AddDays(-7)),
                        Rejected = await merchantProductsQuery.CountAsync(p => p.Status == "Rejected")
                    },
                    Orders = new OrderStats
                    {
                        Total = totalOrders,
                        Pending = pendingOrders,
                        Processing = processingOrders,
                        TodayOrders = todayOrders,
                        Delivered = await merchantOrdersQuery.CountAsync(o => o.Status == "Delivered"),
                        Shipped = await merchantOrdersQuery.CountAsync(o => o.Status == "Shipped"),
                        Cancelled = await merchantOrdersQuery.CountAsync(o => o.Status == "Cancelled"),
                        NewSinceLastHour = await merchantOrdersQuery.CountAsync(o => o.OrderDate >= DateTime.UtcNow.AddHours(-1))
                    }
                };
            }) ?? new MerchantDashboardSummary();
        }

        public async Task<Dictionary<string, object>> GetMerchantMetricsAsync(Guid merchantId)
        {
            var summary = await GetMerchantDashboardSummaryAsync(merchantId);
            return new Dictionary<string, object>
            {
                ["revenue"] = summary.Revenue,
                ["products"] = summary.Products,
                ["orders"] = summary.Orders
            };
        }

        public async Task<List<SalesDataPoint>> GetMerchantSalesDataAsync(Guid merchantId, string period = "month")
        {
            var (startDate, groupBy) = GetPeriodParams(period);
            
            return await _context.Orders
                .Where(o => o.OrderProducts.Any(op => op.MerchantID == merchantId) && o.OrderDate >= startDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new SalesDataPoint
                {
                    Date = g.Key,
                    Amount = g.Sum(o => o.TotalPaymentAmount),
                    OrderCount = g.Count(),
                    Label = g.Key.ToString("MMM dd")
                })
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<List<OrderStatusCount>> GetMerchantOrderStatusDistributionAsync(Guid merchantId)
        {
            var merchantOrders = _context.Orders.Where(o => o.OrderProducts.Any(op => op.MerchantID == merchantId));
            var totalOrders = await merchantOrders.CountAsync();
            
            return await merchantOrders
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusCount
                {
                    Status = g.Key ?? "Unknown",
                    Count = g.Count(),
                    Percentage = totalOrders > 0 ? (double)g.Count() / totalOrders * 100 : 0
                })
                .ToListAsync();
        }

        public async Task<List<RecentOrderDto>> GetMerchantRecentOrdersAsync(Guid merchantId, int limit = 10)
        {
            return await _context.Orders
                .Where(o => o.OrderProducts.Any(op => op.MerchantID == merchantId))
                .OrderByDescending(o => o.OrderDate)
                .Take(limit)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.OrderID,
                    CustomerName = o.OrderedBy ?? "Unknown",
                    TotalAmount = o.TotalPaymentAmount,
                    Status = o.Status ?? "Unknown",
                    OrderDate = o.OrderDate,
                    ItemCount = o.OrderProducts.Count(op => op.MerchantID == merchantId)
                })
                .ToListAsync();
        }

        public async Task<List<TopProductDto>> GetMerchantTopProductsAsync(Guid merchantId, int limit = 5, string period = "month")
        {
            var (startDate, _) = GetPeriodParams(period);

            // First, get the aggregated data without trying to access array elements in GroupBy
            var topProductsData = await _context.OrderProducts
                .Include(op => op.Product)
                .Include(op => op.Order)
                .Where(op => op.MerchantID == merchantId && op.Order.OrderDate >= startDate)
                .GroupBy(op => op.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    ProductName = g.First().Product.ProductName,
                    Price = g.First().Product.Price,
                    ImageUrls = g.First().Product.ImageUrls,
                    QuantitySold = g.Sum(op => op.Quantity),
                    Revenue = g.Sum(op => op.TotalPrice),
                    OrderCount = g.Count()
                })
                .OrderByDescending(p => p.Revenue)
                .Take(limit)
                .ToListAsync();

            // Then, map to TopProductDto with safe image URL extraction
            return topProductsData.Select(p => new TopProductDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName ?? "Unknown",
                Price = p.Price,
                QuantitySold = p.QuantitySold,
                Revenue = p.Revenue,
                OrderCount = p.OrderCount,
                ImageUrl = p.ImageUrls != null && p.ImageUrls.Any() ? p.ImageUrls[0] : ""
            }).ToList();
        }

        public async Task<List<PaymentMethodStats>> GetMerchantPaymentMethodsDistributionAsync(Guid merchantId)
        {
            var merchantOrders = _context.Orders
                .Where(o => o.OrderProducts.Any(op => op.MerchantID == merchantId))
                .Select(o => o.PaymentID)
                .Distinct();

            var totalTransactions = await _context.PaymentDetails
                .Where(pd => merchantOrders.Contains(pd.PaymentID))
                .CountAsync();
            
            return await _context.PaymentDetails
                .Where(pd => merchantOrders.Contains(pd.PaymentID))
                .Join(_context.PaymentMethods,
                    pd => pd.PaymentMethodID,
                    pm => pm.PaymentMethodID,
                    (pd, pm) => new { PaymentDetail = pd, PaymentMethod = pm })
                .GroupBy(x => new { x.PaymentMethod.PaymentMethodID, x.PaymentMethod.Name })
                .Select(g => new PaymentMethodStats
                {
                    PaymentMethodId = g.Key.PaymentMethodID,
                    Name = g.Key.Name ?? "Unknown",
                    TransactionCount = g.Count(),
                    TotalAmount = g.Sum(x => x.PaymentDetail.Amount),
                    Percentage = totalTransactions > 0 ? (double)g.Count() / totalTransactions * 100 : 0
                })
                .OrderByDescending(p => p.TotalAmount)
                .ToListAsync();
        }

        #endregion

        #region Staff Dashboard Methods

        public async Task<MerchantDashboardSummary> GetStaffDashboardSummaryAsync()
        {
            return await _cache.GetOrCreateAsync(STAFF_DASHBOARD_KEY, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                
                _logger.LogInformation("Generating staff dashboard summary");

                // Staff sees orders requiring attention (no revenue data)
                var totalOrders = await _context.Orders.CountAsync();
                var pendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending");
                var processingOrders = await _context.Orders.CountAsync(o => o.Status == "Processing");
                var problemOrders = await _context.Orders.CountAsync(o => new[] { "Cancelled", "Refund Requested", "Disputed" }.Contains(o.Status ?? ""));
                
                var totalProducts = await _context.Products.CountAsync();
                var pendingProducts = await _context.Products.CountAsync(p => p.Status == "Pending" && !p.IsDeleted);

                return new MerchantDashboardSummary
                {
                    Revenue = new RevenueStats { Total = 0 }, // No revenue data for staff
                    Products = new ProductStats
                    {
                        Total = totalProducts,
                        Pending = pendingProducts,
                        Active = await _context.Products.CountAsync(p => p.IsActive && !p.IsDeleted),
                        Rejected = await _context.Products.CountAsync(p => p.Status == "Rejected" && !p.IsDeleted)
                    },
                    Orders = new OrderStats
                    {
                        Total = totalOrders,
                        Pending = pendingOrders,
                        Processing = processingOrders,
                        TodayOrders = await _context.Orders.CountAsync(o => o.OrderDate >= DateTime.Today),
                        Delivered = await _context.Orders.CountAsync(o => o.Status == "Delivered"),
                        Shipped = await _context.Orders.CountAsync(o => o.Status == "Shipped"),
                        Cancelled = await _context.Orders.CountAsync(o => o.Status == "Cancelled"),
                        NewSinceLastHour = problemOrders
                    }
                };
            }) ?? new MerchantDashboardSummary();
        }

        public async Task<List<RecentOrderDto>> GetStaffRecentOrdersAsync(int limit = 10)
        {
            // Staff sees orders requiring attention (pending, cancelled, refunds)
            var problemStatuses = new[] { "Pending", "Cancelled", "Refund Requested", "Disputed" };
            
            return await _context.Orders
                .Where(o => problemStatuses.Contains(o.Status ?? ""))
                .OrderByDescending(o => o.OrderDate)
                .Take(limit)
                .Select(o => new RecentOrderDto
                {
                    OrderId = o.OrderID,
                    CustomerName = o.OrderedBy ?? "Unknown",
                    TotalAmount = o.TotalPaymentAmount,
                    Status = o.Status ?? "Unknown",
                    OrderDate = o.OrderDate,
                    ItemCount = o.OrderProducts.Count
                })
                .ToListAsync();
        }

        public async Task<List<OrderStatusCount>> GetStaffOrderStatusDistributionAsync()
        {
            var totalOrders = await _context.Orders.CountAsync();
            
            return await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusCount
                {
                    Status = g.Key ?? "Unknown",
                    Count = g.Count(),
                    Percentage = totalOrders > 0 ? (double)g.Count() / totalOrders * 100 : 0
                })
                .OrderByDescending(o => o.Count)
                .ToListAsync();
        }

        #endregion

        #region Utility Methods

        public async Task<bool> ValidateMerchantAccessAsync(Guid merchantId, string userId)
        {
            // Simplified for this implementation
            return true;
        }

        public void ClearDashboardCache()
        {
            _cache.Remove(ADMIN_DASHBOARD_KEY);
            _cache.Remove(STAFF_DASHBOARD_KEY);
        }

        public void ClearMerchantDashboardCache(Guid merchantId)
        {
            var cacheKey = string.Format(MERCHANT_DASHBOARD_KEY, merchantId);
            _cache.Remove(cacheKey);
        }

        #endregion

        #region Private Helper Methods

        private (DateTime startDate, string groupBy) GetPeriodParams(string period)
        {
            var now = DateTime.UtcNow;
            return period.ToLower() switch
            {
                "today" => (now.Date, "hour"),
                "week" => (now.AddDays(-7), "day"),
                "month" => (now.AddDays(-30), "day"),
                "year" => (now.AddYears(-1), "month"),
                _ => (now.AddDays(-30), "day")
            };
        }

        private async Task<string> GetMostPopularCategoryAsync()
        {
            var popularCategory = await _context.Categories
                .GroupJoin(_context.Products,
                    c => c.CategoryId,
                    p => p.CategoryId,
                    (c, products) => new { Category = c, ProductCount = products.Count() })
                .OrderByDescending(x => x.ProductCount)
                .Select(x => x.Category.Name)
                .FirstOrDefaultAsync();

            return popularCategory ?? "General";
        }

        private decimal CalculatePlatformHealthScore(int completedOrders, int totalOrders, int activeMerchants, int totalMerchants)
        {
            var completionRate = totalOrders > 0 ? (decimal)completedOrders / totalOrders : 0;
            var merchantActivityRate = totalMerchants > 0 ? (decimal)activeMerchants / totalMerchants : 0;
            
            // Simple health score calculation (0-100)
            return Math.Round((completionRate * 50) + (merchantActivityRate * 50), 1);
        }

        private decimal CalculateMedian(List<decimal> values)
        {
            if (!values.Any()) return 0;
            
            var sorted = values.OrderBy(x => x).ToList();
            int count = sorted.Count;
            
            if (count % 2 == 0)
            {
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2;
            }
            else
            {
                return sorted[count / 2];
            }
        }

        #endregion
    }
}