using Minimart_Api.Models.Enums;

namespace Minimart_Api.DTOS.Orders
{
    /// <summary>
    /// Response showing detailed progress of an order with all product statuses
    /// </summary>
    public class OrderProgressResponse
    {
        /// <summary>
        /// Order identifier
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Overall order status
        /// </summary>
        public string OverallStatus { get; set; } = string.Empty;

        /// <summary>
        /// Overall order status enum
        /// </summary>
        public OrderStatusEnum OverallStatusEnum { get; set; }

        /// <summary>
        /// Descriptive status message
        /// </summary>
        public string StatusMessage { get; set; } = string.Empty;

        /// <summary>
        /// Total number of products in the order
        /// </summary>
        public int TotalProducts { get; set; }

        /// <summary>
        /// Number of products delivered
        /// </summary>
        public int DeliveredProducts { get; set; }

        /// <summary>
        /// Number of products shipped
        /// </summary>
        public int ShippedProducts { get; set; }

        /// <summary>
        /// Number of products being processed
        /// </summary>
        public int ProcessingProducts { get; set; }

        /// <summary>
        /// Overall progress percentage (0-100)
        /// </summary>
        public double ProgressPercentage { get; set; }

        /// <summary>
        /// Individual product progress details
        /// </summary>
        public List<ProductProgressDto> ProductProgress { get; set; } = new();

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Estimated delivery date (latest among all products)
        /// </summary>
        public DateTime? EstimatedDeliveryDate { get; set; }
    }

    /// <summary>
    /// Progress details for individual product in an order
    /// </summary>
    public class ProductProgressDto
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
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Current status of the product
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Current status enum
        /// </summary>
        public OrderStatusEnum StatusEnum { get; set; }

        /// <summary>
        /// Last status update timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Tracking ID for this product
        /// </summary>
        public string? TrackingId { get; set; }

        /// <summary>
        /// Expected delivery date for this product
        /// </summary>
        public DateTime? ExpectedDeliveryDate { get; set; }

        /// <summary>
        /// Shipping carrier
        /// </summary>
        public string? Carrier { get; set; }

        /// <summary>
        /// Current location of the product
        /// </summary>
        public string? CurrentLocation { get; set; }
    }

    /// <summary>
    /// Request to update multiple product statuses at once
    /// </summary>
    public class BulkStatusUpdateRequest
    {
        /// <summary>
        /// Order ID
        /// </summary>
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Product status updates
        /// </summary>
        public List<ProductStatusUpdate> ProductUpdates { get; set; } = new();

        /// <summary>
        /// User making the update
        /// </summary>
        public string UpdatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Individual product status update
    /// </summary>
    public class ProductStatusUpdate
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Tracking ID
        /// </summary>
        public string TrackingId { get; set; } = string.Empty;

        /// <summary>
        /// New status ID
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Optional tracking notes
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Optional carrier information
        /// </summary>
        public string? Carrier { get; set; }

        /// <summary>
        /// Optional location information
        /// </summary>
        public string? Location { get; set; }
    }
}