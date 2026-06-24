using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Merchants
{
    /// <summary>
    /// DTO for merchant registration that matches frontend MerchantRegistration interface
    /// </summary>
    public class MerchantRegistrationDto
    {
        [Required(ErrorMessage = "Business name is required")]
        [StringLength(255, ErrorMessage = "Business name cannot exceed 255 characters")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business registration number is required")]
        [StringLength(100, ErrorMessage = "Business registration cannot exceed 100 characters")]
        public string BusinessRegistration { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Tax ID cannot exceed 50 characters")]
        public string? TaxId { get; set; }

        [Required(ErrorMessage = "Contact person is required")]
        [StringLength(255, ErrorMessage = "Contact person cannot exceed 255 characters")]
        public string ContactPerson { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone format")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string Country { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }

        /// <summary>
        /// Business type
        /// </summary>
        [StringLength(100, ErrorMessage = "Business type cannot exceed 100 characters")]
        public string? BusinessType { get; set; }

        /// <summary>
        /// Business nature/category
        /// </summary>
        [StringLength(100, ErrorMessage = "Business nature cannot exceed 100 characters")]
        public string? BusinessNature { get; set; }

        /// <summary>
        /// Business category
        /// </summary>
        [StringLength(100, ErrorMessage = "Business category cannot exceed 100 characters")]
        public string? BusinessCategory { get; set; }

        /// <summary>
        /// Social media information
        /// </summary>
        [StringLength(500, ErrorMessage = "Social media cannot exceed 500 characters")]
        public string? SocialMedia { get; set; }

        /// <summary>
        /// Preferred payment channel
        /// </summary>
        [StringLength(100, ErrorMessage = "Preferred payment channel cannot exceed 100 characters")]
        public string? PreferredPaymentChannel { get; set; }

        /// <summary>
        /// Delivery method
        /// </summary>
        [StringLength(100, ErrorMessage = "Delivery method cannot exceed 100 characters")]
        public string? DeliveryMethod { get; set; }

        /// <summary>
        /// Return policy acceptance
        /// </summary>
        public bool? ReturnPolicy { get; set; }

        /// <summary>
        /// Terms and conditions acceptance
        /// </summary>
        public bool? TermsAndCondition { get; set; }

        /// <summary>
        /// Payment methods configuration for the merchant
        /// </summary>
        public List<MerchantPaymentMethodRegistrationDto>? PaymentMethods { get; set; }

        /// <summary>
        /// Document URLs (array of document URLs as strings)
        /// </summary>
        public List<string>? Documents { get; set; }
    }

    /// <summary>
    /// DTO for merchant payment method during registration
    /// </summary>
    public class MerchantPaymentMethodRegistrationDto
    {
        [Required(ErrorMessage = "Payment method ID is required")]
        public int PaymentMethodId { get; set; }

        public string PaymentMethodName { get; set; } = string.Empty;

        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// JSON configuration string for the payment method
        /// </summary>
        public string Configuration { get; set; } = string.Empty;

        /// <summary>
        /// Parsed account details from configuration
        /// </summary>
        public PaymentAccountDetailsDto? AccountDetails { get; set; }
    }

    /// <summary>
    /// DTO for payment account details
    /// </summary>
    public class PaymentAccountDetailsDto
    {
        public string? AccountNumber { get; set; }
        public string? AccountName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ApiKey { get; set; }
        public string? MerchantCode { get; set; }
        public string? AdditionalInfo { get; set; }
    }

    /// <summary>
    /// DTO for document metadata (for JSON requests)
    /// </summary>
    public class DocumentMetadataDto
    {
        public string? FileName { get; set; }
        public string? DocumentType { get; set; }
        public string? Base64Content { get; set; }
        public string? FileUrl { get; set; }
    }

    /// <summary>
    /// DTO for document URLs (simplified approach)
    /// </summary>
    public class DocumentUrlDto
    {
        public string Url { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DocumentType { get; set; }
    }

    /// <summary>
    /// DTO for merchant payment method details (response)
    /// </summary>
    public class MerchantPaymentMethodDetailDto
    {
        public int Id { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Parsed configuration as account details
        /// </summary>
        public PaymentAccountDetailsDto? AccountDetails
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Configuration))
                    return null;

                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<PaymentAccountDetailsDto>(Configuration);
                }
                catch
                {
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// Bank details DTO that matches frontend BankDetails interface
    /// </summary>
    public class BankDetailsDto
    {
        [Required(ErrorMessage = "Bank name is required")]
        [StringLength(255, ErrorMessage = "Bank name cannot exceed 255 characters")]
        public string BankName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account name is required")]
        [StringLength(255, ErrorMessage = "Account name cannot exceed 255 characters")]
        public string AccountName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account number is required")]
        [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters")]
        public string AccountNumber { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Bank code cannot exceed 20 characters")]
        public string? BankCode { get; set; }

        [StringLength(20, ErrorMessage = "SWIFT code cannot exceed 20 characters")]
        public string? SwiftCode { get; set; }
    }

    /// <summary>
    /// DTO for updating merchant information
    /// </summary>
    public class UpdateMerchantDto
    {
        public Guid Id { get; set; }

        [StringLength(255, ErrorMessage = "Business name cannot exceed 255 characters")]
        public string? BusinessName { get; set; }

        [StringLength(100, ErrorMessage = "Business registration cannot exceed 100 characters")]
        public string? BusinessRegistration { get; set; }

        [StringLength(50, ErrorMessage = "Tax ID cannot exceed 50 characters")]
        public string? TaxId { get; set; }

        [StringLength(255, ErrorMessage = "Contact person cannot exceed 255 characters")]
        public string? ContactPerson { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone format")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        public string? Phone { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        public string? Country { get; set; }

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        public string? PostalCode { get; set; }

        public BankDetailsDto? BankDetails { get; set; }

        [StringLength(100, ErrorMessage = "Business nature cannot exceed 100 characters")]
        public string? BusinessNature { get; set; }

        [StringLength(100, ErrorMessage = "Business category cannot exceed 100 characters")]
        public string? BusinessCategory { get; set; }

        [StringLength(500, ErrorMessage = "Social media cannot exceed 500 characters")]
        public string? SocialMedia { get; set; }

        [StringLength(100, ErrorMessage = "Preferred payment channel cannot exceed 100 characters")]
        public string? PreferredPaymentChannel { get; set; }

        [StringLength(100, ErrorMessage = "Delivery method cannot exceed 100 characters")]
        public string? DeliveryMethod { get; set; }

        public bool? ReturnPolicy { get; set; }

        /// <summary>
        /// Document URLs array
        /// </summary>
        public List<string>? Documents { get; set; }
    }

    /// <summary>
    /// DTO for merchant approval/rejection that matches frontend MerchantApprovalRequest interface
    /// </summary>
    public class MerchantApprovalDto
    {
        public Guid MerchantId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(approved|rejected)$", ErrorMessage = "Status must be 'approved' or 'rejected'")]
        public string Status { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string? Reason { get; set; }

        public string? ApprovedBy { get; set; }
    }

    /// <summary>
    /// DTO for suspending a merchant
    /// </summary>
    public class SuspendMerchantDto
    {
        public Guid MerchantId { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        public string Reason { get; set; } = string.Empty;

        public string? SuspendedBy { get; set; }
    }
}