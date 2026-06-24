namespace Minimart_Api.DTOS.Dashboard
{
    /// <summary>
    /// Payment method statistics
    /// </summary>
    public class PaymentMethodStats
    {
        /// <summary>
        /// Payment method identifier
        /// </summary>
        public int PaymentMethodId { get; set; }

        /// <summary>
        /// Payment method name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Payment method description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Payment method icon/image URL
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Total number of transactions
        /// </summary>
        public int TransactionCount { get; set; }

        /// <summary>
        /// Total amount processed through this method
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Percentage of total transactions
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// Average transaction amount
        /// </summary>
        public decimal AverageAmount { get; set; }
    }
}