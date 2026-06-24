using Microsoft.AspNetCore.Mvc;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Mpesa;
using Minimart_Api.DTOS.Payments;

namespace Minimart_Api.Services.Mpesa
{
    public interface IMpesaService
    {
        public Task<ConfirmationResponse> Confirmation(ConfimationRequest request);
        public Task<ValidationResponse> Validation(ValidationRequest request);
        public Task<RegisterUrlResponse> RegisterUrl();
        public Task<StkPushResponse> StkPush(StkPushRequest request);
        public Task<bool> ProcessSuccessfulPayment(PaymentData paymentData,string checkOutRequestData,string merchantRequestId);
        public Task<MpesaTrxQueryRes> TrxQueryStatus(MpesaTrxQuery query);
    }
}
