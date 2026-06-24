namespace Minimart_Api.DTOS.Dashboard
{
    /// <summary>
    /// Admin-specific dashboard summary with merchant-focused statistics
    /// </summary>
    public class AdminDashboardSummary
    {
        /// <summary>
        /// Overall platform revenue statistics
        /// </summary>
        public PlatformRevenueStats Revenue { get; set; } = new();

        /// <summary>
        /// Platform-wide merchant statistics
        /// </summary>
        public MerchantPlatformStats Merchants { get; set; } = new();

        /// <summary>
        /// Platform-wide order statistics
        /// </summary>
        public PlatformOrderStats Orders { get; set; } = new();

        /// <summary>
        /// Platform-wide product statistics
        /// </summary>
        public PlatformProductStats Products { get; set; } = new();

        /// <summary>
        /// Platform growth metrics
        /// </summary>
        public PlatformGrowthStats Growth { get; set; } = new();
    }

    /// <summary>
    /// Platform revenue statistics for admin
    /// </summary>
    public class PlatformRevenueStats
    {
        /// <summary>
        /// Total platform revenue (all time)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Revenue for current month
        /// </summary>
        public decimal MonthlyRevenue { get; set; }

        /// <summary>
        /// Revenue for current day
        /// </summary>
        public decimal DailyRevenue { get; set; }

        /// <summary>
        /// Average revenue per merchant
        /// </summary>
        public decimal AverageRevenuePerMerchant { get; set; }

        /// <summary>
        /// Top revenue generating month
        /// </summary>
        public decimal HighestMonthlyRevenue { get; set; }

        /// <summary>
        /// Revenue growth percentage (month over month)
        /// </summary>
        public decimal RevenueGrowthPercentage { get; set; }

        /// <summary>
        /// Platform commission earned
        /// </summary>
        public decimal PlatformCommission { get; set; }
    }

    /// <summary>
    /// Platform merchant statistics for admin
    /// </summary>
    public class MerchantPlatformStats
    {
        /// <summary>
        /// Total number of merchants
        /// </summary>
        public int TotalMerchants { get; set; }

        /// <summary>
        /// Number of active merchants (made sales in last 30 days)
        /// </summary>
        public int ActiveMerchants { get; set; }

        /// <summary>
        /// Number of new merchants this month
        /// </summary>
        public int NewMerchantsThisMonth { get; set; }

        /// <summary>
        /// Number of new merchants today
        /// </summary>
        public int NewMerchantsToday { get; set; }

        /// <summary>
        /// Merchant growth percentage (month over month)
        /// </summary>
        public decimal MerchantGrowthPercentage { get; set; }

        /// <summary>
        /// Number of merchants pending approval
        /// </summary>
        public int PendingApprovalMerchants { get; set; }

        /// <summary>
        /// Number of suspended merchants
        /// </summary>
        public int SuspendedMerchants { get; set; }

        /// <summary>
        /// Average products per merchant
        /// </summary>
        public decimal AverageProductsPerMerchant { get; set; }

        /// <summary>
        /// Top performing merchant ID
        /// </summary>
        public Guid? TopPerformingMerchantId { get; set; }

        /// <summary>
        /// Top performing merchant revenue
        /// </summary>
        public decimal TopMerchantRevenue { get; set; }
    }

    /// <summary>
    /// Platform order statistics for admin
    /// </summary>
    public class PlatformOrderStats
    {
        /// <summary>
        /// Total number of orders (all time)
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Orders this month
        /// </summary>
        public int MonthlyOrders { get; set; }

        /// <summary>
        /// Orders today
        /// </summary>
        public int DailyOrders { get; set; }

        /// <summary>
        /// Pending orders across all merchants
        /// </summary>
        public int PendingOrders { get; set; }

        /// <summary>
        /// Completed orders this month
        /// </summary>
        public int CompletedOrders { get; set; }

        /// <summary>
        /// Cancelled orders this month
        /// </summary>
        public int CancelledOrders { get; set; }

        /// <summary>
        /// Average order value across platform
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// Order completion rate percentage
        /// </summary>
        public decimal OrderCompletionRate { get; set; }
    }

    /// <summary>
    /// Platform product statistics for admin
    /// </summary>
    public class PlatformProductStats
    {
        /// <summary>
        /// Total products across all merchants
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Active products
        /// </summary>
        public int ActiveProducts { get; set; }

        /// <summary>
        /// Products added this month
        /// </summary>
        public int NewProductsThisMonth { get; set; }

        /// <summary>
        /// Products pending approval
        /// </summary>
        public int PendingApprovalProducts { get; set; }

        /// <summary>
        /// Out of stock products
        /// </summary>
        public int OutOfStockProducts { get; set; }

        /// <summary>
        /// Most popular category
        /// </summary>
        public string MostPopularCategory { get; set; } = string.Empty;

        /// <summary>
        /// Number of categories
        /// </summary>
        public int TotalCategories { get; set; }
    }

    /// <summary>
    /// Platform growth statistics for admin
    /// </summary>
    public class PlatformGrowthStats
    {
        /// <summary>
        /// User growth percentage (month over month)
        /// </summary>
        public decimal UserGrowthPercentage { get; set; }

        /// <summary>
        /// Order growth percentage (month over month)
        /// </summary>
        public decimal OrderGrowthPercentage { get; set; }

        /// <summary>
        /// Product growth percentage (month over month)
        /// </summary>
        public decimal ProductGrowthPercentage { get; set; }

        /// <summary>
        /// Revenue growth percentage (month over month)
        /// </summary>
        public decimal RevenueGrowthPercentage { get; set; }

        /// <summary>
        /// Customer retention rate percentage
        /// </summary>
        public decimal CustomerRetentionRate { get; set; }

        /// <summary>
        /// Platform health score (0-100)
        /// </summary>
        public decimal PlatformHealthScore { get; set; }
    }
}