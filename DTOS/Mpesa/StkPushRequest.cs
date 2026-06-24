namespace Minimart_Api.DTOS.Mpesa
{
    public class StkPushRequest    
    {
        public string BusinessShortCode { get; set; }
        public string Amount { get; set; }
        public string PartyA { get; set; }
        public string PartyB { get; set; }
        public string PhoneNumber { get; set; }
        public string CallBackURL { get; set; } 
        public string AccountReference { get; set; }
        public string TransactionDesc { get; set; }
    }
}
