using RabbitMQ.Client;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace Minimart_Api.Services.RabbitMQ
{
    public class RabbitMqConnection : IRabbitMqConnection, IAsyncDisposable
    {
        private IConnection? _connection;
        private readonly ILogger<RabbitMqConnection> _logger;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);

        public RabbitMqConnection(ILogger<RabbitMqConnection> logger)
        {
            _logger = logger;
        }

        public bool IsConnected => _connection?.IsOpen == true;

        public async Task<IConnection> GetConnectionAsync()
        {
            if (IsConnected)
                return _connection!;

            await _connectionLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (IsConnected)
                    return _connection!;

                _connection = await CreatePersistentConnectionAsync();
                return _connection;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task<IConnection> CreatePersistentConnectionAsync()
        {
            int retryCount = 0;
            const int maxRetries = 5;
            TimeSpan initialDelay = TimeSpan.FromSeconds(2);

            while (retryCount < maxRetries)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(Environment.GetEnvironmentVariable("RABBITMQ_URI") ?? "amqp://guest:guest@localhost:5672"),
                        //DispatchConsumersAsync = true,
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                        RequestedHeartbeat = TimeSpan.FromSeconds(30)
                    };

                    _logger.LogInformation("Connecting to RabbitMQ at {Host}", factory.Uri.Host);
                    return await factory.CreateConnectionAsync();
                }
                catch (SocketException ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex,
                        "RabbitMQ connection attempt {RetryCount} failed. Retrying in {Delay}s...",
                        retryCount, initialDelay.TotalSeconds);

                    if (retryCount == maxRetries)
                    {
                        _logger.LogError(ex, "Failed to connect to RabbitMQ after {MaxRetries} attempts", maxRetries);
                        throw;
                    }

                    await Task.Delay(initialDelay);
                    initialDelay = TimeSpan.FromSeconds(initialDelay.TotalSeconds * 2);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Unhandled RabbitMQ connection error");
                    throw;
                }
            }

            throw new InvalidOperationException("RabbitMQ connection could not be established");
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection == null) return;

            try
            {
                await _connection.CloseAsync();
                _connection.Dispose();
                _logger.LogInformation("RabbitMQ connection disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ connection");
            }
            finally
            {
                _connectionLock.Dispose();
            }
        }
    }
}