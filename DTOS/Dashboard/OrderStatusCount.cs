namespace Minimart_Api.DTOS.Dashboard
{
    /// <summary>
    /// Order status distribution count
    /// </summary>
    public class OrderStatusCount
    {
        /// <summary>
        /// Status name
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Number of orders with this status
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Percentage of total orders
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// Color code for chart display
        /// </summary>
        public string Color { get; set; } = string.Empty;
    }
}