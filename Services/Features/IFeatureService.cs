using Minimart_Api.DTOS.Features;
using Minimart_Api.DTOS.General;
using Minimart_Api.Models;

namespace Minimart_Api.Services.Features
{
    public interface IFeatureService
    {
        Task<Status> AddFeatures(FeatureDTO addFeatures);

        Task<List<FeatureDTO>> GetFeatures(FeatureRequestDTO feature);

        Task<IEnumerable<AddFeaturesDTO>> GetAllFeatures();
    }
}
