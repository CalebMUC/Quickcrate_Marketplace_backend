namespace Minimart_Api.DTOS.Payments
{
    public class PaymentDetailsDto
    {

        public Guid PaymentID { get; set; }
        public int PaymentMethodID { get; set; }
        public long PaymentReference { get; set; }

        public string TrxReference { get; set; }

        public string PaymentMethod { get; set; }

        public string Phonenumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }


    }
}
