using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Mpesa;
using Minimart_Api.DTOS.Payments;

namespace Minimart_Api.Repositories.Mpesa
{
    public interface IMpesaRepo
    {
        public Task<ConfirmationResponse> Confirmation(ConfimationRequest request);
        public Task<ValidationResponse> Validation(ValidationRequest request);
        public Task<RegisterUrlResponse> Register();
        public Task<StkPushResponse> StkPush(StkPushRequest request);
        public Task<MpesaTrxQueryRes> TrxQueryStatus(MpesaTrxQuery query);
        public Task<bool> ProcessSuccessfulPayment(PaymentData paymentData,
            string checkOutRequestData, string merchantRequestId);
    }
}
