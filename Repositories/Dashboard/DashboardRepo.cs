using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Dashboard;
using Minimart_Api.Models.Enums;

namespace Minimart_Api.Repositories.Dashboard
{
    public class DashboardRepo : IDashboardRepo
    {
        private readonly MinimartDBContext _context;
        
        public DashboardRepo(MinimartDBContext context) 
        { 
            _context = context; 
        }
        
        public async Task<MerchantDashboardSummary> GetMerchantDashboardSummary(string merchantId)
        {
            try { 
                // Convert merchantId to Guid for consistency
                var merchantGuid = Guid.Parse(merchantId);
                
                // Implementation to fetch dashboard summary from the database
                var now = DateTime.UtcNow;
                
                // FIX: Create UTC DateTimes to avoid PostgreSQL timezone issues
                var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var startOfLastMonth = startOfMonth.AddMonths(-1);

                //revenue collection
                var currentMonthRevenue = await _context.OrderProducts
                    .Where(op => op.MerchantID == merchantGuid && op.CreatedOn >= startOfMonth)
                    .SumAsync(op => op.TotalPrice);

                //revenue collection
                var lastMonthRevenue = await _context.OrderProducts
                    .Where(op => op.MerchantID == merchantGuid && 
                                op.CreatedOn >= startOfLastMonth && 
                                op.CreatedOn < startOfMonth)
                    .SumAsync(op => op.TotalPrice);

                var revenueGrowth = lastMonthRevenue > 0 ?
                    ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0;

                var productStats = await _context.Products
                    .Where(p => p.MerchantID == merchantGuid)
                    .GroupBy(p => 1)
                    .Select(g => new ProductStats
                    {
                        Total = g.Count(),
                        Active = g.Count(p => p.IsActive && p.Status == "Approved"),
                        Pending = g.Count(p => p.Status == "Pending"),
                        Rejected = g.Count(p => p.Status == "Rejected"),
                        NewThisWeek = g.Count(p => p.CreatedOn >= now.AddDays(-7))
                    })
                    .FirstOrDefaultAsync();

                //ORDER STATUS
                var orderStats = await _context.OrderProducts
                    .Where(op => op.MerchantID == merchantGuid)
                    .GroupBy(o => 1)
                    .Select(g => new OrderStats
                    {
                        Total = g.Count(),
                        Pending = g.Count(o => o.Status == OrderStatusEnum.Pending),
                        Processing = g.Count(o => o.Status == OrderStatusEnum.PaymentProcessing),
                        Shipped = g.Count(o => o.Status == OrderStatusEnum.Shipped),
                        Delivered = g.Count(o => o.Status == OrderStatusEnum.Delivered),
                        Cancelled = g.Count(o => o.Status == OrderStatusEnum.Cancelled),
                        TodayOrders = g.Count(o => o.CreatedOn.Date == now.Date),
                        NewSinceLastHour = g.Count(o => o.CreatedOn >= now.AddHours(-1))
                    })
                    .FirstOrDefaultAsync();

                return new MerchantDashboardSummary
                {
                    Revenue = new RevenueStats
                    {
                        Total = currentMonthRevenue,
                        Growth = (double)revenueGrowth,
                        PreviousPeriod = lastMonthRevenue
                    },
                    Products = productStats ?? new ProductStats(),
                    Orders = orderStats ?? new OrderStats()
                };
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                throw new Exception($"Error fetching merchant dashboard summary: {ex.Message}", ex);
            }
        }

        public async Task<List<SalesDataPoint>> GetSalesDataAsync(string merchantId, string period)
        {
            try
            {
                var merchantGuid = Guid.Parse(merchantId);
                var now = DateTime.UtcNow;
                DateTime startDate;
                
                // Determine the date range based on the period - FIX: Ensure UTC DateTimes
                switch (period.ToLower())
                {
                    case "today":
                        startDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
                        break;
                    case "week":
                        startDate = now.AddDays(-7);
                        break;
                    case "month":
                        startDate = now.AddDays(-30);
                        break;
                    case "year":
                        startDate = now.AddDays(-365);
                        break;
                    default:
                        startDate = now.AddDays(-30); // Default to month
                        break;
                }

                var salesData = await _context.OrderProducts
                    .Where(op => op.MerchantID == merchantGuid && op.CreatedOn >= startDate)
                    .GroupBy(op => op.CreatedOn.Date)
                    .Select(g => new SalesDataPoint
                    {
                        Date = g.Key,
                        Amount = g.Sum(op => op.TotalPrice),
                        OrderCount = g.Count(),
                        Label = g.Key.ToString("MMM dd")
                    })
                    .OrderBy(sd => sd.Date)
                    .ToListAsync();

                return salesData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching sales data: {ex.Message}", ex);
            }
        }

