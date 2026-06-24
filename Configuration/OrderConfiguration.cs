namespace Minimart_Api.Configuration
{
    /// <summary>
    /// Order system configuration and constants
    /// </summary>
    public static class OrderConstants
    {
        // Order Status IDs
        public const int STATUS_PENDING = 1;
        public const int STATUS_PROCESSING = 2;
        public const int STATUS_SHIPPED = 3;
        public const int STATUS_DELIVERED = 4;
        public const int STATUS_CANCELLED = 6;

        // Business Rules
        public const decimal DEFAULT_DELIVERY_FEE_PER_MERCHANT = 200m;
        public const decimal VAT_RATE = 0.16m;
        public const int DEFAULT_DELIVERY_DAYS = 3;
        public const int MAX_ITEMS_PER_ORDER = 50;
        public const decimal MAX_ORDER_VALUE = 1000000m;

        // Order ID Generation
        public const string ORDER_PREFIX = "ORD_";
        public const string TRACKING_PREFIX = "TRK_";

        // Error Messages
        public const string ERROR_ORDER_NOT_FOUND = "Order not found";
        public const string ERROR_PRODUCT_NOT_FOUND = "Product not found";
        public const string ERROR_PRODUCT_UNAVAILABLE = "Product is not available";
        public const string ERROR_INVALID_QUANTITY = "Invalid quantity specified";
        public const string ERROR_ORDER_ALREADY_CANCELLED = "Order is already cancelled";
        public const string ERROR_CANNOT_CANCEL_DELIVERED = "Cannot cancel a delivered order";
        public const string ERROR_INVALID_STATUS_TRANSITION = "Invalid status transition";

        // Success Messages
        public const string SUCCESS_ORDER_CREATED = "Order created successfully";
        public const string SUCCESS_ORDER_UPDATED = "Order updated successfully";
        public const string SUCCESS_ORDER_CANCELLED = "Order cancelled successfully";
        public const string SUCCESS_STATUS_UPDATED = "Order status updated successfully";
    }

    /// <summary>
    /// Order system configuration options
    /// </summary>
    public class OrderConfiguration
    {
        public decimal DefaultDeliveryFee { get; set; } = OrderConstants.DEFAULT_DELIVERY_FEE_PER_MERCHANT;
        public decimal VatRate { get; set; } = OrderConstants.VAT_RATE;
        public int DefaultDeliveryDays { get; set; } = OrderConstants.DEFAULT_DELIVERY_DAYS;
        public int MaxItemsPerOrder { get; set; } = OrderConstants.MAX_ITEMS_PER_ORDER;
        public decimal MaxOrderValue { get; set; } = OrderConstants.MAX_ORDER_VALUE;
        public bool EnableStockValidation { get; set; } = false;
        public bool EnablePriceValidation { get; set; } = true;
        public bool AutoApproveOrders { get; set; } = false;
        public bool EnableOrderNotifications { get; set; } = true;
        public string OrderIdPrefix { get; set; } = OrderConstants.ORDER_PREFIX;
        public string TrackingIdPrefix { get; set; } = OrderConstants.TRACKING_PREFIX;
    }
}