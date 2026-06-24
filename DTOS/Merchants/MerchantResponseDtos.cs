using Minimart_Api.DTOS.Merchants;

namespace Minimart_Api.DTOS.Merchants
{
    /// <summary>
    /// Detailed merchant DTO that matches frontend Merchant interface
    /// </summary>
    public class MerchantDetailDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string BusinessRegistration { get; set; } = string.Empty;
        public string? TaxId { get; set; }
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<string>? Documents { get; set; }
        public BankDetailsDto? BankDetails { get; set; }
        public List<MerchantPaymentMethodDetailDto>? PaymentMethods { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
        public string? RejectionReason { get; set; }

        // Additional fields from existing model
        public string? BusinessNature { get; set; }
        public string? BusinessCategory { get; set; }
        public string? BusinessType { get; set; }
        public string? SocialMedia { get; set; }
        public string? PreferredPaymentChannel { get; set; }
        public string? DeliveryMethod { get; set; }
        public bool? ReturnPolicy { get; set; }
        public bool? TermsAndCondition { get; set; }
        public string? MpesaPaybill { get; set; }
        public string? MpesaTillNumber { get; set; }
    }

    /// <summary>
    /// Response DTO for merchants list with pagination
    /// </summary>
    public class MerchantsListResponse
    {
        public List<MerchantDetailDto> Merchants { get; set; } = new List<MerchantDetailDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    /// <summary>
    /// Filter parameters for merchants list
    /// </summary>
    public class MerchantFilters
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string SortBy { get; set; } = "businessName";
        public string SortOrder { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// Merchant statistics DTO
    /// </summary>
    public class MerchantStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Active { get; set; }
        public int Suspended { get; set; }
        public int Rejected { get; set; }
    }
}