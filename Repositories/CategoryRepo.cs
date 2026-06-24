using Microsoft.EntityFrameworkCore;
using Minimart_Api.TempModels;
using Newtonsoft.Json;
using System.Net;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Runtime.CompilerServices;
using Microsoft.Data.SqlClient;
using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.Features;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.Products;

namespace Minimart_Api.Repositories
{
    public class CategoryRepo : ICategoryRepo
    {
        private readonly MinimartDBContext _dbContext;
        public CategoryRepo(MinimartDBContext dBContext ) { 
            _dbContext = dBContext;
        }
        //public async Task<ResponseStatus> AddFeatures(int SubCategoryID, List<FeatureDTO> features)
        public async Task<ResponseStatus> AddFeatures(AddFeaturesDTO addFeatures)
        {
            // Validate input
            if (addFeatures == null || addFeatures.Features == null || !addFeatures.Features.Any())
            {
                return new ResponseStatus
                {
                    ResponseStatusId = 400,
                    ResponseMessage = "Invalid input data"
                };
            }

            foreach (var feature in addFeatures.Features)
            {
                // Check if the feature already exists in the Features table
                var existingFeature = await _dbContext.Features
                    .FirstOrDefaultAsync(f => f.SubCategoryID == addFeatures.SubCategoryID
                                            && f.CategoryID == addFeatures.CategoryID
                                            && f.FeatureName == feature.FeatureName);

                if (existingFeature == null)
                {
                    // Feature does not exist; add it to the Features table
                    var newFeature = new Features
                    {
                        FeatureName = feature.FeatureName,
                        FeatureOptions = JsonConvert.SerializeObject(feature.FeatureOptions),
                        SubCategoryID = addFeatures.SubCategoryID, // Include SubCategoryID
                        CategoryID = addFeatures.CategoryID       // Include CategoryID
                    };

                    await _dbContext.Features.AddAsync(newFeature);
                    await _dbContext.SaveChangesAsync(); // Save to get the FeatureID assigned

                    existingFeature = newFeature; // Update the reference to the newly added feature
                }
                else
                {
                    // Update existing feature if it already exists
                    existingFeature.FeatureOptions = JsonConvert.SerializeObject(feature.FeatureOptions);
                    await _dbContext.SaveChangesAsync();
                }

                // Ensure FeatureID is available
                if (existingFeature?.FeatureID != null)
                {
                    // Check if the link between SubCategory and Feature already exists
                    var subcategoryFeatureExists = await _dbContext.SubCategoryFeatures
                        .AnyAsync(sf => sf.SubCategoryId == addFeatures.SubCategoryID
                                     && sf.FeatureID == existingFeature.FeatureID);

                    if (!subcategoryFeatureExists)
                    {
                        // Add a new link between SubCategory and Feature
                        var subcategoryFeature = new SubCategoryFeatures
                        {
                            SubCategoryId = addFeatures.SubCategoryID,
                            FeatureID = existingFeature.FeatureID
                        };

                        await _dbContext.SubCategoryFeatures.AddAsync(subcategoryFeature);
                    }
                }
                else
                {
                    // Log or handle the error if FeatureID is unexpectedly null
                    Console.WriteLine($"FeatureID for feature '{feature.FeatureName}' could not be determined.");
                }
            }

            // Save all changes to the SubCategoryFeatures at once
            await _dbContext.SaveChangesAsync();

            return new ResponseStatus
            {
                ResponseStatusId = 200,
                ResponseMessage = "Features Added Successfully"
            };
        }


        //public async Task<List<CartResults>> GetFilteredProducts(FilteredProductsDTO filteredProducts)
        //{
        //    try
        //    {
        //        // Base query for category and subcategory filtering
        //        var query = _dbContext.TProducts.Where(p =>
        //            p.CategoryId == Convert.ToInt16(filteredProducts.CategoryID) &&
        //            p.SubCategoryId == filteredProducts.SubCategoryID);

        //        // Dynamically apply feature filters
        //        foreach (var feature in filteredProducts.features)
        //        {
        //            // Escape JSON path for the feature key
        //            string jsonPath = $"$.\"{feature.Key}\"";

        //            // Create conditions for each value under the feature key
        //            string condition = string.Join(" OR ", feature.Value.Select(value =>
        //                $"JSON_VALUE(KeyFeatures, '{jsonPath}') = \"{value}\""));

        //            // Apply the dynamic condition to the query
        //            query = query.Where($"({condition})");
        //        }

