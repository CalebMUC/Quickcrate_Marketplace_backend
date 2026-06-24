using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Payouts
{
    /// <summary>
    /// Payout statistics for merchant dashboard
    /// </summary>
    public class PayoutStatsDto
    {
        public decimal TotalEarnings { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal CompletedAmount { get; set; }
        public decimal TotalCommissionPaid { get; set; }
        public int TotalPayouts { get; set; }
        public int PendingPayouts { get; set; }
        public int CompletedPayouts { get; set; }
        public decimal AveragePayoutAmount { get; set; }
        public DateTime? LastPayoutDate { get; set; }
        public DateTime? NextScheduledPayoutDate { get; set; }
    }

    /// <summary>
    /// Basic payout information for listing
    /// </summary>
    public class PayoutDto
    {
        public Guid PayoutId { get; set; }
        public Guid MerchantId { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public decimal GrossAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int OrderCount { get; set; }
        public int ProductCount { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? Notes { get; set; }
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// Detailed payout information
    /// </summary>
    public class PayoutDetailDto : PayoutDto
    {
        public List<PayoutTransactionDto> Transactions { get; set; } = new List<PayoutTransactionDto>();
        public string? ExternalPaymentReference { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// Individual payout transaction information
    /// </summary>
    public class PayoutTransactionDto
    {
        public Guid PayoutTransactionId { get; set; }
        public Guid PayoutId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public decimal OrderAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal CommissionRate { get; set; }
        public DateTime OrderCompletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CustomerName { get; set; }
        public string? OrderStatus { get; set; }
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// Request to generate payouts for a period
    /// </summary>
    public class GeneratePayoutsRequest
    {
        [Required]
        public DateTime PeriodStartDate { get; set; }

        [Required]
        public DateTime PeriodEndDate { get; set; }

        /// <summary>
        /// If true, process payouts immediately. If false, schedule for later.
        /// </summary>
        public bool ProcessImmediately { get; set; } = false;

        /// <summary>
        /// Optional: Specific merchant IDs to generate payouts for
        /// If empty, generate for all eligible merchants
        /// </summary>
        public List<Guid>? MerchantIds { get; set; }

        /// <summary>
        /// Commission rate to apply (if different from default)
        /// </summary>
        [Range(0, 1)]
        public decimal? CommissionRate { get; set; }

        /// <summary>
        /// Optional notes to add to generated payouts
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Response from payout generation
    /// </summary>
    public class GeneratePayoutsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int PayoutsGenerated { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalCommissionAmount { get; set; }
        public decimal TotalNetAmount { get; set; }
        public List<PayoutDto> GeneratedPayouts { get; set; } = new List<PayoutDto>();
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Request to update payout status
    /// </summary>
    public class UpdatePayoutStatusRequest
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }

        [StringLength(255)]
        public string? ExternalPaymentReference { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Payout filters for queries
    /// </summary>
    public class PayoutFilters
    {
        public Guid? MerchantId { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; } = "CreatedDate";
        public string SortOrder { get; set; } = "desc";
    }

    /// <summary>
    /// Paged result wrapper
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;
    }

    /// <summary>
    /// Merchant payout summary for admin dashboard
    /// </summary>
    public class MerchantPayoutSummaryDto
    {
        public Guid MerchantId { get; set; }
        public string MerchantName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public decimal TotalEarnings { get; set; }
        public decimal TotalCommissionPaid { get; set; }
        public int TotalPayouts { get; set; }
        public int PendingPayouts { get; set; }
        public DateTime? LastPayoutDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}