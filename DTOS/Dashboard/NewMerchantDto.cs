namespace Minimart_Api.DTOS.Dashboard
{
    /// <summary>
    /// New merchant data for admin dashboard
    /// </summary>
    public class NewMerchantDto
    {
        /// <summary>
        /// Merchant identifier
        /// </summary>
        public Guid MerchantId { get; set; }

        /// <summary>
        /// Business name
        /// </summary>
        public string BusinessName { get; set; } = string.Empty;

        /// <summary>
        /// Merchant name
        /// </summary>
        public string MerchantName { get; set; } = string.Empty;

        /// <summary>
        /// Business category
        /// </summary>
        public string BusinessCategory { get; set; } = string.Empty;

        /// <summary>
        /// Business type
        /// </summary>
        public string BusinessType { get; set; } = string.Empty;

        /// <summary>
        /// Date when merchant registered
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Merchant status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Contact email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Business address
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Number of days since registration
        /// </summary>
        public int DaysSinceRegistration { get; set; }

        /// <summary>
        /// Whether merchant has completed profile setup
        /// </summary>
        public bool ProfileComplete { get; set; }

        /// <summary>
        /// Number of products added
        /// </summary>
        public int ProductsAdded { get; set; }

        /// <summary>
        /// Whether merchant has made any sales
        /// </summary>
        public bool HasMadeSales { get; set; }
    }
}