        //        // Execute the query and project the results
        //        return await query.Select(p => new CartResults
        //        {
        //            productID = p.ProductId,
        //            ProductName = p.ProductName,
        //            price = p.Price,
        //            KeyFeatures = p.KeyFeatures
        //        }).ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and re-throw the exception for debugging
        //        Console.Error.WriteLine($"Error in GetFilteredProducts: {ex.Message}");
        //        throw;
        //    }
        //}






public async Task<List<CartResults>> GetFilteredProducts(FilteredProductsDTO filteredProducts)
    {
        try
        {
            // Start building the query
            var query = new StringBuilder("SELECT * FROM T_Products WHERE CategoryID = @CategoryID AND SubCategoryID = @SubCategoryID");

            // Base parameters
            var parameters = new List<object>
        {
            new SqlParameter("@CategoryID", filteredProducts.CategoryID),
            new SqlParameter("@SubCategoryID", filteredProducts.SubCategoryID)
        };

            // Add JSON filters for features
            int paramIndex = 0;
            foreach (var feature in filteredProducts.features)
            {
                string jsonPath = $"$.\"{feature.Key}\"";

                // Add conditions for each value in the feature
                foreach (var value in feature.Value)
                {
                    var parameterName = $"@Param{paramIndex++}";
                    query.Append($" AND JSON_VALUE(KeyFeatures, '{jsonPath}') = {parameterName}");
                    parameters.Add(new SqlParameter(parameterName, value));
                }
            }

            // Convert query to string
            var finalQuery = query.ToString();

            // Execute the query
            return await _dbContext.TProducts
                .FromSqlRaw(finalQuery, parameters.ToArray())
                .Select(p => new CartResults
                {
                    productID = p.ProductId,
                    ProductName = p.ProductName,
                    price = p.Price,
                    ProductImage = p.ImageUrl,
                    KeyFeatures = p.KeyFeatures
                })
                .ToListAsync();
        }
        catch (Exception ex)
        {
            // Log the error
            Console.Error.WriteLine($"Error in GetFilteredProducts: {ex.Message}");
            throw;
        }
    }







    public async Task<ResponseStatus> AddCategories(AddCategoryDTO categories)
        {
            try
            {
                // Check if it's a new category
                if (categories.CategoryID == 0)
                {
                    // Create and add the new category
                    var newCategory = new TCategory
                    {
                        CategoryName = categories.CategoryName,
                        Description = categories.Description
                    };

                    await _dbContext.TCategories.AddAsync(newCategory);
                    await _dbContext.SaveChangesAsync(); // Save to generate CategoryID

                    // Retrieve the generated CategoryID
                    categories.CategoryID = newCategory.CategoryId;
                }
                else
                {
                    // Verify if the existing CategoryID is valid
                    var existingCategory = await _dbContext.TCategories
                        .FirstOrDefaultAsync(c => c.CategoryId == categories.CategoryID);

                    if (existingCategory == null)
                    {
                        return new ResponseStatus
                        {
                            ResponseStatusId = 404,
                            ResponseMessage = "Category not found."
                        };
                    }
                }

                // Add subcategories
                var subCategories = categories.SubCategoryName.Select(subCategoryName => new TSubcategoryid
                {
                    ProductName = subCategoryName,
                    CategoryName = categories.CategoryName,
                    CategoryId = categories.CategoryID,
                    SubCategory = subCategoryName
                }).ToList();

                await _dbContext.TSubcategoryids.AddRangeAsync(subCategories);
                await _dbContext.SaveChangesAsync();

                return new ResponseStatus
                {
                    ResponseStatusId = 200,
                    ResponseMessage = "Category and subcategories added successfully."
                };
            }
            catch (Exception ex)
            {
                // Log the exception (if applicable)
                return new ResponseStatus
                {
                    ResponseStatusId = 500,
                    ResponseMessage = $"Internal Server Error: {ex.Message}"
                };
            }
        }





        public async Task<IEnumerable<CartResults>> GetSearchProducts(string subCategoryId)
        {
            return await _dbContext.TProducts
                .Where(tp => tp.SubCategoryId == subCategoryId)
                .Select(tp => new CartResults
                {
                    productID = tp.ProductId,
                    ProductName = tp.ProductName,
                    ProductImage = tp.ImageUrl,
                    Instock = tp.InStock,
                    price = tp.Price,
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<Features>> GetAllFeatures()
        {
          
            return await _dbContext.Features.ToListAsync();
        }

        //Get Features Linked to a SubCategory
        public async Task<List<FeatureDTO>> GetFeatures(FeatureRequestDTO feature) {

            //var features = await _dbContext.SubCategoryFeatures
            //                    .Where(f => f.SubCategoryId == feature.CategoryID)
            //                    .Select(f => new FeatureDTO
            //                    {
            //                        FeatureName = f.features.FeatureName,
            //                        FeatureOptions = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(f.features.FeatureOptions)
            //                    }).ToListAsync();

            var features = await _dbContext.Features
                                .Where(f => f.CategoryID == feature.CategoryID
                                && f.SubCategoryID == feature.SubCategoryID)
                                .Select(f => new FeatureDTO
                                {
                                    FeatureName = f.FeatureName,
                                    FeatureOptions = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(f.FeatureOptions),
                                    CategoryId = f.CategoryID,
                                    SubCategoryId = f.SubCategoryID,

                                }).ToListAsync();

            return features;   

        }

    }
}
