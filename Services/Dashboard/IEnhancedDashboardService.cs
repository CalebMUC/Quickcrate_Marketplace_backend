using Minimart_Api.DTOS.Dashboard;

namespace Minimart_Api.Services.Dashboard
{
    public interface IEnhancedDashboardService
    {
        // Admin Dashboard Methods - ENHANCED WITH MERCHANT STATISTICS
        Task<AdminDashboardSummary> GetAdminDashboardSummaryAsync();
        Task<Dictionary<string, object>> GetAdminMetricsAsync();
        Task<List<SalesDataPoint>> GetAdminSalesDataAsync(string period = "month");
        Task<List<OrderStatusCount>> GetAdminOrderStatusDistributionAsync();
        Task<List<RecentOrderDto>> GetAdminRecentOrdersAsync(int limit = 10);
        Task<List<TopProductDto>> GetAdminTopProductsAsync(int limit = 5, string period = "month");
        Task<List<PaymentMethodStats>> GetAdminPaymentMethodsDistributionAsync();
        
        // NEW ADMIN MERCHANT-SPECIFIC METHODS
        //Task<List<TopMerchantDto>> GetTopPerformingMerchantsAsync(int limit = 10, string period = "month");
        Task<List<NewMerchantDto>> GetNewMerchantsAsync(int limit = 10, int daysBack = 30);
        //Task<Dictionary<string, object>> GetMerchantRevenueStatsAsync();
        Task<List<SalesDataPoint>> GetPlatformRevenueDataAsync(string period = "month");
        Task<Dictionary<string, int>> GetMerchantStatusDistributionAsync();
        //Task<List<TopMerchantDto>> GetMerchantsByGrowthAsync(int limit = 10, string period = "month");
        
        // Merchant Dashboard Methods (unchanged)
        Task<MerchantDashboardSummary> GetMerchantDashboardSummaryAsync(Guid merchantId);
        Task<Dictionary<string, object>> GetMerchantMetricsAsync(Guid merchantId);
        Task<List<SalesDataPoint>> GetMerchantSalesDataAsync(Guid merchantId, string period = "month");
        Task<List<OrderStatusCount>> GetMerchantOrderStatusDistributionAsync(Guid merchantId);
        Task<List<RecentOrderDto>> GetMerchantRecentOrdersAsync(Guid merchantId, int limit = 10);
        Task<List<TopProductDto>> GetMerchantTopProductsAsync(Guid merchantId, int limit = 5, string period = "month");
        Task<List<PaymentMethodStats>> GetMerchantPaymentMethodsDistributionAsync(Guid merchantId);
        
        // Staff Dashboard Methods (unchanged)
        Task<MerchantDashboardSummary> GetStaffDashboardSummaryAsync();
        Task<List<RecentOrderDto>> GetStaffRecentOrdersAsync(int limit = 10);
        Task<List<OrderStatusCount>> GetStaffOrderStatusDistributionAsync();
        
        // Utility Methods
        Task<bool> ValidateMerchantAccessAsync(Guid merchantId, string userId);
        void ClearDashboardCache();
        void ClearMerchantDashboardCache(Guid merchantId);
    }
}