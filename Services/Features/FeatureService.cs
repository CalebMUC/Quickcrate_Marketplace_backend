using Minimart_Api.DTOS.Features;
using Minimart_Api.DTOS.General;
using Minimart_Api.Repositories.Features;

namespace Minimart_Api.Services.Features
{
    public class FeatureService : IFeatureService
    {
        private readonly IFeatureRepo _featureRepo;
        public FeatureService(IFeatureRepo featureRepo) {
            _featureRepo = featureRepo;
        }
        public async Task<Status> AddFeatures(FeatureDTO addFeatures)
        {
            return await _featureRepo.AddFeatures(addFeatures);
        }
        public async Task<List<FeatureDTO>> GetFeatures(FeatureRequestDTO feature)
        {

            return await _featureRepo.GetFeatures(feature);
        }

        public async Task<IEnumerable<AddFeaturesDTO>> GetAllFeatures()
        {
            return await _featureRepo.GetAllFeatures();
        }
    }
}
