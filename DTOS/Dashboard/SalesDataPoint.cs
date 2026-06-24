namespace Minimart_Api.DTOS.Dashboard
{
    /// <summary>
    /// Sales data point for charts and analytics
    /// </summary>
    public class SalesDataPoint
    {
        /// <summary>
        /// Date of the sales data
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Total sales amount for the date
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Number of orders for the date
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// Formatted date label for display
        /// </summary>
        public string Label { get; set; } = string.Empty;
    }
}