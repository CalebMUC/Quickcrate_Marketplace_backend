namespace Minimart_Api.DTOS.Merchants
{
    public class EditMerchantDto
    {
        public Guid MerchantID { get; set; }
        public string? BusinessName { get; set; }

        public string? BusinessType { get; set; }

        public string? BusinessRegistrationNo { get; set; }

        public string? KRAPIN { get; set; }

        public string? BusinessNature { get; set; }

        public string? BusinessCategory { get; set; }

        public string? MerchantName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? SocialMedia { get; set; }

        public string? BankName { get; set; }

        public string? BankAccountNo { get; set; }

        public string? BankAccountName { get; set; }

        public string? MpesaPaybill { get; set; }

        public string? MpesaTillNumber { get; set; }

        public string? PreferredPaymentChannel { get; set; }

        public string? KRAPINCertificate { get; set; }

        public string? BusinessRegistrationCertificate { get; set; }

        public bool? TermsAndCondition { get; set; }

        public string? DeliveryMethod { get; set; }

        public bool? ReturnPolicy { get; set; }

        public string? Status { get; set; } = "Active";
    }
}
