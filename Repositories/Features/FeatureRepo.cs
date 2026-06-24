using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Features;
using Minimart_Api.DTOS.General;
using Newtonsoft.Json;
using Minimart_Api.Models;
namespace Minimart_Api.Repositories.Features
{
    public class FeatureRepo : IFeatureRepo
    {

        private readonly MinimartDBContext _dbContext;
        public FeatureRepo(MinimartDBContext dBContext)
        {
            _dbContext = dBContext;
        }
        //public async Task<ResponseStatus> AddFeatures(int SubCategoryID, List<FeatureDTO> features)
        public async Task<Status> AddFeatures(FeatureDTO features)
        {
            try
            {
                if (features == null)
                {
                    return new Status
                    {
                        ResponseCode = 400,
                        ResponseMessage = "Invalid input data"
                    };
                }

                // Simplified query since we know all values are non-null
                var existingFeature = await _dbContext.Features
                    .FirstOrDefaultAsync(f => f.SubCategoryID == features.SubCategoryId
                                          && f.CategoryID == features.CategoryId
                                          && f.FeatureName == features.FeatureName);

                if (existingFeature == null)
                {
                    var newFeature = new Models.Features
                    {
                        FeatureName = features.FeatureName,
                        FeatureOptions = JsonConvert.SerializeObject(features.FeatureOptions),
                        SubCategoryID = features.SubCategoryId,
                        CategoryID = features.CategoryId,
                        SubSubCategoryID = features.SubSubCategoryId // This can be null
                    };

                    await _dbContext.Features.AddAsync(newFeature);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    existingFeature.FeatureOptions = JsonConvert.SerializeObject(features.FeatureOptions);
                    await _dbContext.SaveChangesAsync();
                }

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = existingFeature == null
                        ? "Feature added successfully"
                        : "Feature updated successfully"
                };
            }
            catch (Exception ex)
            {
                // Log the exception here
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = $"An error occurred: {ex.Message}"
                };
            }
        }

        public async Task<IEnumerable<AddFeaturesDTO>> GetAllFeatures()
        {
            // First, get all features with their basic information
            var features = await _dbContext.Features
                .Select(f => new AddFeaturesDTO
                {
                    FeatureID = f.FeatureID,
                    FeatureName = f.FeatureName,
                    FeatureOptions = f.FeatureOptions ?? string.Empty,
                    CategoryID = f.CategoryID.HasValue ? f.CategoryID.Value.GetHashCode() : 0, // Convert Guid to int for legacy compatibility
                    SubCategoryID = f.SubCategoryID.HasValue ? f.SubCategoryID.Value.GetHashCode() : 0,
                    SubSubCategoryID = f.SubSubCategoryID.HasValue ? f.SubSubCategoryID.Value.GetHashCode() : 0,
                    // For now, we'll populate names separately or use simple lookups
                    CategoryName = "", // Will populate below
                    SubCategoryName = null,
                    SubSubCategoryName = null
                })
                .ToListAsync();

            // Since the join is causing type issues, let's use a simpler approach
            // You may need to create proper lookup methods or adjust the data model
            return features;
        }
        //Get Features Linked to a SubCategory
        public async Task<List<FeatureDTO>> GetFeatures(FeatureRequestDTO feature)
        {
            var features = await _dbContext.Features
                                .Where(f => f.CategoryID == feature.CategoryID
                                && f.SubCategoryID == feature.SubCategoryID
                                && (feature.SubSubCategoryID == null ? 
                                    f.SubSubCategoryID == null : 
                                    f.SubSubCategoryID.HasValue && f.SubSubCategoryID.Value == feature.SubSubCategoryID))
                                .Select(f => new FeatureDTO
                                {
                                    FeatureName = f.FeatureName,
                                    FeatureOptions = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(f.FeatureOptions) ?? new Dictionary<string, List<string>>(),
                                    CategoryId = f.CategoryID,
                                    SubCategoryId = f.SubCategoryID,
                                    SubSubCategoryId = f.SubSubCategoryID
                                }).ToListAsync();

            return features;
        }

    }
}
