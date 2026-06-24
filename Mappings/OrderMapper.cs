using Minimart_Api.DTOS.Address;
using Minimart_Api.DTOS.Orders;
using Minimart_Api.Models;
using Newtonsoft.Json;

namespace Minimart_Api.Mappings
{
    public class OrderMapper
    {

        public static class ReportConfiguration
        {
            //map report types to thier actual Parameters
            public static readonly Dictionary<string, List<string>> ReportParameterMappings = new Dictionary<string, List<string>> {
            {"Sales Report", new List<string>{"FromDate","ToDate" } },
            {"Product Report", new List<string>{"fromDate","toDate","category" } },
            {"Customer Report", new List<string>{"fromDate","toDate", "customerID" } }
        };

        }
        // Mapping Order to OrderDTO
        public OrderDTO MapToDto(Order order)
        {
            return new OrderDTO
            {
                OrderID = order.OrderID,
                ApplicationUserId = order.ApplicationUserId ?? string.Empty, // Updated to use ApplicationUserId
                OrderDate = order.OrderDate, // Use actual order date
                DeliveryScheduleDate = order.DeliveryScheduleDate,
                OrderedBy = order.OrderedBy,
                Status = order.Status,
                PaymentConfirmation = order.PaymentConfirmation,
                TotalOrderAmount = order.TotalOrderAmount,
                TotalPaymentAmount = order.TotalPaymentAmount,
                TotalDeliveryFees = order.TotalDeliveryFees,
                TotalTax = order.TotalTax,
                ShippingAddress = JsonConvert.DeserializeObject<ShippingAddress>(order.ShippingAddress ?? "{}"),
                Products = JsonConvert.DeserializeObject<List<OrderProductsDTO>>(order.ProductsJson ?? "[]"),
                PickUpLocation = JsonConvert.DeserializeObject<PickUpLocation>(order.PickupLocation ?? "{}"),
                //PaymentDetails = JsonConvert.DeserializeObject<PaymentDetailsDto>(order.PaymentDetailsJson)
            };
        }

        // Mapping OrderDTO back to Order
        public Order MapToEntity(OrderDTO orderDto)
        {
            return new Order
            {
                OrderID = orderDto.OrderID ?? string.Empty,
                ApplicationUserId = orderDto.ApplicationUserId, // Updated to use ApplicationUserId
                OrderDate = orderDto.OrderDate,
                DeliveryScheduleDate = orderDto.DeliveryScheduleDate,
                OrderedBy = orderDto.OrderedBy,
                Status = orderDto.Status,

                // Map PaymentDetails JSON
                PaymentDetailsJson = orderDto.PaymentDetails != null ? 
                    JsonConvert.SerializeObject(orderDto.PaymentDetails.Select(pd => new
                    {
                        PaymentID = pd.PaymentID,
                        Amount = pd.Amount,
                        PaymentDate = DateTime.UtcNow
                    })) : "[]",

                // Map Products JSON
                ProductsJson = orderDto.Products != null ? 
                    JsonConvert.SerializeObject(orderDto.Products.Select(p => new
                    {
                        ProductName = p.ProductName ?? string.Empty,
                        ProductID = p.ProductID,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        Discount = p.Discount,
                    }).ToList()) : "[]",

                // Map the collection of OrderProducts
                OrderProducts = orderDto.Products?.Select(productDto => new OrderProduct
                {
                    ProductId = productDto.ProductID,
                    Quantity = productDto.Quantity,
                    OrderID = orderDto.OrderID ?? string.Empty,
                    MerchantID = productDto.merchantId
                }).ToList() ?? new List<OrderProduct>(),

                // Map Payment Confirmation
                PaymentConfirmation = orderDto.PaymentConfirmation ?? string.Empty,
                TotalOrderAmount = orderDto.TotalOrderAmount,
                TotalPaymentAmount = orderDto.TotalPaymentAmount,
                TotalDeliveryFees = orderDto.TotalDeliveryFees,
                TotalTax = orderDto.TotalTax,

                // Map ShippingAddress JSON
                ShippingAddress = orderDto.ShippingAddress != null ? JsonConvert.SerializeObject(new ShippingAddress
                {
                    Address = orderDto.ShippingAddress.Address ?? string.Empty,
                    County = orderDto.ShippingAddress.County ?? string.Empty,
                    Town = orderDto.ShippingAddress.Town ?? string.Empty,
                    PostalCode = orderDto.ShippingAddress.PostalCode ?? string.Empty,
                    Name = orderDto.ShippingAddress.Name ?? string.Empty,
                    Phonenumber = orderDto.ShippingAddress.Phonenumber ?? string.Empty
                }) : "{}",

                // Map PickupLocation JSON
                PickupLocation = orderDto.PickUpLocation != null ? JsonConvert.SerializeObject(new PickUpLocation
                {
                    countyId = orderDto.PickUpLocation.countyId,
                    townId = orderDto.PickUpLocation.townId,
                    deliveryStationId = orderDto.PickUpLocation.deliveryStationId
                }) : "{}"
            };
        }
    }
}