        public async Task<List<RecentOrderDto>> GetRecentOrdersAsync(string merchantId, int limit)
        {
            try
            {
                var merchantGuid = Guid.Parse(merchantId);
                
                var recentOrders = await _context.OrderProducts
                    .Include(op => op.Order)
                        .ThenInclude(o => o.User)
                    .Include(op => op.Product)
                    .Where(op => op.MerchantID == merchantGuid)
                    .GroupBy(op => op.OrderID)
                    .Select(g => new RecentOrderDto
                    {
                        OrderId = g.Key,
                        CustomerName = g.First().Order.User != null ? 
                            $"{g.First().Order.User.FirstName} {g.First().Order.User.LastName}".Trim() : 
                            g.First().Order.OrderedBy ?? "Unknown",
                        CustomerEmail = g.First().Order.User != null ? 
                            g.First().Order.User.Email ?? "N/A" : "N/A",
                        TotalAmount = g.Sum(op => op.TotalPrice),
                        Status = GetOrderStatusStatic(g.First().Status), // Use static method
                        OrderDate = g.First().Order.OrderDate,
                        ItemCount = g.Count()
                    })
                    .OrderByDescending(ro => ro.OrderDate)
                    .Take(limit)
                    .ToListAsync();

                return recentOrders;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching recent orders: {ex.Message}", ex);
            }
        }

