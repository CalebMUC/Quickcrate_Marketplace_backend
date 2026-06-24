using Minimart_Api.DTOS.Dashboard;

namespace Minimart_Api.Services.Dashboard
{
    public interface IDashboardService
    {
        public Task<MerchantDashboardSummary> GetMerchantDashboardSummary(string merchantId);
        Task<List<SalesDataPoint>> GetSalesDataAsync(string merchantId, string period);
        Task<List<RecentOrderDto>> GetRecentOrdersAsync(string merchantId, int limit);
        Task<List<OrderStatusCount>> GetOrderStatusDistributionAsync(string merchantId);
        Task<List<TopProductDto>> GetTopProductsAsync(string merchantId, int limit, string period);
        Task<List<PaymentMethodStats>> GetPaymentMethodsDistributionAsync(string merchantId);
    }
}
