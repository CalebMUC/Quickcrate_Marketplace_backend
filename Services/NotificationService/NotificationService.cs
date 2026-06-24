using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Minimart_Api.DTOS.Notification;
using Minimart_Api.DTOS.Orders;

namespace Minimart_Api.Services.NotificationService
{
    public class NotificationService : INotfication
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly CelcomAfrica _celcomAfrica;
        public NotificationService(ILogger<NotificationService> logger , IOptions<CelcomAfrica> celcomAfrica) { 

            _logger = logger;
            _celcomAfrica = celcomAfrica.Value;
    }

        public async Task NotifyCustomer(OrderEvent orderEvent) {

            var url = "https://isms.celcomafrica.com/api/services/sendsms/";

            // Create an instance of HttpClient
            using (var client = new HttpClient())
            {
                // Set the base address of the API
                client.BaseAddress = new Uri(url);



                // Construct the message body
                var messageBody = $"Dear {orderEvent.MerchantName}, " +
                                  $"Order #{orderEvent.OrderID} has been placed with you. " +
                                  $"Total Items: {orderEvent.products.Count} | Total Amount: KES {orderEvent.Amount:N2}\n" +
                                  $"View full order details: https://minimartke.com/merchant/orders/{orderEvent.OrderID}";

                // Set the Content-Type header to application/json
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Create the request body
                var requestBody = new
                {
                    partnerID = "36",
                    apikey = _celcomAfrica.Apikey,
                    mobile = orderEvent.UserPhoneNumber,
                    message = messageBody,
                    shortcode = "TEXTME",
                    pass_type = "plain" // bm5 {base64 encode} or plain
                };

                // Serialize the request body to JSON
                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(url, jsonContent);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    string responseData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseData);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }



        }

        public async Task NotifyMerchants(OrderEvent orderEvent)
        {
            var url = "https://isms.celcomafrica.com/api/services/sendsms/";

            // Construct the message body
            var messageBody = $"Dear {orderEvent.MerchantName}, " +
                              $"Order #{orderEvent.OrderID} has been placed with you. " +
                              $"Total Items: {orderEvent.products.Count} | Total Amount: KES {orderEvent.Amount:N2}\n" +
                              $"View full order details: https://minimartke.com/merchant/orders/{orderEvent.OrderID}";

            // Create an instance of HttpClient
            using (var client = new HttpClient())
            {
                // Set the base address of the API
                client.BaseAddress = new Uri(url);

                // Set the Content-Type header to application/json
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                // Create the request body
                var requestBody = new
                {
                    partnerID = "36",
                    apikey = _celcomAfrica.Apikey,
                    mobile = orderEvent.MerchantPhoneNumber,
                    message = messageBody,
                    shortcode = "TEXTME",
                    pass_type = "plain" // bm5 {base64 encode} or plain
                };

                // Serialize the request body to JSON
                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                // Send the POST request
                HttpResponseMessage response = await client.PostAsync(url, jsonContent);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    string responseData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseData);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                }
            }
        }
    }
}
