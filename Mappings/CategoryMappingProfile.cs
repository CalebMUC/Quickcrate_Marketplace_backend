using AutoMapper;
using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.SubCategory;
using Minimart_Api.DTOS.SubSubCategory;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;

namespace Minimart_Api.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping between Category models and DTOs
    /// </summary>
    public class CategoryMappingProfile : Profile
    {
        public CategoryMappingProfile()
        {
            // Category to CategoryResponseDto
            CreateMap<Category, CategoryResponseDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));

            // SubCategory model to SubCategoryResponseDto
            CreateMap<Models.SubCategory, SubCategoryResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count(p => !p.IsDeleted)))
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products.Where(p => !p.IsDeleted)))
                .ForMember(dest => dest.SubSubCategories, opt => opt.MapFrom(src => src.SubSubCategories));

            // Product to ProductSummaryDto (for subcategory products)
            CreateMap<Product, ProductSummaryDto>(); // Remove the ImageUrl mapping since ProductSummaryDto doesn't have it

            // SubSubCategory to SubSubCategoryResponseDto
            // SUBCATEGORY → DTO
            CreateMap<SubCategory, SubCategoryResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count(p => !p.IsDeleted)))
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products.Where(p => !p.IsDeleted)))
                .ForMember(dest => dest.SubSubCategories, opt => opt.MapFrom(src => src.SubSubCategories));

            // Reverse mappings for potential future use
            CreateMap<CategoryResponseDto, Category>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));

            CreateMap<SubCategoryResponseDto, Models.SubCategory>()
                .ForMember(dest => dest.SubSubCategories, opt => opt.MapFrom(src => src.SubSubCategories));

            CreateMap<SubSubCategoryResponseDto, Models.SubSubCategory>();
        }
    }
}