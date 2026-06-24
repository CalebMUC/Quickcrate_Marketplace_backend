using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Mpesa;
using Minimart_Api.DTOS.Payments;
using Minimart_Api.Repositories.Mpesa;

namespace Minimart_Api.Services.Mpesa
{
    public class MpesaService : IMpesaService
    {
        private readonly IMpesaRepo _mpesaRepo;
        public MpesaService(IMpesaRepo mpesaRepo ) {
            _mpesaRepo = mpesaRepo;
        }
        public async Task<ConfirmationResponse> Confirmation(ConfimationRequest request)
        {
            return await _mpesaRepo.Confirmation(request);
        }
        public async Task<ValidationResponse> Validation(ValidationRequest request)
        {
            return await _mpesaRepo.Validation(request);
        }
        public async Task<RegisterUrlResponse> RegisterUrl() {
            return await _mpesaRepo.Register();
        }

        public async Task<StkPushResponse> StkPush(StkPushRequest request) {
            return await _mpesaRepo.StkPush(request);
        }

        public async Task<MpesaTrxQueryRes> TrxQueryStatus(MpesaTrxQuery query) {
            return await _mpesaRepo.TrxQueryStatus(query);
        }

        public async Task<bool> ProcessSuccessfulPayment(PaymentData paymentData, string checkOutRequestData, string merchantRequestId)
        {
            return await _mpesaRepo.ProcessSuccessfulPayment(paymentData, checkOutRequestData, merchantRequestId);
        }
    }
}
