using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Minimart_Api.Data;
using Minimart_Api.Services.OpenSearchService;

public class SyncProductsToOpenSearch : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncProductsToOpenSearch> _logger;

    public SyncProductsToOpenSearch(IServiceProvider serviceProvider, ILogger<SyncProductsToOpenSearch> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var openSearchService = scope.ServiceProvider.GetRequiredService<IOpenSearchService>();
                    var dbContext = scope.ServiceProvider.GetRequiredService<MinimartDBContext>();

                    var products = await dbContext.Products.ToListAsync();
                    foreach (var product in products)
                    {
                        await openSearchService.IndexProductAsync(product);
                    }

                    _logger.LogInformation("Synced {Count} products to OpenSearch", products.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing products to OpenSearch");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}