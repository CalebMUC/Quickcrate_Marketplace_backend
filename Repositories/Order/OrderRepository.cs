using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore.ReportingServices.ReportProcessing.ReportObjectModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Merchants;
using Minimart_Api.DTOS.Mpesa;
using Minimart_Api.DTOS.Orders;
using Minimart_Api.DTOS.Payments;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;
using Minimart_Api.Repositories.Mpesa;
using Minimart_Api.Repositories.Order;
using Minimart_Api.Services.RabbitMQ;
using Minimart_Api.Services.SignalR;
using Newtonsoft.Json;
using Npgsql;
using StackExchange.Redis;

namespace Minimart_Api.Repositories.Order
{
    public class OrderRepository : IorderRepository
    {
        private readonly MinimartDBContext _dbContext;
        private readonly IOrderEventPublisher _orderEventPublisher;
        private readonly IConfiguration _configuration;
        private readonly MpesaSandBox _mpesaSandBox;
        private readonly MpesaGoLive _mpesaGoLive;
        private readonly IHubContext<ActivityHub> _hubContext;
        private readonly IMpesaRepo _mpesaRepo;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<OrderRepository> _logger;
        
        private const string ConsumerKey = "vM5KjasAGTVzdddzpP8tENa1Z9us6G6CDjeZzEAHQKzVbQu4";
        private const string ConsumerSecret = "BZQ2uAq84LIzonV6uaXBo7ofYGTHvhhvFD5vVd8EuTwnsd0n0b9ewQ8ExNMKuOnn";
        private const string BusinessShortCode = "174379";
        private const string PassKey = "bfb279f9aa9bdbcf158e97dd71a467cd2e0c893059b10f78e6b72ada1ed2c919";

        public OrderRepository(MinimartDBContext dbContext,
            IOrderEventPublisher orderEventPublisher,
            IConfiguration configuration,
            IOptions<MpesaSandBox> mpesaSandBox,
            IOptions<MpesaGoLive> mpesaGoLive,
            IHttpClientFactory clientFactory,
            IHubContext<ActivityHub> hubContext,
            IMpesaRepo mpesaRepo,
            ILogger<OrderRepository> logger)
        {
            _dbContext = dbContext;
            _orderEventPublisher = orderEventPublisher;
            _configuration = configuration;
            _mpesaSandBox = mpesaSandBox.Value;
            _clientFactory = clientFactory;
            _mpesaRepo = mpesaRepo;
            _mpesaGoLive = mpesaGoLive.Value;
            _hubContext = hubContext;
            _logger = logger;
        }

        #region Enhanced CRUD Operations

        public async Task<ServiceResult<bool>> CreateOrderAsync(Models.Order order)
        {
            try
            {
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();
                
                return ServiceResult<bool>.Success(true, "Order created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order {OrderId}", order.OrderID);
                return ServiceResult<bool>.Failure("Failed to create order", new List<string> { ex.Message });
            }
        }

        public async Task<Models.Order?> GetOrderByIdAsync(string orderId)
        {
            try
            {
                return await _dbContext.Orders
                    .Include(o => o.OrderProducts)
                    .Include(o => o.OrderTrackings)
                    .FirstOrDefaultAsync(o => o.OrderID == orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order {OrderId}", orderId);
                return null;
            }
        }

        // Updated to use string userId (Identity system)
        public async Task<List<Models.Order>> GetUserOrdersAsync(string userId, int? statusId = null)
        {
            try
            {
                var query = _dbContext.Orders
                    .Where(o => o.ApplicationUserId == userId); // Updated to use ApplicationUserId

                if (statusId.HasValue)
                {
                    // Convert int status to string for comparison
                    var statusString = statusId.Value switch
                    {
                        1 => "Pending",
                        2 => "Processing", 
                        3 => "Paid",
                        4 => "Shipped",
                        5 => "Delivered",
                        6 => "Cancelled",
                        _ => "Pending"
                    };
                    
                    query = query.Where(o => o.Status == statusString);
                }

                return await query
                    .Include(o => o.OrderProducts)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user orders for user {UserId}", userId);
                return new List<Models.Order>();
            }
        }

        public async Task<PagedOrderResponse> GetOrdersAsync(OrderFilterRequest filter)
        {
            try
            {
                var query = _dbContext.Orders.AsQueryable();

                // Apply filters - Updated to handle string userId
                if (!string.IsNullOrEmpty(filter.ApplicationUserId))
                {
                    query = query.Where(o => o.ApplicationUserId == filter.ApplicationUserId);
                }

                if (filter.StatusIds != null && filter.StatusIds.Any())
                {
                    // Convert int status IDs to string status values
                    var statusStrings = filter.StatusIds.Select(id => id switch
                    {
                        1 => "Pending",
                        2 => "Processing",
                        3 => "Paid", 
                        4 => "Shipped",
                        5 => "Delivered",
                        6 => "Cancelled",
                        _ => "Pending"
                    }).ToList();

                    query = query.Where(o => statusStrings.Contains(o.Status));
                }

                if (filter.FromDate.HasValue)
                    query = query.Where(o => o.OrderDate >= filter.FromDate.Value);

                if (filter.ToDate.HasValue)
                    query = query.Where(o => o.OrderDate <= filter.ToDate.Value);

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply sorting
                if (!string.IsNullOrEmpty(filter.SortBy))
                {
                    switch (filter.SortBy.ToLower())
                    {
                        case "orderdate":
                            query = filter.SortDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate);
                            break;
                        case "amount":
                            query = filter.SortDescending ? query.OrderByDescending(o => o.TotalPaymentAmount) : query.OrderBy(o => o.TotalPaymentAmount);
                            break;
                        default:
                            query = query.OrderByDescending(o => o.OrderDate);
                            break;
                    }
                }

                // Apply pagination
                var orders = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Include(o => o.OrderProducts)
                    .ToListAsync();

                // Map to OrderResponse
                var orderResponses = new List<OrderResponse>();
                foreach (var order in orders)
                {
                    orderResponses.Add(await MapToOrderResponse(order));
                }

                return new PagedOrderResponse
                {
                    Orders = orderResponses,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize),
                    HasNextPage = filter.Page < Math.Ceiling((double)totalCount / filter.PageSize),
                    HasPreviousPage = filter.Page > 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders with filters");
                return new PagedOrderResponse { Orders = new List<OrderResponse>() };
            }
        }

