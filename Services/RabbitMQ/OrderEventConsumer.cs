
using System.Text;
using System.Text.Json;
using Minimart_Api.DTOS.Orders;
using Minimart_Api.Services.NotificationService;
using RabbitMQ.Client.Events;

namespace Minimart_Api.Services.RabbitMQ
{
    public class OrderEventConsumer : BackgroundService
    {
        private readonly IRabbitMqConnection _connection;
        private readonly ILogger<OrderEventConsumer> _logger;
        private IServiceScopeFactory _scopeFactory;

        public OrderEventConsumer(IRabbitMqConnection connection, ILogger<OrderEventConsumer> logger, IServiceScopeFactory scopeFactory) { 

            _scopeFactory = scopeFactory;
            _connection = connection;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consumer is starting");
            
            try
            {
                //create a scope to resolve services
                var scope = _scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotfication>();
                //Create Channel Async
                var connection = await _connection.GetConnectionAsync();

                var channel = await  connection.CreateChannelAsync();

                //Declare Queue
                await channel.QueueDeclareAsync(
                    queue: "orderEvent_queue",
                    durable: true,
                    autoDelete: false,
                    exclusive: false,
                    arguments: null
                );

                //Create Consumer
                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    //stop processing if the service is stopping
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Consumer service is stopping");
                        return;
                    }
                    _logger.LogInformation("Received Event from the queue.");

                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation($"Raw message received: {message}");

                    try
                    {
                        //deserialize orderEvet - use fully qualified DTO name
                        var orderEvent = JsonSerializer.Deserialize<Minimart_Api.DTOS.Orders.OrderEvent>(message);

                        if (orderEvent == null)
                        {
                            _logger.LogWarning("message is malformed");
                            await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag,
                                multiple: false,
                               requeue: false
                                );
                            return;
                        }

                        _logger.LogInformation($"Processing {orderEvent}");



                        await notificationService.NotifyMerchants(orderEvent);

                        await notificationService.NotifyCustomer(orderEvent);


                        //acknowlwdge the message after succesfull processing
                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag,
                            multiple: false);

                        _logger.LogInformation($"Event {orderEvent} was successfully Processed on {DateTime.Now}");




                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing message: {ex.Message}");

                        // Nack the message so it can be requeued in case of failure
                        await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                    }

                };

                //start Consuming message from the Queue
                await channel.BasicConsumeAsync(
                    queue: "orderEvent_queue",
                    autoAck: false,
                    consumerTag: "",
                    noLocal: false,
                    exclusive: false,
                    arguments: null,
                    consumer: consumer
                    );
                _logger.LogInformation("Consumer is now listening for messages.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in consumer initialization: {ex.Message}");
                throw;
            }


            // Keep the service running while the cancellation token is not triggered
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
