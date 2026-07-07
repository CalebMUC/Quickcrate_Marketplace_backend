using AutoMapper;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;

namespace Minimart_Api.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping between Product models and DTOs
    /// </summary>
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // Product -> ProductResponseDto
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.Merchant, opt => opt.MapFrom(src => src.Merchant))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category))
                .ForMember(dest => dest.SubCategory, opt => opt.MapFrom(src => src.SubCategory))
                .ForMember(dest => dest.SubSubCategory, opt => opt.MapFrom(src => src.SubSubCategory))
                // SEO Properties
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug))
                .ForMember(dest => dest.SlugUpdatedAt, opt => opt.MapFrom(src => src.SlugUpdatedAt))
                .ForMember(dest => dest.MetaTitle, opt => opt.MapFrom(src => src.MetaTitle))
                .ForMember(dest => dest.MetaDescription, opt => opt.MapFrom(src => src.MetaDescription))
                .ForMember(dest => dest.MetaKeywords, opt => opt.MapFrom(src => src.MetaKeywords))
                // Category Information
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.SubCategoryId, opt => opt.MapFrom(src => src.SubCategoryId))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategoryName))
                .ForMember(dest => dest.SubSubCategoryId, opt => opt.MapFrom(src => src.SubSubCategoryId))
                .ForMember(dest => dest.SubSubCategoryName, opt => opt.MapFrom(src => src.SubSubCategoryName));

            // Product -> ProductListDto
            CreateMap<Product, ProductListDto>()
                // SEO Properties
                .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug))
                .ForMember(dest => dest.MetaTitle, opt => opt.MapFrom(src => src.MetaTitle))
                .ForMember(dest => dest.MetaDescription, opt => opt.MapFrom(src => src.MetaDescription))
                // Category Information
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.SubCategoryId, opt => opt.MapFrom(src => src.SubCategoryId))
                .ForMember(dest => dest.SubCategoryName, opt => opt.MapFrom(src => src.SubCategoryName))
                //.ForMember(dest => dest.SubSubCategoryId, opt => opt.MapFrom(src => src.SubSubCategoryId))
                .ForMember(dest => dest.SubSubCategoryName, opt => opt.MapFrom(src => src.SubSubCategoryName));

            // Product -> ProductSummaryDto
            CreateMap<Product, ProductSummaryDto>();

            // CreateProductDto -> Product
            CreateMap<CreateProductDto, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) // Auto-generated
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.MerchantID, opt => opt.MapFrom(src => src.MerchantID))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategory, opt => opt.Ignore())
                .ForMember(dest => dest.SubSubCategory, opt => opt.Ignore())
                // SEO fields will be set manually in repository
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.SlugUpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.MetaTitle, opt => opt.Ignore())
                .ForMember(dest => dest.MetaDescription, opt => opt.Ignore())
                .ForMember(dest => dest.MetaKeywords, opt => opt.Ignore());

            // UpdateProductDto -> Product
            CreateMap<UpdateProductDto, Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore()) // Don't update ID
                .ForMember(dest => dest.CreatedOn, opt => opt.Ignore()) // Don't update created date
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Don't update creator
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Merchant, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.SubCategory, opt => opt.Ignore())
                .ForMember(dest => dest.SubSubCategory, opt => opt.Ignore())
                .ForMember(dest => dest.MerchantID, opt => opt.Ignore()) // Don't allow changing MerchantID
                // SEO slug regeneration handled in repository
                .ForMember(dest => dest.Slug, opt => opt.Ignore())
                .ForMember(dest => dest.SlugUpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.MetaTitle, opt => opt.Ignore())
                .ForMember(dest => dest.MetaDescription, opt => opt.Ignore())
                .ForMember(dest => dest.MetaKeywords, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Merchant -> ProductMerchantDto
            CreateMap<Merchants, ProductMerchantDto>()
                .ForMember(dest => dest.MerchantId, opt => opt.MapFrom(src => src.MerchantID))
                .ForMember(dest => dest.MerchantName, opt => opt.MapFrom(src => src.MerchantName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));

            // Category -> ProductCategoryDto
            CreateMap<Category, ProductCategoryDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId));

            // SubCategory -> ProductSubCategoryDto
            CreateMap<SubCategory, ProductSubCategoryDto>()
                .ForMember(dest => dest.SubCategoryId, opt => opt.MapFrom(src => src.SubCategoryId));

            // SubSubCategory -> ProductSubSubCategoryDto
            CreateMap<SubSubCategory, ProductSubSubCategoryDto>()
                .ForMember(dest => dest.SubSubCategoryId, opt => opt.MapFrom(src => src.SubSubCategoryId));
        }
    }
}