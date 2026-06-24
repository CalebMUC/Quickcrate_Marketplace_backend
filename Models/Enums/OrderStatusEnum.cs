namespace Minimart_Api.Models.Enums
{
    public enum OrderStatusEnum
    {
        Pending = 0,             // Order created but not yet paid
        PaymentProcessing = 1,   // Waiting for M-Pesa/STK confirmation
        Paid = 2,                // Payment received
        Shipped = 3,             // Out for delivery
        Delivered = 4,           // Customer received order
        Cancelled = 5,           // Cancelled by merchant or user
        Refunded = 6,            // Money refunded
        Failed = 7               // Payment or order failed
    }
}
