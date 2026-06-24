namespace Minimart_Api.DTOS.Dashboard
{
    /// <summary>
    /// Top selling product data
    /// </summary>
    public class TopProductDto
    {
        /// <summary>
        /// Product identifier
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Product image URL
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Product price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Total quantity sold
        /// </summary>
        public int QuantitySold { get; set; }

        /// <summary>
        /// Total revenue generated from this product
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Number of orders containing this product
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// Current stock quantity
        /// </summary>
        public int StockQuantity { get; set; }

        /// <summary>
        /// Product category
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }
}