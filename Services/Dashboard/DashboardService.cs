using Minimart_Api.DTOS.Dashboard;
using Minimart_Api.Repositories.Dashboard;

namespace Minimart_Api.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepo _dashboardRepo;
        
        public DashboardService(IDashboardRepo dashboardRepo)
        {
            _dashboardRepo = dashboardRepo;
        }
        
        public async Task<MerchantDashboardSummary> GetMerchantDashboardSummary(string merchantId) 
        { 
            var summary = await _dashboardRepo.GetMerchantDashboardSummary(merchantId);
            return summary;
        }

        public async Task<List<SalesDataPoint>> GetSalesDataAsync(string merchantId, string period)
        {
            var salesData = await _dashboardRepo.GetSalesDataAsync(merchantId, period);
            return salesData;
        }

        public async Task<List<RecentOrderDto>> GetRecentOrdersAsync(string merchantId, int limit)
        {
            var recentOrders = await _dashboardRepo.GetRecentOrdersAsync(merchantId, limit);
            return recentOrders;
        }

        public async Task<List<OrderStatusCount>> GetOrderStatusDistributionAsync(string merchantId)
        {
            var statusDistribution = await _dashboardRepo.GetOrderStatusDistributionAsync(merchantId);
            return statusDistribution;
        }

        public async Task<List<TopProductDto>> GetTopProductsAsync(string merchantId, int limit, string period)
        {
            var topProducts = await _dashboardRepo.GetTopProductsAsync(merchantId, limit, period);
            return topProducts;
        }

        public async Task<List<PaymentMethodStats>> GetPaymentMethodsDistributionAsync(string merchantId)
        {
            var paymentStats = await _dashboardRepo.GetPaymentMethodsDistributionAsync(merchantId);
            return paymentStats;
        }
    }
}
