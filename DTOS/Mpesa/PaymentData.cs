namespace Minimart_Api.DTOS.Mpesa
{
    public class PaymentData
    {
        public string Amount { get; set; } = string.Empty;
        public string MpesaReceiptNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string TransactionDate { get; set; } = string.Empty;
        public string AccountReference { get; set; } = string.Empty;
    }
}
