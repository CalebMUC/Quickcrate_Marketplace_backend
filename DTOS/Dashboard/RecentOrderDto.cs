namespace Minimart_Api.DTOS.Dashboard
{
    /// <summary>
    /// Recent order data for dashboard display
    /// </summary>
    public class RecentOrderDto
    {
        /// <summary>
        /// Order identifier
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Customer information
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Customer email
        /// </summary>
        public string CustomerEmail { get; set; } = string.Empty;

        /// <summary>
        /// Total order amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Current order status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Date when the order was placed
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Number of items in the order
        /// </summary>
        public int ItemCount { get; set; }
    }
}