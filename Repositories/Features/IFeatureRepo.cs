using Minimart_Api.DTOS.Features;
using Minimart_Api.DTOS.General;

namespace Minimart_Api.Repositories.Features
{
    public interface IFeatureRepo
    {
        Task<Status> AddFeatures(FeatureDTO addFeatures);

        Task<IEnumerable<AddFeaturesDTO>> GetAllFeatures();

        Task<List<FeatureDTO>> GetFeatures(FeatureRequestDTO feature);

    }
}