        public async Task<bool> UpdateOrderAsync(Models.Order order)
        {
            try
            {
                _dbContext.Orders.Update(order);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order {OrderId}", order.OrderID);
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(string orderId)
        {
            try
            {
                var order = await _dbContext.Orders.FindAsync(orderId);
                if (order != null)
                {
                    _dbContext.Orders.Remove(order);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order {OrderId}", orderId);
                return false;
            }
        }

        #endregion

        #region Order Products Operations

        public async Task<bool> CreateOrderProductAsync(OrderProduct orderProduct)
        {
            try
            {
                _dbContext.OrderProducts.Add(orderProduct);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order product");
                return false;
            }
        }

        public async Task<List<OrderProduct>> GetOrderProductsAsync(string orderId)
        {
            try
            {
                return await _dbContext.OrderProducts
                    .Where(op => op.OrderID == orderId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order products for order {OrderId}", orderId);
                return new List<OrderProduct>();
            }
        }

        #endregion

        #region Status Operations

        public async Task<bool> UpdateOrderStatusAsync(string orderId, int newStatusId, string updatedBy)
        {
            try
            {
                var order = await _dbContext.Orders.FindAsync(orderId);
                if (order != null)
                {
                    // Convert int status to string
                    var statusString = newStatusId switch
                    {
                        1 => "Pending",
                        2 => "Processing",
                        3 => "Paid",
                        4 => "Shipped", 
                        5 => "Delivered",
                        6 => "Cancelled",
                        _ => "Pending"
                    };

                    order.Status = statusString;
                    order.StatusMessage = $"Updated by {updatedBy}";
                    
                    _dbContext.Orders.Update(order);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for {OrderId}", orderId);
                return false;
            }
        }

        public async Task<bool> CancelOrderAsync(string orderId, string reason, string cancelledBy)
        {
            try
            {
                var order = await _dbContext.Orders.FindAsync(orderId);
                if (order != null)
                {
                    order.Status = "Cancelled";
                    order.StatusEnum = Models.Enums.OrderStatusEnum.Cancelled;
                    order.StatusMessage = $"Cancelled: {reason}";
                    
                    _dbContext.Orders.Update(order);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order {OrderId}", orderId);
                return false;
            }
        }

        #endregion

        #region Tracking Operations

        public async Task<List<OrderTracking>> GetOrderTrackingByOrderIdAsync(string orderId)
        {
            try
            {
                return await _dbContext.OrderTracking
                    .Where(ot => ot.OrderID == orderId)
                    .OrderByDescending(ot => ot.TrackingDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order tracking for {OrderId}", orderId);
                return new List<OrderTracking>();
            }
        }

        public async Task<bool> AddTrackingUpdateAsync(OrderTracking tracking)
        {
            try
            {
                _dbContext.OrderTracking.Add(tracking);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tracking update");
                return false;
            }
        }

        #endregion

        #region Summary and Analytics

        // Updated to accept string userId
        public async Task<OrderSummaryResponse> GetOrderSummaryAsync(string userId = null, Guid? merchantId = null)
        {
            try
            {
                var query = _dbContext.Orders.AsQueryable();

                //if (!string.IsNullOrEmpty(userId))
                //{
                //    var userGuid = await ResolveUserIdAsync(userId);
                //    if (userGuid.HasValue)
                //    {
                //        query = query.Where(o => o.UserID == userGuid.Value);
                //    }
                //}

                var summary = new OrderSummaryResponse
                {
                    TotalOrders = await query.CountAsync(),
                    PendingOrders = await query.CountAsync(o => o.StatusEnum == Models.Enums.OrderStatusEnum.Pending),
                    ProcessingOrders = await query.CountAsync(o => o.StatusEnum == Models.Enums.OrderStatusEnum.PaymentProcessing),
                    ShippedOrders = await query.CountAsync(o => o.StatusEnum == Models.Enums.OrderStatusEnum.Shipped),
                    DeliveredOrders = await query.CountAsync(o => o.StatusEnum == Models.Enums.OrderStatusEnum.Delivered),
                    CancelledOrders = await query.CountAsync(o => o.StatusEnum == Models.Enums.OrderStatusEnum.Cancelled),
                    TotalRevenue = await query.SumAsync(o => o.TotalPaymentAmount),
                    TodayRevenue = await query
                        .Where(o => o.OrderDate.Date == DateTime.UtcNow.Date)
                        .SumAsync(o => o.TotalPaymentAmount),
                    RecentOrders = await GetRecentOrdersAsync(5, userId)
                };

                return summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order summary");
                return new OrderSummaryResponse();
            }
        }

        // Updated to accept string userId
        public async Task<List<OrderResponse>> GetRecentOrdersAsync(int count = 10, string userId = null)
        {
            try
            {
                var query = _dbContext.Orders.AsQueryable();

                //if (!string.IsNullOrEmpty(userId))
                //{
                //    var userGuid = await ResolveUserIdAsync(userId);
                //    if (userGuid.HasValue)
                //    {
                //        query = query.Where(o => o.UserID == userGuid.Value);
                //    }
                //}

                var orders = await query
                    .OrderByDescending(o => o.OrderDate)
                    .Take(count)
                    .Include(o => o.OrderProducts)
                    .ToListAsync();

                var orderResponses = new List<OrderResponse>();
                foreach (var order in orders)
                {
                    orderResponses.Add(await MapToOrderResponse(order));
                }

                return orderResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                return new List<OrderResponse>();
            }
        }

        #endregion

        #region Product and Merchant Lookups

        public async Task<Product?> GetProductByIdAsync(Guid productId)
        {
            try
            {
                return await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product {ProductId}", productId);
                return null;
            }
        }

        public async Task<Merchants?> GetMerchantByIdAsync(Guid merchantId)
        {
            try
            {
                return await _dbContext.Merchants
                    .FirstOrDefaultAsync(m => m.MerchantID == merchantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant {MerchantId}", merchantId);
                return null;
            }
        }

        #endregion

        #region Helper Methods

        // Helper method to resolve user ID from Identity to Legacy
        //private async Task<Guid?> ResolveUserIdAsync(string identityUserId)
        //{
        //    try
        //    {
        //        // First try to parse as Guid directly (for backward compatibility)
        //        if (Guid.TryParse(identityUserId, out var directGuid))
        //        {
        //            return directGuid;
        //        }

        //        // Try to find the Identity user and get their legacy user ID
        //        var identityUser = await _dbContext.Users
        //            .FirstOrDefaultAsync(u => u.Id == identityUserId);

        //        if (identityUser?.LegacyUserId.HasValue == true)
        //        {
        //            // Convert int legacy ID to Guid format
        //            var legacyId = identityUser.LegacyUserId.Value;
        //            var guidBytes = new byte[16];
        //            BitConverter.GetBytes(legacyId).CopyTo(guidBytes, 0);
        //            return new Guid(guidBytes);
        //        }

        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error resolving user ID {IdentityUserId}", identityUserId);
        //        return null;
        //    }
        //}

        // Helper method to resolve Identity user ID from legacy Guid
        //private async Task<string> ResolveIdentityUserIdAsync(Guid legacyUserGuid)
        //{
        //    try
        //    {
        //        // Extract int from Guid
        //        var guidBytes = legacyUserGuid.ToByteArray();
        //        var legacyUserId = BitConverter.ToInt32(guidBytes, 0);

        //        // Find Identity user by legacy ID
        //        var identityUser = await _dbContext.Users
        //            .FirstOrDefaultAsync(u => u.LegacyUserId == legacyUserId);

        //        return identityUser?.Id ?? legacyUserGuid.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error resolving Identity user ID from legacy GUID {LegacyUserGuid}", legacyUserGuid);
        //        return legacyUserGuid.ToString();
        //    }
        //}

        private async Task<OrderResponse> MapToOrderResponse(Models.Order order)
        {
            try
            {
                var shippingAddress = !string.IsNullOrEmpty(order.ShippingAddress) 
                    ? JsonConvert.DeserializeObject<ShippingAddressResponse>(order.ShippingAddress)
                    : null;

                var merchantGroups = !string.IsNullOrEmpty(order.ProductsJson)
                    ? JsonConvert.DeserializeObject<List<MerchantOrderGroup>>(order.ProductsJson)
                    : new List<MerchantOrderGroup>();

                var paymentDetails = !string.IsNullOrEmpty(order.PaymentDetailsJson)
                    ? JsonConvert.DeserializeObject<PaymentResponse>(order.PaymentDetailsJson)
                    : null;

                var tracking = await GetOrderTrackingByOrderIdAsync(order.OrderID);

                // Convert Guid UserID to Identity user ID string
                //var userIdString = await ResolveIdentityUserIdAsync(order.UserID);

                return new OrderResponse
                {
                    OrderId = order.OrderID,
                    ApplicationUserId = order.ApplicationUserId ?? string.Empty, // Updated fallback
                    Status = order.StatusMessage ?? "Unknown",
                    StatusEnum = order.StatusEnum,
                    OrderDate = order.OrderDate,
                    DeliveryScheduleDate = order.DeliveryScheduleDate,
                    OrderedBy = order.OrderedBy ?? "",
                    PaymentConfirmation = order.PaymentConfirmation ?? "",
                    SubTotal = order.TotalOrderAmount,
                    TotalDeliveryFees = order.TotalDeliveryFees,
                    TotalTax = order.TotalTax,
                    TotalAmount = order.TotalPaymentAmount,
                    ShippingAddress = shippingAddress,
                    MerchantGroups = merchantGroups,
                    PaymentDetails = paymentDetails,
                    TrackingHistory = tracking.Select(MapToOrderTrackingResponse).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error mapping order {OrderId} to response", order.OrderID);
                // Return a basic response in case of mapping errors
                return new OrderResponse
                {
                    OrderId = order.OrderID,
                    ApplicationUserId = order.ApplicationUserId ?? string.Empty, // Updated fallback
                    Status = order.StatusMessage ?? "Unknown",
                    StatusEnum = order.StatusEnum,
                    OrderDate = order.OrderDate,
                    SubTotal = order.TotalOrderAmount,
                    TotalAmount = order.TotalPaymentAmount,
                    MerchantGroups = new List<MerchantOrderGroup>(),
                    TrackingHistory = new List<OrderTrackingResponse>()
                };
            }
        }

        private OrderTrackingResponse MapToOrderTrackingResponse(OrderTracking tracking)
        {
            return new OrderTrackingResponse
            {
                TrackingId = tracking.TrackingID,
                ProductId = tracking.ProductId,
                ProductName = "Product", // You might want to fetch this from the database
                PreviousStatus = tracking.PreviousStatus.ToString() ?? "Unknown",
                CurrentStatus = tracking.CurrentStatus.ToString(),
                TrackingDate = tracking.TrackingDate,
                ExpectedDeliveryDate = tracking.ExpectedDeliveryDate,
                Carrier = tracking.Carrier,
                UpdatedBy = tracking.UpdatedBy ?? ""
            };
        }

        #endregion

        #region Legacy Methods (Existing Implementation) - Updated for Identity

        // Updated to use string userID
        public async Task<List<GetOrdersDTO>> GetOrdersByStatusAsync(int status, string userID)
        {
            try
            {
                // Step 1: Fetch orders directly without joins to OrderStatuses
                //var ordersWithStatus = await _dbContext.Orders
                //    .Where(o => o.StatusID == status && o.ApplicationUserId == userID && o.StatusEnum == Models.Enums.OrderStatusEnum.Paid)
                //    .Select(o => new { Order = o, StatusMessage = o.Status })
                //    .ToListAsync();

                var ordersWithStatus = await _dbContext.Orders
                   .Where(o => o.StatusID == status && o.ApplicationUserId == userID)
                   .Select(o => new { Order = o, StatusMessage = o.Status })
                   .ToListAsync();

                // Step 2: Map the result to GetOrdersDTO and fetch product images
                var orders = new List<GetOrdersDTO>();

                foreach (var orderWithStatus in ordersWithStatus)
                {
                    var order = orderWithStatus.Order;
                    var products = JsonConvert.DeserializeObject<List<OrderProductsDTO>>(order.ProductsJson);

                    // Fetch ImageUrl for each product
                    var productsWithImages = products?.Select(p => new OrderProductsDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        merchantId = p.merchantId,
                        ImageUrl = _dbContext.Products
                            .Where(tp => tp.ProductId.ToString() == p.ProductID.ToString())
                            .Select(tp => tp.ImageUrls.FirstOrDefault() ?? "")
                            .FirstOrDefault() ?? ""
                    }).ToList();

                    // Map to GetOrdersDTO
                    var getOrderDTO = new GetOrdersDTO
                    {
                        OrderID = order.OrderID,
                        OrderDate = order.OrderDate,
                        TotalOrderAmount = (double)order.TotalOrderAmount,
                        Status = orderWithStatus.StatusMessage,
                        PaymentConfirmation = order.PaymentConfirmation,
                        TotalPaymentAmount = (double)order.TotalPaymentAmount,
                        TotalDeliveryFees = (double)order.TotalDeliveryFees,
                        TotalTax = (double)order.TotalTax,
                        ShippingAddress = JsonConvert.DeserializeObject<ShippingAddress>(order.ShippingAddress),
                        Products = productsWithImages,
                        PickUpLocation = JsonConvert.DeserializeObject<PickUpLocation>(order.PickupLocation),
                        PaymentDetails = JsonConvert.DeserializeObject<List<PaymentDetailsDto>>(order.PaymentDetailsJson)
                    };

                    orders.Add(getOrderDTO);
                }

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by status for user {UserID}", userID);
                return new List<GetOrdersDTO>();
            }
        }


        public async Task<List<GetOrdersDTO>> GetUserOrdersAsync(string userId)
        {
            try {

                var userOrders = await _dbContext.Orders.Where(o=> o.ApplicationUserId == userId).ToListAsync();
                //loop through the orders to bind with GetOrdersDto
                var orders = new List<GetOrdersDTO>();

                foreach (var userOrder in userOrders) {

                    var products = JsonConvert.DeserializeObject<List<OrderProductsDTO>>(userOrder.ProductsJson);

                    // Fetch ImageUrl for each product
                    var productsWithImages = products?.Select(p => new OrderProductsDTO
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        merchantId = p.merchantId,
                        ImageUrl = _dbContext.Products
                            .Where(tp => tp.ProductId.ToString() == p.ProductID.ToString())
                            .Select(tp => tp.ImageUrls.FirstOrDefault() ?? "")
                            .FirstOrDefault() ?? ""
                    }).ToList();

                    // Map to GetOrdersDTO
                    var getOrderDTO = new GetOrdersDTO
                    {
                        OrderID = userOrder.OrderID,
                        OrderDate = userOrder.OrderDate,
                        TotalOrderAmount = (double)userOrder.TotalOrderAmount,
                        Status = userOrder.StatusMessage,
                        PaymentConfirmation = userOrder.PaymentConfirmation,
                        TotalPaymentAmount = (double)userOrder.TotalPaymentAmount,
                        TotalDeliveryFees = (double)userOrder.TotalDeliveryFees,
                        TotalTax = (double)userOrder.TotalTax,
                        ShippingAddress = JsonConvert.DeserializeObject<ShippingAddress>(userOrder.ShippingAddress),
                        Products = productsWithImages,
                        PickUpLocation = JsonConvert.DeserializeObject<PickUpLocation>(userOrder.PickupLocation),
                        PaymentDetails = JsonConvert.DeserializeObject<List<PaymentDetailsDto>>(userOrder.PaymentDetailsJson)
                    };


                    orders.Add(getOrderDTO);

                }

                return orders;

            }
            catch (Exception ex) {
            
                _logger.LogError(ex, "Error getting orders by status for user {UserID}", userId);
                return new List<GetOrdersDTO>();
            }
        }

        public async Task<Status> UpdateOrderStatusAsync(OrderTrackingDTO orderTracking)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                
                try
                {
                    // 1. Get Existing Tracking Record
                    var existingProductTracker = await _dbContext.OrderTracking
                        .Where(ot => ot.TrackingID == orderTracking.TrackingID && ot.ProductId == orderTracking.ProductId)
                        .FirstOrDefaultAsync();

                    if (existingProductTracker == null)
                    {
                        return new Status
                        {
                            ResponseCode = 404,
                            ResponseMessage = "Order tracking record not found."
                        };
                    }

                    // 2. Get the new status from OrderStatuses table
                    var newOrderStatus = await _dbContext.OrderStatuses
                        .Where(os => os.StatusID == orderTracking.StatusId)
                        .FirstOrDefaultAsync();

                    if (newOrderStatus == null)
                    {
                        return new Status
                        {
                            ResponseCode = 404,
                            ResponseMessage = "Order status not found."
                        };
                    }

                    var updateTime = DateTime.UtcNow;

                    // 3. Update Tracking Record
                    existingProductTracker.PreviousStatus = existingProductTracker.CurrentStatus;
                    existingProductTracker.CurrentStatus = newOrderStatus.Status;
                    existingProductTracker.UpdatedBy = orderTracking.UpdatedBy;
                    existingProductTracker.UpdatedOn = updateTime;
                    existingProductTracker.TrackingNotes = $"Status updated to {newOrderStatus.Status} by {orderTracking.UpdatedBy}";

                    _dbContext.OrderTracking.Update(existingProductTracker);

                    // 4. Update OrderProduct status to match tracking status
                    var orderProduct = await _dbContext.OrderProducts
                        .Where(op => op.OrderID == orderTracking.OrderId && op.ProductId == orderTracking.ProductId)
                        .FirstOrDefaultAsync();

                    if (orderProduct != null)
                    {
                        var previousProductStatus = orderProduct.Status;
                        orderProduct.Status = MapStringToOrderStatusEnum(newOrderStatus.Status);
                        orderProduct.UpdatedOn = updateTime;
                        
                        _dbContext.OrderProducts.Update(orderProduct);
                        
                        _logger.LogInformation("Updated OrderProduct {ProductId} in Order {OrderId} from {PreviousStatus} to {NewStatus}", 
                            orderProduct.ProductId, orderProduct.OrderID, previousProductStatus, orderProduct.Status);
                    }
                    else
                    {
                        _logger.LogWarning("OrderProduct not found for Order {OrderId}, Product {ProductId}", 
                            orderTracking.OrderId, orderTracking.ProductId);
                    }

                    // 5. Calculate and update overall order status based on all product statuses
                    await UpdateOverallOrderStatusAsync(orderTracking.OrderId, orderTracking.UpdatedBy, updateTime);

                    // 6. Save all changes
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully updated tracking for Order {OrderId}, Product {ProductId} to status {Status}", 
                        orderTracking.OrderId, orderTracking.ProductId, newOrderStatus.Status);

                    return new Status
                    {
                        ResponseCode = 200,
                        ResponseMessage = "Order Tracking and Status Updated Successfully"
                    };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error updating order tracking for Order {OrderId}, Product {ProductId}", 
                        orderTracking.OrderId, orderTracking.ProductId);
                    
                    return new Status
                    {
                        ResponseCode = 500,
                        ResponseMessage = $"Error updating order tracking: {ex.Message}"
                    };
                }
            });
        }

        public async Task<List<GetOrderTracking>> GetOrderTrackingAsync(GetOrderTrackingStatus trackingStatus)
        {
            try
            {
                var tracking = await _dbContext.OrderTracking
                    .Where(ot => ot.ProductId == trackingStatus.ProductID && ot.OrderID == trackingStatus.OrderID)
                    .Select(ot => new GetOrderTracking
                    {
                        TrackingID = ot.TrackingID,
                        //OrderID = ot.OrderID,
                        //ProductId = ot.ProductId,
                        MerchantID = ot.MerchantID,
                        CurrentStatus = ot.CurrentStatus,
                        PreviousStatus = ot.PreviousStatus,
                        TrackingDate = ot.TrackingDate,
                        ExpectedDeliveryDate = ot.ExpectedDeliveryDate,
                        Carrier = ot.Carrier,
                        CreatedOn = ot.CreatedOn,
                        CreatedBy = ot.CreatedBy,
                        UpdatedBy = ot.UpdatedBy,
                        UpdatedOn = ot.UpdatedOn,
                    }).ToListAsync();

                return tracking;
            }
            catch (Exception ex)
            {
                return new List<GetOrderTracking>();
            }
        }

        public async Task<List<GetOrdersDTO>> GetOrdersByIdAsync(string OrderId)
        {
            try
            {
                // Use a join query to get the orders and their status messages from OrderStatuses
                var orders = await _dbContext.Orders
                    .Where(o => o.OrderID == OrderId)
                    .Join(_dbContext.OrderStatuses,  // Join with general statuses
                          o => o.StatusID,
                          os => os.StatusID,
                          (o, os) => new GetOrdersDTO
                          {
                              OrderID = o.OrderID,
                              OrderDate = o.OrderDate,
                              TotalOrderAmount = (double)o.TotalOrderAmount,
                              Status = os.Status,
                              PaymentConfirmation = o.PaymentConfirmation,
                              TotalPaymentAmount = (double)o.TotalPaymentAmount,
                              TotalDeliveryFees = (double)o.TotalDeliveryFees,
                              TotalTax = (double)o.TotalTax,
                              ShippingAddress = JsonConvert.DeserializeObject<ShippingAddress>(o.ShippingAddress),
                              Products = JsonConvert.DeserializeObject<List<OrderProductsDTO>>(o.ProductsJson),
                              PickUpLocation = JsonConvert.DeserializeObject<PickUpLocation>(o.PickupLocation),
                              PaymentDetails = JsonConvert.DeserializeObject<List<PaymentDetailsDto>>(o.PaymentDetailsJson)
                          })
                    .ToListAsync();

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders by ID {OrderId}", OrderId);
                return new List<GetOrdersDTO>();
            }
        }

        public async Task<List<MerchantOrderDto>> GetAdminOrdersAsync()
        {
            try
            {
                var merchantOrders = await _dbContext.Orders
                    .Join(
                        _dbContext.OrderStatuses, // join with static lookup table
                        o => o.StatusID,
                        os => os.StatusID,
                        (o, os) => new { Order = o, StatusName = os.Status }
                    )
                    .AsNoTracking() // optional for read-only queries
                    .ToListAsync(); // execute query in database

                // Deserialize products and flatten into MerchantOrderDto
                var result = merchantOrders
                    .SelectMany(joined =>
                        JsonConvert.DeserializeObject<List<OrderProductsDTO>>(joined.Order.ProductsJson)
                            .Select(p => new MerchantOrderDto
                            {
                                OrderId = joined.Order.OrderID,
                                //ProductName = p.ProductName,
                                //Quantity = p.Quantity,
                                //Price = p.Price,
                                Status = joined.StatusName
                            })
                    )
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting admin orders: {Message}", ex.Message);
                return new List<MerchantOrderDto>();
            }
        }


        //public async Task<List<MerchantOrderDto>> GetMerchantOrdersAsync(MerchantRequestDto requestDto)
        //{
        //    try
        //    {
        //        // Apply filtering at the database level - join with OrderStatuses
        //        var query = _dbContext.Orders
        //            .Join(
        //                _dbContext.OrderStatuses, // Join with general statuses
        //                o => o.StatusID,
        //                os => os.StatusID,
        //                (o, os) => new { Order = o, StatusName = os.Status }
        //            )
        //            .Where(joined => joined.Order.ProductsJson != null);

        //        if (!string.IsNullOrEmpty(requestDto.OrderId))
        //        {
        //            query = query.Where(joined => joined.Order.OrderID == requestDto.OrderId);
        //        }

        //        var rawOrders = await query.ToListAsync();

        //        var merchantOrders = rawOrders
        //            .SelectMany(joined =>
        //                JsonConvert.DeserializeObject<List<OrderProductsDTO>>(joined.Order.ProductsJson ?? "[]")
        //                .Where(p => p.merchantId == requestDto.MerchantId)
        //                .Select(p => new MerchantOrderDto
        //                {
        //                    OrderId = joined.Order.OrderID,
        //                    Quantity = p.Quantity,
        //                    ProductName = p.ProductName,
        //                    Price = p.Price,
        //                    Status = joined.StatusName,
        //                    ProductID = p.ProductID
        //                })
        //            )
        //            .ToList();

        //        return merchantOrders;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "An error occurred while getting merchant orders: {Message}", ex.Message);
        //        return new List<MerchantOrderDto>();
        //    }
        //}


        public async Task<List<MerchantOrderDto>> GetMerchantOrdersAsync(MerchantRequestDto requestDto)
        {
            try
            {
                // Query Orders with Status
                var query = _dbContext.Orders
                    .Join(
                        _dbContext.OrderStatuses,
                        o => o.StatusID,
                        os => os.StatusID,
                        (o, os) => new { Order = o, StatusName = os.Status }
                    )
                    .Where(x => x.Order.ProductsJson != null);

                // Filter by specific OrderID if provided
                if (!string.IsNullOrEmpty(requestDto.OrderId))
                {
                    query = query.Where(x => x.Order.OrderID == requestDto.OrderId);
                }

                var rawOrders = await query.ToListAsync();

                // Transform to grouped Merchant Orders
                var merchantOrders = rawOrders
                    .Select(joined =>
                    {
                        // Deserialize products
                        var allProducts = JsonConvert.DeserializeObject<List<OrderProductsDTO>>(joined.Order.ProductsJson ?? "[]");

                        // Filter products for this merchant
                        var merchantProducts = allProducts
                            .Where(p => p.merchantId == requestDto.MerchantId)
                            .ToList();

                        if (!merchantProducts.Any())
                            return null;

                        return new
                        {
                            joined.Order,
                            joined.StatusName,
                            Products = merchantProducts
                        };
                    })
                    .Where(x => x != null)
                    .GroupBy(x => x.Order.OrderID) // Group by Order ID
                    .Select(group => new MerchantOrderDto
                    {
                        //MerchantOrderId = $"MO-{group.Key}",
                        OrderId = group.Key,
                        Status = group.First().StatusName,
                        OrderDate = group.First().Order.OrderDate,

                        SubTotal = group
                            .SelectMany(g => g.Products)
                            .Sum(p => p.Price * p.Quantity),

                        Products = group
                            .SelectMany(g => g.Products)
                            .Select(p => new MerchantOrderProductDto
                            {
                                ProductID = p.ProductID,
                                ProductName = p.ProductName,
                                Quantity = p.Quantity,
                                Price = p.Price
                            })
                            .ToList()
                    })
                    .ToList();

                return merchantOrders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving merchant orders: {Message}", ex.Message);
                return new List<MerchantOrderDto>();
            }
        }



        //public async Task<Status> AddOrder(OrderListDto transaction)
        //{
        //    var strategy = _dbContext.Database.CreateExecutionStrategy();
        //    Status result = null;

        //    await strategy.ExecuteAsync(async () =>
        //    {
        //        await using var transactionScope = await _dbContext.Database.BeginTransactionAsync();

        //        try
        //        {
        //            foreach (var orderDto in transaction.Orders)
        //            {
        //                var trxRef = orderDto.PaymentDetails.FirstOrDefault()?.TrxReference;

        //                var existingPayment = await _dbContext.PaymentDetails
        //                    .FirstOrDefaultAsync(p => p.TrxReference == trxRef);

        //                if (existingPayment == null || existingPayment.Status != "Success") 
        //                    throw new Exception($"Payment not confirmed.");

        //                var newOrder = await CreateOrderEntity(orderDto, existingPayment);

        //                // Update payment Details with OrderID
        //                existingPayment.OrderID = newOrder.OrderID;
        //                _dbContext.PaymentDetails.Update(existingPayment);
        //                await _dbContext.SaveChangesAsync();

        //                await UpdateProductStock(orderDto.Products);

        //                _dbContext.Orders.Add(newOrder);
        //                await _dbContext.SaveChangesAsync();

        //                // Updated to use string userId
        //                await UpdateCartItems(orderDto.Products, orderDto.UserID.ToString());

        //                await TrackOrderAsync(newOrder);
        //            }

        //            await transactionScope.CommitAsync();

        //            result = new Status
        //            {
        //                ResponseCode = 200,
        //                ResponseMessage = "Transaction completed successfully"
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            await transactionScope.RollbackAsync();
        //            result = new Status
        //            {
        //                ResponseCode = 500,
        //                ResponseMessage = $"Internal Server Error: {ex.Message}"
        //            };
        //        }
        //    });

        //    return result;
        //}

        public async Task<Status> AddOrder(OrderListDto transaction)
        {
            // Option 1: Use execution strategy without manual transaction (recommended for this case)
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            Status result = null;

            await strategy.ExecuteAsync(async () =>
            {
                try
                {
                    foreach (var orderDto in transaction.Orders)
                    {
                        var trxRef = orderDto.PaymentDetails.FirstOrDefault()?.TrxReference;
                        var paymentMethod = orderDto.PaymentDetails.FirstOrDefault()?.PaymentMethod?.ToLower();

                        PaymentDetails existingPayment = null;

                        // Handle different payment methods
                        if (paymentMethod == "cash on delivery" || paymentMethod == "cod")
                        {
                            // For Cash On Delivery, create a pending payment record if it doesn't exist
                            existingPayment = await _dbContext.PaymentDetails
                                .FirstOrDefaultAsync(p => p.TrxReference == trxRef);

                            if (existingPayment == null)
                            {
                                // Create a new payment record for COD
                                var paymentDetail = orderDto.PaymentDetails.FirstOrDefault();
                                var paymentDate = DateTime.UtcNow; // Use UTC for timestamp with time zone

                                existingPayment = new PaymentDetails
                                {
                                    PaymentID = paymentDetail?.PaymentID ?? Guid.NewGuid(),
                                    PaymentMethodID = paymentDetail?.PaymentMethodID ?? 1005, // COD method ID
                                    TrxReference = trxRef ?? $"COD_{DateTime.UtcNow:yyyyMMddHHmmss}",
                                    PaymentReference = trxRef ?? $"COD_{DateTime.UtcNow:yyyyMMddHHmmss}",
                                    Phonenumber = paymentDetail?.Phonenumber ?? "",
                                    Amount = paymentDetail?.Amount ?? orderDto.TotalPaymentAmount,
                                    PaymentDate = paymentDate, // Use UTC DateTime
                                    Status = "Pending" // COD orders start as pending
                                };

                                _dbContext.PaymentDetails.Add(existingPayment);
                                await _dbContext.SaveChangesAsync();
                            }

                            // For COD, we allow "Pending" status
                            if (existingPayment.Status != "Success" && existingPayment.Status != "Pending")
                            {
                                throw new Exception($"Payment status is invalid for COD order. Status: {existingPayment.Status}");
                            }
                        }
                        else if (paymentMethod == "mpesa")
                        {
                            // For M-Pesa, require successful payment
                            existingPayment = await _dbContext.PaymentDetails
                                .FirstOrDefaultAsync(p => p.TrxReference == trxRef);

                            if (existingPayment == null || existingPayment.Status != "Success")
                                throw new Exception($"Payment not confirmed for M-Pesa payment. Please complete payment first.");
                        }
                        else
                        {
                            // For other payment methods, require successful payment
                            existingPayment = await _dbContext.PaymentDetails
                                .FirstOrDefaultAsync(p => p.TrxReference == trxRef);

                            if (existingPayment == null || existingPayment.Status != "Success")
                                throw new Exception($"Payment not confirmed for {paymentMethod} payment method.");
                        }

                        var productIds = orderDto.Products.Select(P => P.ProductID).Distinct().ToList();

                        //Getting MerchnatId and ProductId from the extracted productIds
                        var productMerchantMap = await _dbContext.Products
                                                    .Where(p => productIds.Contains(p.ProductId))
                                                    .ToDictionaryAsync(
                                                        p => p.ProductId,
                                                        p =>p.MerchantID
                                                        
                                                    );

                        if (productMerchantMap.Count != productIds.Count)
                            throw new Exception("One or more products are invalid.");

                        var newOrder = await CreateOrderEntity(orderDto, existingPayment,productMerchantMap);

                        // Update payment Details with OrderID
                        //existingPayment.OrderID = newOrder.OrderID; //PaymentID is the foreign key in Orders table
                        _dbContext.PaymentDetails.Update(existingPayment);

                        await UpdateProductStock(orderDto.Products);

                        _dbContext.Orders.Add(newOrder);

                        // Update cart items - remove purchased items from user's cart
                        await UpdateCartItemsAsync(orderDto.Products, orderDto.ApplicationUserId);

                        await TrackOrderAsync(newOrder);
                    }

                    // Save all changes at once
                    await _dbContext.SaveChangesAsync();

                    result = new Status
                    {
                        ResponseCode = 200,
                        ResponseMessage = "Transaction completed successfully"
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order transaction");
                    result = new Status
                    {
                        ResponseCode = 500,
                        ResponseMessage = $"Internal Server Error: {ex.Message}"
                    };
                    throw; // Re-throw to trigger execution strategy retry if needed
                }
            });

            return result;
        }

        #endregion

        #region Cart Management Methods

        /// <summary>
        /// Updates cart items by removing purchased products from the user's cart
        /// </summary>
        private async Task UpdateCartItemsAsync(List<OrderProductsDTO> purchasedProducts, string applicationUserId)
        {
            try
            {
                if (purchasedProducts == null || !purchasedProducts.Any() || string.IsNullOrEmpty(applicationUserId))
                {
                    _logger.LogWarning("No products to remove from cart or invalid user ID");
                    return;
                }

                // Get the user's cart
                var userCart = await _dbContext.Cart
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);

                if (userCart == null || !userCart.CartItems.Any())
                {
                    _logger.LogInformation("No cart found or cart is empty for user {UserId}", applicationUserId);
                    return;
                }

                var removedItemsCount = 0;
                var updatedItemsCount = 0;

                // Process each purchased product
                foreach (var purchasedProduct in purchasedProducts)
                {
                    // Find matching cart item
                    var cartItem = userCart.CartItems
                        .FirstOrDefault(ci => ci.ProductId == purchasedProduct.ProductID);

                    if (cartItem != null)
                    {
                        if (cartItem.Quantity <= purchasedProduct.Quantity)
                        {
                            // Remove the cart item completely if purchased quantity >= cart quantity
                            userCart.CartItems.Remove(cartItem);
                            _dbContext.CartItems.Remove(cartItem);
                            removedItemsCount++;

                            _logger.LogInformation("Removed product {ProductId} from cart for user {UserId} - Purchased: {PurchasedQty}, Cart had: {CartQty}", 
                                purchasedProduct.ProductID, applicationUserId, purchasedProduct.Quantity, cartItem.Quantity);
                        }
                        else
                        {
                            // Reduce the cart item quantity if cart quantity > purchased quantity
                            cartItem.Quantity -= purchasedProduct.Quantity;
                            cartItem.UpdatedOn = DateTime.UtcNow;
                            _dbContext.CartItems.Update(cartItem);
                            updatedItemsCount++;

                            _logger.LogInformation("Reduced quantity for product {ProductId} in cart for user {UserId} - Purchased: {PurchasedQty}, Remaining: {RemainingQty}", 
                                purchasedProduct.ProductID, applicationUserId, purchasedProduct.Quantity, cartItem.Quantity);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Product {ProductId} not found in cart for user {UserId}", 
                            purchasedProduct.ProductID, applicationUserId);
                    }
                }

                // Update cart's UpdatedAt timestamp if any changes were made
                if (removedItemsCount > 0 || updatedItemsCount > 0)
                {
                    //userCart.UpdatedAt = DateTime.UtcNow;
                    _dbContext.Cart.Update(userCart);

                    _logger.LogInformation("Updated cart for user {UserId} - Removed {RemovedCount} items, Updated {UpdatedCount} items", 
                        applicationUserId, removedItemsCount, updatedItemsCount);
                }
                else
                {
                    _logger.LogInformation("No cart items were affected for user {UserId}", applicationUserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart items for user {UserId}", applicationUserId);
                // Don't throw the exception as cart update failure shouldn't fail the order
                // The order should still be processed even if cart update fails
            }
        }

        /// <summary>
        /// Alternative method that completely clears the user's cart (if preferred)
        /// </summary>
        private async Task ClearUserCartAsync(string applicationUserId)
        {
            try
            {
                if (string.IsNullOrEmpty(applicationUserId))
                {
                    _logger.LogWarning("Invalid user ID provided for cart clearing");
                    return;
                }

                var userCart = await _dbContext.Cart
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId);

                if (userCart == null)
                {
                    _logger.LogInformation("No cart found for user {UserId}", applicationUserId);
                    return;
                }

                if (userCart.CartItems.Any())
                {
                    var itemCount = userCart.CartItems.Count;
                    _dbContext.CartItems.RemoveRange(userCart.CartItems);
                    userCart.UpdatedAt = DateTime.UtcNow;
                    _dbContext.Cart.Update(userCart);

                    _logger.LogInformation("Cleared {ItemCount} items from cart for user {UserId}", itemCount, applicationUserId);
                }
                else
                {
                    _logger.LogInformation("Cart was already empty for user {UserId}", applicationUserId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", applicationUserId);
                // Don't throw the exception as cart clearing failure shouldn't fail the order
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<Models.Order> CreateOrderEntity(OrderDTO orderDto, PaymentDetails payment,Dictionary<Guid, Guid> productMerchants)
        {
            // Use the ApplicationUserId directly as a string since we're now using Identity system
            var applicationUserId = orderDto.ApplicationUserId ?? throw new Exception("Application User ID is required");

            // For timestamp with time zone columns, we MUST use UTC DateTime values
            var orderDate = orderDto.OrderDate.Kind == DateTimeKind.Utc 
                ? orderDto.OrderDate 
                : DateTime.SpecifyKind(orderDto.OrderDate, DateTimeKind.Utc);

            var deliveryDate = orderDto.DeliveryScheduleDate.Kind == DateTimeKind.Utc 
                ? orderDto.DeliveryScheduleDate 
                : DateTime.SpecifyKind(orderDto.DeliveryScheduleDate, DateTimeKind.Utc);

            var utcNow = DateTime.UtcNow; // Always use UTC for timestamp with time zone

            // Set appropriate status based on payment method using actual DB status names
            var paymentMethod = orderDto.PaymentDetails?.FirstOrDefault()?.PaymentMethod?.ToLower();
            var orderStatus = "Pending Confirmation"; // Default to actual DB status
            var statusEnum = Models.Enums.OrderStatusEnum.Pending;
            var statusMessage = "Order created";

            if (paymentMethod == "cash on delivery" || paymentMethod == "cod")
            {
                orderStatus = "Pending Confirmation"; // COD orders start with confirmation pending
                statusEnum = Models.Enums.OrderStatusEnum.Pending;
                statusMessage = "Cash on Delivery order created - awaiting confirmation";
            }
            else if (payment.Status == "Success")
            {
                orderStatus = "Processing"; // Paid orders start processing
                statusEnum = Models.Enums.OrderStatusEnum.PaymentProcessing;
                statusMessage = "Payment confirmed - order processing";
            }

            return new Models.Order
            {
                OrderID = orderDto.OrderID ?? throw new Exception("Order ID is required"),
                ApplicationUserId = applicationUserId, // Store ApplicationUserId for Identity system
                OrderDate = orderDate,    // Use UTC DateTime
                DeliveryScheduleDate = deliveryDate, // Use UTC DateTime
                OrderedBy = orderDto.OrderedBy ?? "Unknown",
                Status = orderStatus, // Use actual DB status names
                PaymentID = payment.PaymentID,
                TotalOrderAmount = orderDto.TotalOrderAmount,
                TotalPaymentAmount = orderDto.TotalPaymentAmount,
                TotalDeliveryFees = orderDto.TotalDeliveryFees,
                TotalTax = orderDto.TotalTax,
                PaymentDetailsJson = JsonConvert.SerializeObject(orderDto.PaymentDetails),
                ProductsJson = JsonConvert.SerializeObject(orderDto.Products),

                OrderProducts = orderDto.Products.Select(p => 
                {
                    if(!productMerchants.TryGetValue(p.ProductID,out var merchantId))
                        throw new Exception($"Merchant not found for Product ID {p.ProductID}");


                return new OrderProduct
                {

                    ProductId = p.ProductID,
                    Quantity = p.Quantity,
                    OrderID = orderDto.OrderID,
                    MerchantID = merchantId,
                    TotalPrice = (decimal)(p.Price * p.Quantity),
                    Status = statusEnum, // Use corresponding enum
                    CreatedOn = utcNow,  // Use UTC DateTime
                    UpdatedOn = utcNow   // Use UTC DateTime
                };

                }).ToList(),

                ShippingAddress = JsonConvert.SerializeObject(orderDto.ShippingAddress),
                PickupLocation = JsonConvert.SerializeObject(orderDto.PickUpLocation),
                StatusEnum = statusEnum,
                StatusMessage = statusMessage,
                PaymentConfirmation = payment.Status == "Success" ? "Confirmed" : "Pending",
            };
        }

        private async Task UpdateProductStock(List<OrderProductsDTO> products)
        {
            foreach (var product in products)
            {
                // Use a more direct approach that doesn't involve navigation properties
                var stockUpdateQuery = @"
                    UPDATE ""Products"" 
                    SET ""StockQuantity"" = ""StockQuantity"" - @Quantity 
                    WHERE ""ProductId"" = @ProductId AND ""StockQuantity"" >= @Quantity";

                var parameters = new[]
                {
                    new NpgsqlParameter("@ProductId", product.ProductID),
                    new NpgsqlParameter("@Quantity", product.Quantity)
                };

                var rowsAffected = await _dbContext.Database.ExecuteSqlRawAsync(stockUpdateQuery, parameters);

                if (rowsAffected == 0)
                {
                    // Check if product exists and get current stock for better error message
                    var productInfo = await _dbContext.Products
                        .Where(p => p.ProductId == product.ProductID)
                        .Select(p => new { p.ProductName, p.StockQuantity })
                        .FirstOrDefaultAsync();

                    if (productInfo == null)
                    {
                        throw new Exception($"Product ID {product.ProductID} not found.");
                    }
                    else
                    {
                        throw new Exception($"Insufficient stock for {productInfo.ProductName}. Available: {productInfo.StockQuantity}, Requested: {product.Quantity}");
                    }
                }
            }
        }

        private async Task<Status> TrackOrderAsync(Models.Order order)
        {
            try
            {
                var utcNow = DateTime.UtcNow;

                // Determine initial status based on payment confirmation
                var isPaymentConfirmed = order.PaymentConfirmation == "Confirmed";
                var initialStatusName = isPaymentConfirmed ? "Processing" : "Pending Confirmation";
                var trackingDescription = isPaymentConfirmed
                    ? $"Order {order.OrderID} is being processed"
                    : $"Order {order.OrderID} is pending confirmation";

                // Get the static status from OrderStatuses lookup table
                var orderStatusRecord = await _dbContext.OrderStatuses
                    .Where(os => os.Status == initialStatusName)
                    .FirstOrDefaultAsync();

                if (orderStatusRecord == null)
                {
                    // If specific status not found, default to "Pending Confirmation"
                    orderStatusRecord = await _dbContext.OrderStatuses
                        .Where(os => os.Status == "Pending Confirmation")
                        .FirstOrDefaultAsync();
                        
                    if (orderStatusRecord == null)
                    {
                        _logger.LogError("No OrderStatus found for 'Pending Confirmation'. Please check your OrderStatuses table.");
                        throw new InvalidOperationException("OrderStatus lookup failed - missing required statuses.");
                    }
                }

                // Update the order's StatusID
                order.StatusID = orderStatusRecord.StatusID;
                order.Status = initialStatusName;
                order.StatusMessage = trackingDescription;

                // Determine createdBy from Identity User
                string createdBy = "System";
                var identityUser = await _dbContext.Users
                    .Where(u => u.Id == order.ApplicationUserId)
                    .Select(u => u.DisplayName ?? u.UserName ?? u.Email)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(identityUser))
                    createdBy = identityUser;

                // Create tracking records for each product
                foreach (var product in order.OrderProducts)
                {
                    var trackingId = $"TRK-{Guid.NewGuid():N}".Substring(0, 12);
                    var expectedDelivery = utcNow.AddDays(isPaymentConfirmed ? 3 : 5);

                    var newOrderTrack = new OrderTracking
                    {
                        TrackingID = trackingId,
                        OrderID = order.OrderID,
                        ProductId = product.ProductId,
                        MerchantID = product.MerchantID,
                        CurrentStatus = initialStatusName,
                        PreviousStatus = initialStatusName,
                        TrackingDate = utcNow,
                        ExpectedDeliveryDate = expectedDelivery,
                        Carrier = "Standard Delivery",
                        Location = "Warehouse",
                        TrackingNotes = trackingDescription,
                        CreatedOn = utcNow,
                        CreatedBy = createdBy,
                        UpdatedBy = createdBy,
                        UpdatedOn = utcNow
                    };

                    _dbContext.OrderTracking.Add(newOrderTrack);
                }

                // Save order and tracking records
                await _dbContext.SaveChangesAsync();

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Order Tracking Created Successfully for All Products"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order tracking for order {OrderID}", order.OrderID);
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = $"Error creating tracking: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Maps status string to OrderStatusEnum based on actual DB values
        /// </summary>
        private Models.Enums.OrderStatusEnum MapStringToOrderStatusEnum(string statusString)
        {
            return statusString?.Trim() switch
            {
                "Processing" => Models.Enums.OrderStatusEnum.PaymentProcessing,
                "Pending Confirmation" => Models.Enums.OrderStatusEnum.Pending,
                "Confirmed" => Models.Enums.OrderStatusEnum.Paid,
                "Shipped" => Models.Enums.OrderStatusEnum.Shipped,
                "Pickup" => Models.Enums.OrderStatusEnum.Shipped, // Map Pickup to Shipped for enum compatibility
                "Delivered" => Models.Enums.OrderStatusEnum.Delivered,
                "Cancelled" => Models.Enums.OrderStatusEnum.Cancelled,
                "Returned" => Models.Enums.OrderStatusEnum.Refunded, // Map Returned to Refunded for enum compatibility
                "Refunded" => Models.Enums.OrderStatusEnum.Refunded,
                _ => Models.Enums.OrderStatusEnum.Pending
            };
        }

        /// <summary>
        /// Maps OrderStatusEnum to actual database status strings
        /// </summary>
        private string MapOrderStatusEnumToString(Models.Enums.OrderStatusEnum statusEnum)
        {
            return statusEnum switch
            {
                Models.Enums.OrderStatusEnum.Pending => "Pending Confirmation",
                Models.Enums.OrderStatusEnum.PaymentProcessing => "Processing",
                Models.Enums.OrderStatusEnum.Paid => "Confirmed",
                Models.Enums.OrderStatusEnum.Shipped => "Shipped",
                Models.Enums.OrderStatusEnum.Delivered => "Delivered",
                Models.Enums.OrderStatusEnum.Cancelled => "Cancelled",
                Models.Enums.OrderStatusEnum.Refunded => "Refunded",
                Models.Enums.OrderStatusEnum.Failed => "Cancelled", // Map Failed to Cancelled as closest match
                _ => "Pending Confirmation"
            };
        }

        /// <summary>
        /// Updates the overall order status based on all product statuses
        /// </summary>
        private async Task UpdateOverallOrderStatusAsync(string orderId, string updatedBy, DateTime updateTime)
        {
            // Get all products in this order with their current statuses
            var orderProducts = await _dbContext.OrderProducts
                .Where(op => op.OrderID == orderId)
                .ToListAsync();

            if (!orderProducts.Any())
            {
                _logger.LogWarning("No products found for order {OrderId}", orderId);
                return;
            }

            // Calculate overall status based on product statuses
            var overallStatus = CalculateOverallOrderStatus(orderProducts);
            var overallStatusString = MapOrderStatusEnumToString(overallStatus);

            // Get the main order
            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.OrderID == orderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", orderId);
                return;
            }

            // Only update if status has changed
            if (order.StatusEnum != overallStatus)
            {
                var previousStatus = order.Status;
                
                order.StatusEnum = overallStatus;
                order.Status = overallStatusString;
                order.StatusMessage = GenerateStatusMessage(overallStatus, orderProducts, previousStatus);

                // Find corresponding StatusID from OrderStatuses table
                var orderStatusRecord = await _dbContext.OrderStatuses
                    .Where(os => os.Status == overallStatusString)
                    .FirstOrDefaultAsync();

                if (orderStatusRecord != null)
                {
                    order.StatusID = orderStatusRecord.StatusID;
                }

                _dbContext.Orders.Update(order);
                
                // Log the status change
                _logger.LogInformation("Order {OrderId} status updated from {PreviousStatus} to {NewStatus} by {UpdatedBy}", 
                    orderId, previousStatus, overallStatusString, updatedBy);
            }
        }

        /// <summary>
        /// Calculates the overall order status based on all product statuses using actual DB status flow
        /// </summary>
        private Models.Enums.OrderStatusEnum CalculateOverallOrderStatus(List<OrderProduct> orderProducts)
        {
            var statuses = orderProducts.Select(op => op.Status).ToList();

            // If any product is cancelled, and no products are delivered, order is cancelled
            if (statuses.All(s => s == Models.Enums.OrderStatusEnum.Cancelled))
            {
                return Models.Enums.OrderStatusEnum.Cancelled;
            }

            // If any product failed and others aren't delivered
            if (statuses.Any(s => s == Models.Enums.OrderStatusEnum.Failed) && 
                !statuses.Any(s => s == Models.Enums.OrderStatusEnum.Delivered))
            {
                return Models.Enums.OrderStatusEnum.Failed;
            }

            // If ALL products are delivered, order is delivered (COMPLETE) ✅
            if (statuses.All(s => s == Models.Enums.OrderStatusEnum.Delivered))
            {
                return Models.Enums.OrderStatusEnum.Delivered;
            }

            // If any product is shipped/pickup (but not all delivered), order is shipped
            if (statuses.Any(s => s == Models.Enums.OrderStatusEnum.Shipped))
            {
                return Models.Enums.OrderStatusEnum.Shipped;
            }

            // If any product is confirmed/paid (but none shipped/delivered), order is confirmed
            if (statuses.Any(s => s == Models.Enums.OrderStatusEnum.Paid))
            {
                return Models.Enums.OrderStatusEnum.Paid;
            }

            // If any product is processing, order is processing
            if (statuses.Any(s => s == Models.Enums.OrderStatusEnum.PaymentProcessing))
            {
                return Models.Enums.OrderStatusEnum.PaymentProcessing;
            }

            // Default to pending confirmation
            return Models.Enums.OrderStatusEnum.Pending;
        }

        /// <summary>
        /// Generates status message based on actual DB status flow
        /// </summary>
        private string GenerateStatusMessage(Models.Enums.OrderStatusEnum overallStatus, 
            List<OrderProduct> orderProducts, string previousStatus)
        {
            var totalProducts = orderProducts.Count;
            var statusCounts = orderProducts.GroupBy(op => op.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            return overallStatus switch
            {
                Models.Enums.OrderStatusEnum.Delivered => 
                    $"Order completed - All {totalProducts} products delivered",
                
                Models.Enums.OrderStatusEnum.Shipped => 
                    $"Order in transit - {statusCounts.GetValueOrDefault(Models.Enums.OrderStatusEnum.Shipped, 0)} of {totalProducts} products shipped/ready for pickup",
                
                Models.Enums.OrderStatusEnum.Paid => 
                    $"Order confirmed - {statusCounts.GetValueOrDefault(Models.Enums.OrderStatusEnum.Paid, 0)} of {totalProducts} products confirmed and ready for shipping",
                
                Models.Enums.OrderStatusEnum.PaymentProcessing => 
                    $"Order processing - {statusCounts.GetValueOrDefault(Models.Enums.OrderStatusEnum.PaymentProcessing, 0)} of {totalProducts} products being processed",
                
                Models.Enums.OrderStatusEnum.Pending => 
                    $"Order pending confirmation - {statusCounts.GetValueOrDefault(Models.Enums.OrderStatusEnum.Pending, 0)} of {totalProducts} products awaiting confirmation",
                
                Models.Enums.OrderStatusEnum.Cancelled => 
                    "Order cancelled - All products cancelled",
                
                Models.Enums.OrderStatusEnum.Refunded => 
                    "Order refunded - Products returned/refunded",
                
                _ => $"Order status updated from {previousStatus}"
            };
        }

        #endregion

        #region Status Methods

        // Return list of available order statuses
        public async Task<List<OrderStatus>> GetOrderStatusAsync()
        {
            try
            {
                // Execute the query asynchronously and materialize the results into a list
                var orderStatusList = await _dbContext.OrderStatuses
                    .Select(o => new OrderStatus
                    {
                        StatusID = o.StatusID,
                        Status = o.Status,
                        Description = o.Description,
                        CreatedBy = o.CreatedBy,
                        CreatedOn = o.CreatedOn,
                        UpdatedBy = o.UpdatedBy,
                        UpdatedOn = o.UpdatedOn,
                    })
                    .ToListAsync();

                return orderStatusList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching order statuses.");
                return new List<OrderStatus>();
            }
        }

        #endregion
    }
}
