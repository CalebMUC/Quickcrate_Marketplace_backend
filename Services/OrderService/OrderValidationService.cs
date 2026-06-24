using Minimart_Api.DTOS.Orders;
using Minimart_Api.Models;
using Minimart_Api.Repositories.Order;

namespace Minimart_Api.Services.OrderService
{
    public interface IOrderValidationService
    {
        Task<OrderValidationResult> ValidateCreateOrderAsync(CreateOrderRequest request);
        Task<OrderValidationResult> ValidateUpdateOrderStatusAsync(UpdateOrderStatusRequest request);
        Task<OrderValidationResult> ValidateOrderCancellationAsync(string orderId, string userId);
    }

    public class OrderValidationService : IOrderValidationService
    {
        private readonly IorderRepository _orderRepository;
        private readonly ILogger<OrderValidationService> _logger;

        public OrderValidationService(
            IorderRepository orderRepository,
            ILogger<OrderValidationService> logger)
        {
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<OrderValidationResult> ValidateCreateOrderAsync(CreateOrderRequest request)
        {
            var result = new OrderValidationResult { IsValid = true };

            try
            {
                // Validate basic requirements
                if (request.Items == null || !request.Items.Any())
                {
                    result.AddError("At least one item is required");
                }

                if (request.ShippingAddress == null)
                {
                    result.AddError("Shipping address is required");
                }

                // Validate each item
                if (request.Items != null)
                {
                    foreach (var item in request.Items)
                    {
                        await ValidateOrderItem(item, result);
                    }
                }

                // Validate user exists (if needed)
                // You might want to add user validation here

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating create order request");
                result.AddError("Validation failed due to system error");
                return result;
            }
        }

        public async Task<OrderValidationResult> ValidateUpdateOrderStatusAsync(UpdateOrderStatusRequest request)
        {
            var result = new OrderValidationResult { IsValid = true };

            try
            {
                // Check if order exists
                var order = await _orderRepository.GetOrderByIdAsync(request.OrderId);
                if (order == null)
                {
                    result.AddError("Order not found");
                    return result;
                }

                // Check if product exists in the order
                var orderProducts = await _orderRepository.GetOrderProductsAsync(request.OrderId);
                if (!orderProducts.Any(op => op.ProductId == request.ProductId))
                {
                    result.AddError("Product not found in this order");
                }

                // Validate status transition
                var validStatuses = await _orderRepository.GetOrderStatusAsync();
                if (!validStatuses.Any(s => s.StatusID == request.NewStatusId))
                {
                    result.AddError("Invalid status ID");
                }

                // Add business rules for status transitions
                await ValidateStatusTransition(order, request.NewStatusId, result);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating update order status request");
                result.AddError("Validation failed due to system error");
                return result;
            }
        }

        public async Task<OrderValidationResult> ValidateOrderCancellationAsync(string orderId, string userId)
        {
            var result = new OrderValidationResult { IsValid = true };

            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    result.AddError("Order not found");
                    return result;
                }

                // Check if order can be cancelled (business rules)
                if (order.StatusEnum == Models.Enums.OrderStatusEnum.Delivered)
                {
                    result.AddError("Cannot cancel a delivered order");
                }

                if (order.StatusEnum == Models.Enums.OrderStatusEnum.Cancelled)
                {
                    result.AddError("Order is already cancelled");
                }

                // Add more business rules as needed

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating order cancellation");
                result.AddError("Validation failed due to system error");
                return result;
            }
        }

        private async Task ValidateOrderItem(OrderItemRequest item, OrderValidationResult result)
        {
            // Check if product exists and is available
            var product = await _orderRepository.GetProductByIdAsync(item.ProductId);
            if (product == null)
            {
                result.AddError($"Product {item.ProductId} not found");
                return;
            }

            if (!product.IsActive)
            {
                result.AddError($"Product '{product.ProductName}' is not available");
            }

            // Check stock availability (if you have stock management)
            // You might want to add stock validation here

            // Validate quantity
            if (item.Quantity <= 0)
            {
                result.AddError($"Invalid quantity for product '{product.ProductName}'");
            }

            // Validate special price if provided
            if (item.SpecialPrice.HasValue && item.SpecialPrice.Value < 0)
            {
                result.AddError($"Invalid special price for product '{product.ProductName}'");
            }
        }

        private async Task ValidateStatusTransition(Order order, int newStatusId, OrderValidationResult result)
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<Models.Enums.OrderStatusEnum, List<int>>
            {
                { Models.Enums.OrderStatusEnum.Pending, new List<int> { 2, 6 } }, // Can go to PaymentProcessing or Cancelled
                { Models.Enums.OrderStatusEnum.PaymentProcessing, new List<int> { 3, 6 } }, // Can go to Shipped or Cancelled
                { Models.Enums.OrderStatusEnum.Shipped, new List<int> { 4 } }, // Can go to Delivered
                { Models.Enums.OrderStatusEnum.Delivered, new List<int>() }, // Terminal state
                { Models.Enums.OrderStatusEnum.Cancelled, new List<int>() } // Terminal state
            };

            if (validTransitions.ContainsKey(order.StatusEnum))
            {
                if (!validTransitions[order.StatusEnum].Contains(newStatusId))
                {
                    var statuses = await _orderRepository.GetOrderStatusAsync();
                    var currentStatus = statuses.FirstOrDefault(s => s.StatusID == (int)order.StatusEnum)?.Status ?? "Unknown";
                    var newStatus = statuses.FirstOrDefault(s => s.StatusID == newStatusId)?.Status ?? "Unknown";
                    
                    result.AddError($"Cannot transition from '{currentStatus}' to '{newStatus}'");
                }
            }
        }
    }

    public class OrderValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new();

        public void AddError(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }
    }
}