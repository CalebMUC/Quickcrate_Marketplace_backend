namespace Minimart_Api.DTOS.Dashboard
{
    /// <summary>
    /// Top performing merchant data for admin dashboard
    /// </summary>
    public class TopMerchantDto
    {
        /// <summary>
        /// Merchant identifier
        /// </summary>
        public Guid MerchantId { get; set; }

        /// <summary>
        /// Business name
        /// </summary>
        public string BusinessName { get; set; } = string.Empty;

        /// <summary>
        /// Merchant name
        /// </summary>
        public string MerchantName { get; set; } = string.Empty;

        /// <summary>
        /// Business category
        /// </summary>
        public string BusinessCategory { get; set; } = string.Empty;

        /// <summary>
        /// Total revenue generated
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Total number of orders
        /// </summary>
        public int TotalOrders { get; set; }

        /// <summary>
        /// Total number of products
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Average order value
        /// </summary>
        public decimal AverageOrderValue { get; set; }

        /// <summary>
        /// Number of active products
        /// </summary>
        public int ActiveProducts { get; set; }

        /// <summary>
        /// Date when merchant joined
        /// </summary>
        public DateTime JoinedDate { get; set; }

        /// <summary>
        /// Last order date
        /// </summary>
        public DateTime? LastOrderDate { get; set; }

        /// <summary>
        /// Merchant status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Growth percentage compared to previous period
        /// </summary>
        public decimal GrowthPercentage { get; set; }

        /// <summary>
        /// Number of customers served
        /// </summary>
        public int CustomersServed { get; set; }
    }
}