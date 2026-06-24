namespace Minimart_Api.DTOS.Merchants
{
    public class ApproveMerchantDto
    {
        public Guid MerchantID { get; set; }
        public string BusinessName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string? UserId { get; set; }
        public bool AccountCreated { get; set; }
        public bool EmailSent { get; set; }
    }
}
