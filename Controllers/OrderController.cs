using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.Merchants;
using Minimart_Api.DTOS.Orders;
using Minimart_Api.Models;
using Minimart_Api.Services.OrderService.OrderService;
using Minimart_Api.Services.RabbitMQ;

namespace Minimart_Api.Controllers
{
    /// <summary>
    /// Order management controller - Enhanced endpoints while maintaining existing logic
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderEventPublisher _orderEventPublisher;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService, 
            IOrderEventPublisher orderEventPublisher,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _orderEventPublisher = orderEventPublisher;
            _logger = logger;
        }

        #region Enhanced Endpoints (Improved but maintaining exact logic)

        /// <summary>
        /// Get orders by status for a specific user - Enhanced version
        /// </summary>
        [HttpGet("user/{userId}/status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(int userId, int status)
        {
            try
            {
                _logger.LogInformation("Getting orders for user {UserId} with status {Status}", userId, status);
                
                var orders = await _orderService.GetOrdersByStatusAsync(status, userId.ToString());
                
                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "No orders found for the given status.",
                        data = new List<object>()
                    });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Orders retrieved successfully", 
                    data = orders,
                    count = orders.Count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for user {UserId} with status {Status}", userId, status);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving orders" 
                });
            }
        }

        /// <summary>
        /// Get all available order statuses - Enhanced version
        /// </summary>
        [HttpGet("statuses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrderStatuses()
        {
            try
            {
                _logger.LogInformation("Getting all order statuses");
                
                var statuses = await _orderService.GetOrderStatusAsync();
                
                if (statuses == null || statuses.Count == 0)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "No order statuses found.",
                        data = new List<object>()
                    });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Order statuses retrieved successfully", 
                    data = statuses,
                    count = statuses.Count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statuses");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving order statuses" 
                });
            }
        }

        /// <summary>
        /// Update order status - Enhanced version
        /// </summary>
        [HttpPost("tracking/update/{productId}")]   
        //[Authorize(Policy = "AdminOrMerchant")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] OrderTrackingDTO orderTracking)
        {
            try
            {
                _logger.LogInformation("Updating status for order {product}", orderTracking.ProductId);
                
                // Ensure consistency
                //orderTracking.OrderId = orderId;
                
                var response = await _orderService.UpdateOrderStatusAsync(orderTracking);
                
                if (response.ResponseCode == 200)
                {
                    return Ok(new { 
                        success = true, 
                        message = response.ResponseMessage, 
                        data = response 
                    });
                }
                
                return BadRequest(new { 
                    success = false, 
                    message = response.ResponseMessage, 
                    data = response 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for {productId}", orderTracking.ProductId);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while updating order status" 
                });
            }
        }

        /// <summary>
        /// Get order tracking information - Enhanced version
        /// </summary>
        [HttpPost("tracking/product/{productId}")]
        public async Task<IActionResult> GetOrderTracking(GetOrderTrackingStatus trackingStatus)
        {
            try
            {
                _logger.LogInformation("Getting tracking for product {ProductId}", trackingStatus.ProductID);
                
                //var trackingStatus = new GetOrderTrackingStatus { ProductID = productId };
                var response = await _orderService.GetOrderTrackingAsync(trackingStatus);
                
                if (response == null || response.Count == 0)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "No tracking information found.",
                        data = new List<object>()
                    });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Tracking information retrieved successfully", 
                    data = response,
                    count = response.Count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tracking for product {ProductId}", trackingStatus.ProductID);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving tracking information" 
                });
            }
        }

        /// <summary>
        /// Get order by ID - Enhanced version
        /// </summary>
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(string orderId)
        {
            try
            {
                _logger.LogInformation("Getting order {OrderId}", orderId);
                
                var orders = await _orderService.GetOrdersByIdAsync(orderId);
                
                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "Order not found.",
                        data = new List<object>()
                    });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Order retrieved successfully", 
                    data = orders,
                    count = orders.Count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving the order" 
                });
            }
        }

        /// <summary>
        /// Get all orders (Admin only) - Enhanced version
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAdminOrders()
        {
            try
            {
                _logger.LogInformation("Getting all orders for admin");
                
                var orders = await _orderService.GetAdminOrdersAsync();
                
                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "No orders found.",
                        data = new List<object>()
                    });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Orders retrieved successfully", 
                    data = orders,
                    count = orders.Count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting admin orders");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving orders" 
                });
            }
        }

        /// <summary>
        /// Get merchant orders - Enhanced version
        /// </summary>
        [HttpGet("merchant/{merchantId}")]
        //[Authorize(Policy = "MerchantOnly")]
        public async Task<IActionResult> GetMerchantOrders(Guid merchantId, [FromQuery] string? orderId = null)
        {
            try
            {
                _logger.LogInformation("Getting orders for merchant {MerchantId}", merchantId);
                
                var requestDto = new MerchantRequestDto 
                { 
                    MerchantId = merchantId, 
                    OrderId = orderId ?? string.Empty 
                };
                
                var orders = await _orderService.GetMerchantOrdersAsync(requestDto);
                
                if (orders == null || orders.Count == 0)
                {
                    return NotFound(new { 
                        success = false, 
                        message = "No orders found for the merchant.",
                        data = new List<object>()
                    });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Merchant orders retrieved successfully", 
                    data = orders,
                    count = orders.Count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant orders for {MerchantId}", merchantId);
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while retrieving merchant orders" 
                });
            }
        }

        /// <summary>
        /// Create new order - Enhanced version (EXACT SAME LOGIC)
        /// </summary>
        [HttpPost("create")]
        //[Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] OrderListDto orderListDto)

        {
            if (orderListDto == null)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Order data is required",
                    data = (object)null 
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Invalid order data", 
                    errors = ModelState,
                    data = (object)null 
                });
            }

            try
            {
                _logger.LogInformation("Creating order with {Count} items", orderListDto.Orders?.Count ?? 0);
                
                // EXACT SAME LOGIC AS BEFORE
                var response = await _orderService.AddOrder(orderListDto);

                if (response.ResponseCode == 200)
                {
                    _logger.LogInformation("Order created successfully");
                    
                    return Created("", new { 
                        success = true, 
                        message = response.ResponseMessage, 
                        data = response,
                        orderCreated = true
                    });
                }
                
                return BadRequest(new { 
                    success = false, 
                    message = response.ResponseMessage, 
                    data = response 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { 
                    success = false, 
                    message = "An error occurred while creating the order",
                    data = (object)null 
                });
            }
        }

        #endregion

        #region Original Endpoints (Maintained for backward compatibility)

        /// <summary>
        /// Original: Get orders by status (Maintained for backward compatibility)
        /// </summary>
        [HttpPost("GetOrders")]
        public async Task<IActionResult> GetOrders([FromBody] OrderRequest request)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(request.Status, request.ApplicationUserId);
            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found for the given status.");
            }
            return Ok(orders);
        }

        /// <summary>
        /// Original: Get User Orders
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserOrders(string userId)
        {
            var orders = await _orderService.GetUserOrdersAsync(userId);
            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found for the given status.");
            }
            return Ok(orders);
        }

        /// <summary>
        /// Original: Get order status (Maintained for backward compatibility)
        /// </summary>
        [HttpGet("GetOrderStatus")]
        public async Task<IActionResult> GetOrderStatus()
        {
            var orders = await _orderService.GetOrderStatusAsync();
            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found for the given status.");
            }
            return Ok(orders);
        }

        /// <summary>
        /// Original: Update order status (Maintained for backward compatibility)
        /// </summary>
        [HttpPost("UpdateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatusAsync(OrderTrackingDTO orderTracking)
        {
            try
            {
                var response = await _orderService.UpdateOrderStatusAsync(orderTracking);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Original: Get order tracking (Maintained for backward compatibility)
        /// </summary>
        [HttpPost("GetOrderTracking")]
        public async Task<IActionResult> GetOrderTrackingAsync(GetOrderTrackingStatus trackingStatus)
        {
            try
            {
                var response = await _orderService.GetOrderTrackingAsync(trackingStatus);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Original: Get orders by ID (Maintained for backward compatibility)
        /// </summary>
        [HttpGet("GetOrdersById")]
        public async Task<IActionResult> GetOrdersByIdAsync(string OrderId)
        {
            var orders = await _orderService.GetOrdersByIdAsync(OrderId);
            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found for the given status.");
            }
            return Ok(orders);
        }

        /// <summary>
        /// Original: Get admin orders (Maintained for backward compatibility)
        /// </summary>
        [HttpPost("GetAdminOrders")]
        public async Task<IActionResult> GetAdminOrdersAsync()
        {
            var orders = await _orderService.GetAdminOrdersAsync();
            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found for the given status.");
            }
            return Ok(orders);
        }

        /// <summary>
        /// Original: Get merchant orders (Maintained for backward compatibility)
        /// </summary>
        [HttpPost("GetMerchantOrders")]
        public async Task<IActionResult> GetMerchantOrdersAsync(MerchantRequestDto requestDto)
        {
            var orders = await _orderService.GetMerchantOrdersAsync(requestDto);
            if (orders == null || orders.Count == 0)
            {
                return NotFound("No orders found for the given status.");
            }
            return Ok(orders);
        }

        /// <summary>
        /// Original: Add order (Maintained for backward compatibility)
        /// </summary>
        [HttpPost("AddOrder")]
        public async Task<IActionResult> AddOrder(OrderListDto orderDTO)
        {
            if (orderDTO == null)
            {
                return BadRequest("No DATA");
            }
            try
            {
                var response = await _orderService.AddOrder(orderDTO);
                return Ok(response);
                //call react page here for printing by passing the response
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Helper Methods

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? 
                             User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            
            return null;
        }

        private string? GetCurrentUserName()
        {
            return User.FindFirst("username")?.Value ?? 
                   User.FindFirst(ClaimTypes.Name)?.Value ??
                   User.FindFirst(ClaimTypes.Email)?.Value;
        }

        #endregion
    }
}
