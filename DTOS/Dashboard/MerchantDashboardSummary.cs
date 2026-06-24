using Minimart_Api.Models;

namespace Minimart_Api.DTOS.Dashboard
{
    /// <summary>
    /// Dashboard summary statistics for a merchant
    /// </summary>
    public class MerchantDashboardSummary
    {
        public RevenueStats Revenue { get; set; } = new();
        public ProductStats Products { get; set; } = new();
        public OrderStats Orders { get; set; } = new();
    }

    /// <summary>
    /// Revenue statistics
    /// </summary>
    public class RevenueStats
    {
        /// <summary>
        /// Total revenue for current period
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Growth percentage compared to previous period
        /// </summary>
        public double Growth { get; set; }

        /// <summary>
        /// Revenue from previous period
        /// </summary>
        public decimal PreviousPeriod { get; set; }
    }

    /// <summary>
    /// Product statistics
    /// </summary>
    public class ProductStats
    {
        /// <summary>
        /// Total number of products
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Number of active approved products
        /// </summary>
        public int Active { get; set; }

        /// <summary>
        /// Number of pending approval products
        /// </summary>
        public int Pending { get; set; }

        /// <summary>
        /// Number of rejected products
        /// </summary>
        public int Rejected { get; set; }

        /// <summary>
        /// Number of new products added this week
        /// </summary>
        public int NewThisWeek { get; set; }
    }

    /// <summary>
    /// Order statistics
    /// </summary>
    public class OrderStats
    {
        /// <summary>
        /// Total number of orders
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Number of pending orders
        /// </summary>
        public int Pending { get; set; }

        /// <summary>
        /// Number of processing orders
        /// </summary>
        public int Processing { get; set; }

        /// <summary>
        /// Number of shipped orders
        /// </summary>
        public int Shipped { get; set; }

        /// <summary>
        /// Number of delivered orders
        /// </summary>
        public int Delivered { get; set; }

        /// <summary>
        /// Number of cancelled orders
        /// </summary>
        public int Cancelled { get; set; }

        /// <summary>
        /// Number of orders placed today
        /// </summary>
        public int TodayOrders { get; set; }

        /// <summary>
        /// Number of new orders in the last hour
        /// </summary>
        public int NewSinceLastHour { get; set; }
    }
}