        public async Task<List<OrderStatusCount>> GetOrderStatusDistributionAsync(string merchantId)
        {
            try
            {
                var merchantGuid = Guid.Parse(merchantId);
                
                var statusCounts = await _context.OrderProducts
                    .Where(op => op.MerchantID == merchantGuid)
                    .GroupBy(op => op.Status)
                    .Select(g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync();

                var totalOrders = statusCounts.Sum(sc => sc.Count);
                
                var statusDistribution = statusCounts.Select(sc => new OrderStatusCount
                {
                    Status = GetOrderStatusStatic(sc.Status), // Use static method
                    Count = sc.Count,
                    Percentage = totalOrders > 0 ? (double)sc.Count / totalOrders * 100 : 0,
                    Color = GetStatusColorStatic(sc.Status) // Use static method
                }).ToList();

                return statusDistribution;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching order status distribution: {ex.Message}", ex);
            }
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(string merchantId, int limit, string period)
        {
            try
            {
                var merchantGuid = Guid.Parse(merchantId);
                var now = DateTime.UtcNow;
                DateTime startDate;
                
                // Determine the date range based on the period
                switch (period.ToLower())
                {
                    case "week":
                        startDate = now.AddDays(-7);
                        break;
                    case "month":
                        startDate = now.AddDays(-30);
                        break;
                    case "year":
                        startDate = now.AddDays(-365);
                        break;
                    default:
                        startDate = now.AddDays(-30); // Default to month
                        break;
                }

                var topProducts = await _context.OrderProducts
                    .Include(op => op.Product)
                        .ThenInclude(p => p.Category)
                    .Where(op => op.MerchantID == merchantGuid && op.CreatedOn >= startDate)
                    .GroupBy(op => op.ProductId)
                    .Select(g => new TopProductDto
                    {
                        ProductId = g.Key,
                        ProductName = g.First().Product.ProductName,
                        ImageUrl = g.First().Product.ImageUrls.FirstOrDefault() ?? "",
                        Price = g.First().Product.Price,
                        QuantitySold = g.Sum(op => op.Quantity),
                        Revenue = g.Sum(op => op.TotalPrice),
                        OrderCount = g.Count(),
                        StockQuantity = g.First().Product.StockQuantity,
                        Category = g.First().Product.Category.Name
                    })
                    .OrderByDescending(tp => tp.Revenue)
                    .Take(limit)
                    .ToListAsync();

                return topProducts;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching top products: {ex.Message}", ex);
            }
        }

        public async Task<List<PaymentMethodStats>> GetPaymentMethodsDistributionAsync(string merchantId)
        {
            try
            {
                var merchantGuid = Guid.Parse(merchantId);
                
                var paymentStats = await _context.OrderProducts
                    .Include(op => op.Order)
                        .ThenInclude(o => o.PaymentDetails)
                            .ThenInclude(pd => pd.PaymentMethod)
                    .Where(op => op.MerchantID == merchantGuid && 
                                op.Order.PaymentDetails != null)
                    .GroupBy(op => new 
                    {
                        op.Order.PaymentDetails.PaymentMethodID,
                        op.Order.PaymentDetails.PaymentMethod.Name,
                        op.Order.PaymentDetails.PaymentMethod.Description,
                        op.Order.PaymentDetails.PaymentMethod.ImageUrl
                    })
                    .Select(g => new
                    {
                        PaymentMethodId = g.Key.PaymentMethodID,
                        Name = g.Key.Name,
                        Description = g.Key.Description,
                        ImageUrl = g.Key.ImageUrl,
                        TransactionCount = g.Count(),
                        TotalAmount = g.Sum(op => op.TotalPrice)
                    })
                    .ToListAsync();

                var totalTransactions = paymentStats.Sum(ps => ps.TransactionCount);
                
                var paymentMethodStats = paymentStats.Select(ps => new PaymentMethodStats
                {
                    PaymentMethodId = ps.PaymentMethodId,
                    Name = ps.Name ?? "Unknown",
                    Description = ps.Description ?? "",
                    ImageUrl = ps.ImageUrl ?? "",
                    TransactionCount = ps.TransactionCount,
                    TotalAmount = ps.TotalAmount,
                    Percentage = totalTransactions > 0 ? (double)ps.TransactionCount / totalTransactions * 100 : 0,
                    AverageAmount = ps.TransactionCount > 0 ? ps.TotalAmount / ps.TransactionCount : 0
                }).ToList();

                return paymentMethodStats;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching payment methods distribution: {ex.Message}", ex);
            }
        }

        #region Helper Methods
        
        private string GetOrderStatus(OrderStatusEnum statusCode)
        {
            return GetOrderStatusStatic(statusCode);
        }

        private string GetStatusColor(OrderStatusEnum statusCode)
        {
            return GetStatusColorStatic(statusCode);
        }

        // Static versions for use in LINQ queries
        private static string GetOrderStatusStatic(OrderStatusEnum statusCode)
        {
            return statusCode switch
            {
                OrderStatusEnum.Pending => "Pending",
                OrderStatusEnum.PaymentProcessing => "Processing", 
                OrderStatusEnum.Paid => "Paid",
                OrderStatusEnum.Shipped => "Shipped",
                OrderStatusEnum.Delivered => "Delivered",
                OrderStatusEnum.Cancelled => "Cancelled",
                OrderStatusEnum.Refunded => "Refunded",
                OrderStatusEnum.Failed => "Failed",
                _ => "Unknown"
            };
        }

        private static string GetStatusColorStatic(OrderStatusEnum statusCode)
        {
            return statusCode switch
            {
                OrderStatusEnum.Pending => "#FFA500", // Orange for Pending
                OrderStatusEnum.PaymentProcessing => "#0066CC", // Blue for Processing
                OrderStatusEnum.Paid => "#00AA00", // Green for Paid
                OrderStatusEnum.Shipped => "#9933CC", // Purple for Shipped
                OrderStatusEnum.Delivered => "#00CC66", // Green for Delivered
                OrderStatusEnum.Cancelled => "#FF3333", // Red for Cancelled
                OrderStatusEnum.Refunded => "#FF9999", // Light Red for Refunded
                OrderStatusEnum.Failed => "#AA0000", // Dark Red for Failed
                _ => "#CCCCCC"   // Gray for Unknown
            };
        }
        
        #endregion
    }
}
