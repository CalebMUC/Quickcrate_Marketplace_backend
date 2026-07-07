using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.Models;
using Minimart_Api.Services.SlugService;
using System;
using System.Diagnostics;
using GeneralPagedResultDto = Minimart_Api.DTOS.General.PagedResultDto<Minimart_Api.DTOS.Products.ProductListDto>;

namespace Minimart_Api.Repositories.ProductRepository
{
    public class ProductRepository : IProductRepository
    {
        private readonly MinimartDBContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductRepository> _logger;
        private readonly ISlugService _slugService;
        private readonly IMemoryCache _cache;
        private static readonly int[] DiscountBuckets = [10, 20, 30, 50];

        public ProductRepository(MinimartDBContext context, IMapper mapper, ILogger<ProductRepository> logger,
            ISlugService slugService,
            IMemoryCache cache)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _slugService = slugService;
            _cache = cache;
        }

        private sealed record SubCategoryFacetRaw(Guid? SubCategoryId, string? SubCategoryName, int Count);



        #region Basic CRUD Operations

        public async Task<ProductResponseDto?> GetByIdAsync(Guid productId)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Merchant)
                    .Include(p => p.Category)
                    .Include(p => p.SubCategory)
                    .Include(p => p.SubSubCategory)
                    .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);

                return product == null ? null : _mapper.Map<ProductResponseDto>(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID: {ProductId}", productId);
                throw;
            }
        }

        public async Task<Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>> GetAllAsync(ProductFilterDto filter)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.SubCategory)
                    .Include(p => p.SubSubCategory)
                    .Where(p => !p.IsDeleted);
                            //&& p.IsActive);

                query = ApplyFilters(query, filter);

                return await GetPagedResultAsync(query, filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                throw;
            }
        }

        public async Task<Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>> GetProductsByMerchantIdAsync(Guid merchantId, ProductFilterDto filter)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.SubCategory)
                    .Include(p => p.SubSubCategory)
                    .Where(p => p.MerchantID == merchantId && !p.IsDeleted);

                query = ApplyFilters(query, filter);

                return await GetPagedResultAsync(query, filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products for merchant: {MerchantId}", merchantId);
                throw;
            }
        }

        //public async Task<Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>> GetProductsByCategoryAsync(Guid categoryId, ProductFilterDto filter)
        //{
        //    try
        //    {
        //        // Start with the base query
        //        var baseQuery = _context.Products
        //            .Where(p => p.CategoryId == categoryId && !p.IsDeleted);

        //        // Count before applying any additional filters
        //        var countBeforeFilters = await baseQuery.CountAsync();
        //        _logger.LogInformation("Products found for CategoryId {CategoryId} before filters: {Count}", categoryId, countBeforeFilters);

        //        // Apply filters
        //        var query = ApplyFilters(baseQuery, filter);

        //        // Get final count after filters
        //        var totalCount = await query.CountAsync();

        //        var products = await query
        //            .Skip((filter.Page - 1) * filter.PageSize)
        //            .Take(filter.PageSize)
        //            .ToListAsync();

        //        // Manually load category information if needed
        //        var categoryInfo = await _context.Categories
        //            .Where(c => c.CategoryId == categoryId)
        //            .Select(c => new { c.CategoryId, c.Name })
        //            .FirstOrDefaultAsync();

        //        // Map to DTOs using the retrieved product list instead of query
        //        var productDtos = products.Select(p => new ProductListDto
        //        {
        //            ProductId = p.ProductId,
        //            CategoryId = p.CategoryId,
        //            SubCategoryId = p.SubCategoryId ?? Guid.Empty,
        //            CategoryName = categoryInfo?.Name ?? "Unknown Category",
        //            SubCategoryName = p.SubCategoryName,
        //            SubSubCategoryName = p.SubSubCategoryName,
        //            ProductName = p.ProductName,
        //            Description = p.Description,
        //            Slug = p.Slug,
        //            MetaTitle = p.MetaTitle,
        //            MetaDescription = p.MetaDescription,
        //            Price = p.Price,
        //            Discount = p.Discount,
        //            StockQuantity = p.StockQuantity,
        //            SKU = p.SKU,
        //            ProductDescription = p.ProductDescription,
        //            ProductSpecification = p.ProductSpecification,
        //            BoxContents = p.BoxContents,
        //            Features = p.Features,
        //            ImageUrls = p.ImageUrls,
        //            IsActive = p.IsActive,
        //            IsFeatured =  p.IsFeatured,
        //            Status = p.Status ?? "Unknown",
        //            MerchantID = p.MerchantID,
        //            CreatedOn = p.CreatedOn,
        //            UpdatedOn = p.UpdatedOn
        //        }).ToList();

        //        return new Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>
        //        {
        //            Data = productDtos,
        //            TotalCount = totalCount,
        //            PageNumber = filter.Page,
        //            PageSize = filter.PageSize
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving products for category: {CategoryId}", categoryId);

        //        // Log the actual SQL query being generated for debugging
        //        _logger.LogError("Query parameters: CategoryId={CategoryId}, IsDeleted=false", categoryId);
        //        throw;
        //    }
        //}

            public async Task<PagedResultDto<ProductListDto>> GetProductsByCategoryAsync(
        Guid categoryId,
        ProductFilterDto filter)
            {
                var query = _context.Products
                    .Where(p => p.CategoryId == categoryId
                             && !p.IsDeleted
                             && p.IsActive);

                // ── Filters ──────────────────────────────────────────────────────────
                if (filter.Status is not null)
                    query = query.Where(p => p.Status == filter.Status);

                if (filter.IsFeatured.HasValue)
                    query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

                //if (!string.IsNullOrWhiteSpace(filter.Brand))
                //    query = query.Where(p => p.Brand != null &&
                //                             p.Brand.ToLower() == filter.Brand.ToLower());

                if (filter.MinPrice.HasValue)
                    query = query.Where(p => p.Price >= filter.MinPrice.Value);

                if (filter.MaxPrice.HasValue)
                    query = query.Where(p => p.Price <= filter.MaxPrice.Value);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    query = query.Where(p => p.ProductName.Contains(filter.SearchTerm)
                                          || (p.Description != null && p.Description.Contains(filter.SearchTerm)));

                // ── Sorting ───────────────────────────────────────────────────────────
                var descending = string.Equals(filter.SortDirection, "DESC",
                                     StringComparison.OrdinalIgnoreCase);

                query = filter.SortBy?.ToLower() switch
                {
                    "price" => descending ? query.OrderByDescending(p => p.Price)
                                                : query.OrderBy(p => p.Price),
                    "discount" => descending ? query.OrderByDescending(p => p.Discount)
                                                : query.OrderBy(p => p.Discount),
                    "productname" => descending ? query.OrderByDescending(p => p.ProductName)
                                                : query.OrderBy(p => p.ProductName),
                    _ => descending ? query.OrderByDescending(p => p.CreatedOn)   // default: newest
                                                : query.OrderBy(p => p.CreatedOn),
                };

                // ── Count + page ──────────────────────────────────────────────────────
                var totalCount = await query.CountAsync();

                var products = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(p => new ProductListDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Slug = p.Slug,
                        Price = p.Price,
                        Discount = p.Discount,
                        StockQuantity = p.StockQuantity,
                        ImageUrls = p.ImageUrls,
                        IsActive = p.IsActive,
                        IsFeatured = p.IsFeatured,
                        SKU = p.SKU,
                        ProductDescription = p.ProductDescription,
                        ProductSpecification = p.ProductSpecification,
                        BoxContents = p.BoxContents,
                        Features = p.Features,
                        Status = p.Status ?? "unknown",
                        Brand = p.Brand,
                        CategoryId = p.CategoryId,
                        CategoryName = p.CategoryName,
                        SubCategoryId = p.SubCategoryId ?? Guid.Empty,
                        SubCategoryName = p.SubCategoryName,
                        SubSubCategoryName = p.SubSubCategoryName,
                        MerchantID = p.MerchantID,
                        MetaTitle = p.MetaTitle,
                        MetaDescription = p.MetaDescription,
                        CreatedOn = p.CreatedOn,
                        UpdatedOn = p.UpdatedOn,
                    })
                    .ToListAsync();

                return new PagedResultDto<ProductListDto>
                {
                    Data = products,
                    TotalCount = totalCount,
                    PageNumber = filter.Page,
                    PageSize = filter.PageSize,
                };
            }

        // Application/Services/ProductService.cs
      

            public async Task<ProductPagedResultDto<ProductListDto>> GetFilteredProductsByCategoryAsync(
                Guid categoryId,
                ProductFilterDto filter,
                CancellationToken ct = default)
            {
                var sw = Stopwatch.StartNew();

                var baseQuery = BuildBaseQuery(categoryId, filter);

            // Run facets and page fetch concurrently
            //var (facets, page) = await (
            //    ComputeFacetsAsync(baseQuery, filter, ct),
            //    FetchPageAsync(baseQuery, filter, ct)
            //);

            var facets = await ComputeFacetsAsync(baseQuery, filter, ct);
            var page = await FetchPageAsync(baseQuery, filter, ct);

            //await Task.WhenAll(facetsTask, pageTask);

            //var facets = facetsTask.Result;
            //var page = pageTask.Result;



            sw.Stop();

                return new ProductPagedResultDto<ProductListDto>
                {
                    Products = page.Items,
                    Pagination = new PaginationDto
                    {
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        TotalItems = page.TotalCount,
                        TotalPages = (int)Math.Ceiling(page.TotalCount / (double)filter.PageSize),
                        HasPrevious = filter.Page > 1,
                        HasNext = filter.Page * filter.PageSize < page.TotalCount,
                    },
                    Facets = facets,
                    AppliedFilters = MapAppliedFilters(filter),
                    Meta = new CategoryMetaDto
                    {
                        CategoryId = categoryId,
                        QueryDuration = sw.ElapsedMilliseconds,
                    },
                };
            }

            // ── Shared base query (no paging, no select) ──────────────────────────────
            private IQueryable<Product> BuildBaseQuery(Guid categoryId, ProductFilterDto filter)
            {
                var q = _context.Products
                    .Where(p => p.CategoryId == categoryId
                             && !p.IsDeleted
                             && p.IsActive);

                if (filter.Status is not null)
                    q = q.Where(p => p.Status == filter.Status);

                if (filter.SubCategoryId.HasValue)
                    q = q.Where(p => p.SubCategoryId == filter.SubCategoryId.Value);

                if (filter.Brands.Count > 0)
                {
                    // Case-insensitive multi-brand: EF translates to SQL IN (...)
                    var lower = filter.Brands.Select(b => b.ToLower()).ToList();
                    q = q.Where(p => lower.Contains(p.Brand!.ToLower()));
                }

                if (filter.MinPrice.HasValue && filter.MinPrice != 0)
                    q = q.Where(p => p.Price >= filter.MinPrice.Value);

                if (filter.MaxPrice.HasValue && filter.MaxPrice != 0)
                    q = q.Where(p => p.Price <= filter.MaxPrice.Value);

                if (filter.MinDiscount.HasValue && filter.MinDiscount != 0)
                    q = q.Where(p => p.Discount >= filter.MinDiscount.Value);

                if (filter.InStockOnly)
                    q = q.Where(p => p.StockQuantity > 0);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                    q = q.Where(p => p.ProductName.Contains(filter.SearchTerm)
                                   || (p.Description != null
                                       && p.Description.Contains(filter.SearchTerm)));

                return q;
            }

            // ── Sorting + paging ──────────────────────────────────────────────────────
            private static async Task<(IReadOnlyList<ProductListDto> Items, int TotalCount)>
                FetchPageAsync(
                    IQueryable<Product> query,
                    ProductFilterDto filter,
                    CancellationToken ct)
            {
                var sorted = filter.SortBy?.ToLower() switch
                {
                    "price" => filter.SortDescending
                                         ? query.OrderByDescending(p => p.Price)
                                         : query.OrderBy(p => p.Price),
                    "discount" => filter.SortDescending
                                         ? query.OrderByDescending(p => p.Discount)
                                         : query.OrderBy(p => p.Discount),
                    "productname" => filter.SortDescending
                                         ? query.OrderByDescending(p => p.ProductName)
                                         : query.OrderBy(p => p.ProductName),
                    //"rating" => filter.SortDescending
                    //                     ? query.OrderByDescending(p => p.Rating)
                    //                     : query.OrderBy(p => p.Rating),
                    _ => filter.SortDescending
                                         ? query.OrderByDescending(p => p.CreatedOn)
                                         : query.OrderBy(p => p.CreatedOn),
                };

                var totalCount = await sorted.CountAsync(ct);

                var items = await sorted
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(p => new ProductListDto {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Slug = p.Slug,
                        Price = p.Price,
                        Discount = p.Discount,
                        StockQuantity = p.StockQuantity,
                        ImageUrls = p.ImageUrls,
                        IsActive = p.IsActive,
                        IsFeatured = p.IsFeatured,
                        SKU = p.SKU,
                        ProductDescription = p.ProductDescription,
                       // ProductSpecification = p.ProductSpecification,
                        //BoxContents = p.BoxContents,
                        //Features = p.Features,
                        Status = p.Status ?? "unknown",
                        Brand = p.Brand,
                        CategoryId = p.CategoryId,
                        CategoryName = p.CategoryName,
                        SubCategoryId = p.SubCategoryId ?? Guid.Empty,
                        SubCategoryName = p.SubCategoryName,
                        SubSubCategoryName = p.SubSubCategoryName,
                        MerchantID = p.MerchantID,
                        MetaTitle = p.MetaTitle,
                        MetaDescription = p.MetaDescription,
                        CreatedOn = p.CreatedOn,
                        UpdatedOn = p.UpdatedOn,
                    })
                    .ToListAsync(ct);

                return (items, totalCount);
            }

        // ── Facets (runs against filtered base, BEFORE paging) ───────────────────
        private static async Task<CategoryFacetsDto> ComputeFacetsAsync(
 IQueryable<Product> baseQuery,
 ProductFilterDto filter,
 CancellationToken ct,
 bool isSubCategory = false)
        {
            var brandFacets = await baseQuery
                .Where(p => p.Brand != null)
                .GroupBy(p => p.Brand!)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(50)
                .ToListAsync(ct);

            // Sub-category facets — skip entirely on subcategory pages
            List<SubCategoryFacetRaw> subCatFacets = new();
            if (!isSubCategory)
            {
                subCatFacets = await baseQuery
                    .Where(p => p.SubCategoryId != null)
                    .GroupBy(p => p.SubCategoryId)
                    .Select(g => new SubCategoryFacetRaw(
                        g.Key,
                        g.OrderByDescending(x => x.SubCategoryName != null && x.SubCategoryName != "")
                         .Select(x => x.SubCategoryName)
                         .FirstOrDefault(),
                        g.Count()))
                    .ToListAsync(ct);
            }

            var priceRange = await baseQuery
                .GroupBy(_ => 1)
                .Select(g => new { Min = g.Min(p => p.Price), Max = g.Max(p => p.Price) })
                .FirstOrDefaultAsync(ct);

            var discounts = await baseQuery.Select(p => p.Discount).ToListAsync(ct);
            var discountCounts = DiscountBuckets
                .Select(bucket => new { Value = bucket, Count = discounts.Count(d => d >= bucket) })
                .ToList();

            var inStockCount = await baseQuery.CountAsync(p => p.StockQuantity > 0, ct);

            var selectedBrands = filter.Brands.Select(b => b.ToLower()).ToHashSet();

            return new CategoryFacetsDto
            {
                Brands = brandFacets
                    .Select(b => new BrandFacetDto
                    {
                        Name = b.Name,
                        Count = b.Count,
                        Selected = selectedBrands.Contains(b.Name.ToLower()),
                    })
                    .ToList(),

                SubCategories = subCatFacets
                    .Where(s => s.SubCategoryId.HasValue)
                    .Select(s => new SubCategoryFacetDto
                    {
                        Id = s.SubCategoryId!.Value,
                        Name = s.SubCategoryName ?? string.Empty,
                        Count = s.Count,
                        Selected = filter.SubCategoryId == s.SubCategoryId,
                    })
                    .ToList(),

                PriceRange = new PriceRangeFacetDto
                {
                    Min = priceRange?.Min ?? 0,
                    Max = priceRange?.Max ?? 0,
                    SelectedMin = filter.MinPrice,
                    SelectedMax = filter.MaxPrice,
                },

                Discounts = discountCounts
                    .Select(d => new DiscountFacetDto
                    {
                        Value = d.Value,
                        Count = d.Count,
                        Selected = filter.MinDiscount == d.Value,
                    })
                    .ToList(),

                InStock = new StockFacetDto
                {
                    TotalInStock = inStockCount,
                    Selected = filter.InStockOnly,
                },
            };
        }

        private static AppliedFiltersDto MapAppliedFilters(ProductFilterDto f) => new()
            {
                Brands = f.Brands,
                SubCategoryId = f.SubCategoryId,
                MinPrice = f.MinPrice,
                MaxPrice = f.MaxPrice,
                MinDiscount = f.MinDiscount,
                InStockOnly = f.InStockOnly,
                SortBy = f.SortBy,
                SortDirection = f.SortDirection,
            };


        public async Task<ProductPagedResultDto<ProductListDto>> GetFilteredProductsBySubCategoryAsync(
                Guid subCategoryId,
                ProductFilterDto filter,
                CancellationToken ct = default)
        { 
            var sw = Stopwatch.StartNew();
            var baseQuery = BuildBaseQueryForSubCategory(subCategoryId, filter);

            var facets = await ComputeFacetsAsync(baseQuery, filter, ct, isSubCategory: true);
            var page = await FetchPageAsync(baseQuery, filter, ct);

            sw.Stop();

            return new ProductPagedResultDto<ProductListDto>
            {
                Products = page.Items,
                Pagination = new PaginationDto
                {
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalItems = page.TotalCount,
                    TotalPages = (int)Math.Ceiling(page.TotalCount / (double)filter.PageSize),
                    HasPrevious = filter.Page > 1,
                    HasNext = filter.Page * filter.PageSize < page.TotalCount,
                },
                Facets = facets,
                AppliedFilters = MapAppliedFilters(filter),
                Meta = new CategoryMetaDto
                {
                    CategoryId = subCategoryId, // Note: This is the sub-category ID
                    QueryDuration = sw.ElapsedMilliseconds,
                },
            };

        }

        private IQueryable<Product> BuildBaseQueryForSubCategory(Guid subCategoryId, ProductFilterDto filter)
        {
            var q = _context.Products
                .Where(p => p.SubCategoryId == subCategoryId
                         && !p.IsDeleted
                         && p.IsActive);

            if (filter.Status is not null)
                q = q.Where(p => p.Status == filter.Status);

            //if (filter.SubCategoryId.HasValue)
            //    q = q.Where(p => p.SubCategoryId == filter.SubCategoryId.Value);

            if (filter.Brands.Count > 0)
            {
                // Case-insensitive multi-brand: EF translates to SQL IN (...)
                var lower = filter.Brands.Select(b => b.ToLower()).ToList();
                q = q.Where(p => lower.Contains(p.Brand!.ToLower()));
            }

            if (filter.MinPrice.HasValue && filter.MinPrice != 0)
                q = q.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue && filter.MaxPrice != 0)
                q = q.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.MinDiscount.HasValue && filter.MinDiscount != 0)
                q = q.Where(p => p.Discount >= filter.MinDiscount.Value);

            if (filter.InStockOnly)
                q = q.Where(p => p.StockQuantity > 0);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                q = q.Where(p => p.ProductName.Contains(filter.SearchTerm)
                               || (p.Description != null
                                   && p.Description.Contains(filter.SearchTerm)));

            return q;
        }





        public async Task<PagedResultDto<ProductListDto>> GetSubCategoryProductsAsync(
  Guid categoryId,
  ProductFilterDto filter)
        {
            var query = _context.Products
                .Where(p => p.CategoryId == categoryId
                         && !p.IsDeleted
                         && p.IsActive);

            // ── Filters ──────────────────────────────────────────────────────────
            if (filter.Status is not null)
                query = query.Where(p => p.Status == filter.Status);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

            //if (!string.IsNullOrWhiteSpace(filter.Brand))
            //    query = query.Where(p => p.Brand != null &&
            //                             p.Brand.ToLower() == filter.Brand.ToLower());

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(p => p.ProductName.Contains(filter.SearchTerm)
                                      || (p.Description != null && p.Description.Contains(filter.SearchTerm)));

            // ── Sorting ───────────────────────────────────────────────────────────
            var descending = string.Equals(filter.SortDirection, "DESC",
                                 StringComparison.OrdinalIgnoreCase);

            query = filter.SortBy?.ToLower() switch
            {
                "price" => descending ? query.OrderByDescending(p => p.Price)
                                            : query.OrderBy(p => p.Price),
                "discount" => descending ? query.OrderByDescending(p => p.Discount)
                                            : query.OrderBy(p => p.Discount),
                "productname" => descending ? query.OrderByDescending(p => p.ProductName)
                                            : query.OrderBy(p => p.ProductName),
                _ => descending ? query.OrderByDescending(p => p.CreatedOn)   // default: newest
                                            : query.OrderBy(p => p.CreatedOn),
            };

            // ── Count + page ──────────────────────────────────────────────────────
            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProductListDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Slug = p.Slug,
                    Price = p.Price,
                    Discount = p.Discount,
                    StockQuantity = p.StockQuantity,
                    ImageUrls = p.ImageUrls,
                    IsActive = p.IsActive,
                    IsFeatured = p.IsFeatured,
                    SKU = p.SKU,
                    ProductDescription = p.ProductDescription,
                    ProductSpecification = p.ProductSpecification,
                    BoxContents = p.BoxContents,
                    Features = p.Features,
                    Status = p.Status ?? "unknown",
                    Brand = p.Brand,
                    CategoryId = p.CategoryId,
                    CategoryName = p.CategoryName,
                    SubCategoryId = p.SubCategoryId ?? Guid.Empty,
                    SubCategoryName = p.SubCategoryName,
                    SubSubCategoryName = p.SubSubCategoryName,
                    MerchantID = p.MerchantID,
                    MetaTitle = p.MetaTitle,
                    MetaDescription = p.MetaDescription,
                    CreatedOn = p.CreatedOn,
                    UpdatedOn = p.UpdatedOn,
                })
                .ToListAsync();

            return new PagedResultDto<ProductListDto>
            {
                Data = products,
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize,
            };
        }

        //public async Task<Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>> GetSubCategoryProductsAsync(Guid subCategoryId, ProductFilterDto filter)
        //{
        //    try
        //    {
        //        // Start with the base query
        //        var baseQuery = _context.Products
        //            .Where(p => p.SubCategoryId == subCategoryId && !p.IsDeleted);

        //        // Count before applying any additional filters
        //        var countBeforeFilters = await baseQuery.CountAsync();
        //        _logger.LogInformation("Products found for SubCategoryId {SubCategoryId} before filters: {Count}", subCategoryId, countBeforeFilters);

        //        // Apply filters
        //        var query = ApplyFilters(baseQuery, filter);

        //        // Get final count after filters
        //        var totalCount = await query.CountAsync();

        //        var products = await query
        //            .Skip((filter.Page - 1) * filter.PageSize)
        //            .Take(filter.PageSize)
        //            .ToListAsync();

        //        // Manually load subcategory information if needed
        //        var subCategoryInfo = await _context.SubCategories
        //            .Where(c => c.SubCategoryId == subCategoryId)
        //            .Select(c => new { c.SubCategoryId, c.Name })
        //            .FirstOrDefaultAsync();

        //        // Map to DTOs using the retrieved product list instead of query
        //        var productDtos = products.Select(p => new ProductListDto
        //        {
        //            ProductId = p.ProductId,
        //            CategoryId = p.CategoryId,
        //            SubCategoryId = p.SubCategoryId ?? Guid.Empty,
        //            CategoryName = p.CategoryName, // Use the property from the product
        //            SubCategoryName = subCategoryInfo?.Name ?? "Unknown SubCategory",
        //            SubSubCategoryName = p.SubSubCategoryName,
        //            ProductName = p.ProductName,
        //            Description = p.Description,
        //            Slug = p.Slug,
        //            MetaTitle = p.MetaTitle,
        //            MetaDescription = p.MetaDescription,
        //            Price = p.Price,
        //            Discount = p.Discount,
        //            StockQuantity = p.StockQuantity,
        //            SKU = p.SKU,
        //            ProductDescription = p.ProductDescription,
        //            ProductSpecification = p.ProductSpecification,
        //            BoxContents = p.BoxContents,
        //            Features = p.Features,
        //            ImageUrls = p.ImageUrls.ToList(),
        //            IsActive = p.IsActive,
        //            IsFeatured = p.IsFeatured,
        //            Status = p.Status ?? "Unknown",
        //            MerchantID = p.MerchantID,
        //            CreatedOn = p.CreatedOn,
        //            UpdatedOn = p.UpdatedOn
        //        }).ToList();

        //        return new Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>
        //        {
        //            Data = productDtos,
        //            TotalCount = totalCount,
        //            PageNumber = filter.Page,
        //            PageSize = filter.PageSize
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error retrieving products for subcategory: {SubCategoryId}", subCategoryId);

        //        // Log the actual SQL query being generated for debugging
        //        _logger.LogError("Query parameters: SubCategoryId={SubCategoryId}, IsDeleted=false", subCategoryId);
        //        throw;
        //    }
        //}
        public async Task<ProductResponseDto> CreateAsync(CreateProductDto createProductDto, string createdBy)
        {
            try
            {
                // Validate SKU uniqueness if provided
                if (!string.IsNullOrEmpty(createProductDto.SKU))
                {
                    var skuExists = await IsSkuUniqueAsync(createProductDto.SKU, createProductDto.MerchantID);
                    if (!skuExists)
                    {
                        throw new ArgumentException($"SKU '{createProductDto.SKU}' already exists for this merchant.");
                    }
                }

                var product = _mapper.Map<Product>(createProductDto);
                product.CreatedBy = createdBy;
                product.IsActive = true; // Set to true by default, can be changed later by merchant

                // **GENERATE SEO SLUG AND META TAGS**
                product.Slug = _slugService.GenerateSlug(createProductDto.ProductName, product.ProductId);
                product.SlugUpdatedAt = DateTime.UtcNow;

                // Auto-generate meta tags for SEO
                product.MetaTitle = $"{createProductDto.ProductName} | Buy in Kenya | QuickCrate";
                
                product.MetaDescription = string.IsNullOrEmpty(createProductDto.Description) 
                    ? $"Buy {createProductDto.ProductName} in Kenya at QuickCrate" 
                    : createProductDto.Description.Length > 250 
                        ? createProductDto.Description.Substring(0, 247) + "..." 
                        : createProductDto.Description;

                product.MetaKeywords = $"{createProductDto.ProductName}, Kenya, QuickCrate, " +
                                      $"{createProductDto.CategoryName}, Buy Online";

                _logger.LogInformation(
                    "Generated slug '{Slug}' for new product '{ProductName}' (ID: {ProductId})", 
                    product.Slug, product.ProductName, product.ProductId);

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return await GetByIdAsync(product.ProductId) ?? throw new InvalidOperationException("Failed to retrieve created product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product for merchant: {MerchantId}", createProductDto.MerchantID);
                throw;
            }
        }

        public async Task<ProductResponseDto> UpdateAsync(UpdateProductDto updateProductDto, string updatedBy)
        {
            try
            {
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == updateProductDto.ProductId && !p.IsDeleted);

                if (existingProduct == null)
                {
                    throw new ArgumentException($"Product with ID {updateProductDto.ProductId} not found or has been deleted.");
                }

                // Validate SKU uniqueness if changed
                if (!string.IsNullOrEmpty(updateProductDto.SKU) && updateProductDto.SKU != existingProduct.SKU)
                {
                    var skuExists = await IsSkuUniqueAsync(updateProductDto.SKU, existingProduct.MerchantID, updateProductDto.ProductId);
                    if (!skuExists)
                    {
                        throw new ArgumentException($"SKU '{updateProductDto.SKU}' already exists for this merchant.");
                    }
                }

                // **CHECK IF PRODUCT NAME CHANGED - REGENERATE SLUG IF NEEDED**
                bool nameChanged = existingProduct.ProductName != updateProductDto.ProductName;

                _mapper.Map(updateProductDto, existingProduct);
                existingProduct.UpdatedBy = updatedBy;

                // **REGENERATE SLUG IF PRODUCT NAME CHANGED**
                if (nameChanged && !string.IsNullOrEmpty(updateProductDto.ProductName))
                {
                    var oldSlug = existingProduct.Slug;
                    var newSlug = _slugService.GenerateSlug(updateProductDto.ProductName, existingProduct.ProductId);

                    existingProduct.Slug = newSlug;
                    existingProduct.SlugUpdatedAt = DateTime.UtcNow;

                    // Store old slug in redirects table for SEO (301 redirect)
                    if (!string.IsNullOrEmpty(oldSlug) && oldSlug != newSlug)
                    {
                        var redirect = new SlugRedirect
                        {
                            OldSlug = oldSlug,
                            NewSlug = newSlug,
                            ProductId = existingProduct.ProductId,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.SlugRedirects.Add(redirect);

                        _logger.LogInformation(
                            "Product name changed. Created slug redirect: {OldSlug} -> {NewSlug} for ProductId {ProductId}",
                            oldSlug, newSlug, existingProduct.ProductId);
                    }

                    // Update meta tags when product name changes
                    existingProduct.MetaTitle = $"{updateProductDto.ProductName} | Buy in Kenya | QuickCrate";
                    existingProduct.MetaDescription = string.IsNullOrEmpty(updateProductDto.Description) 
                        ? $"Buy {updateProductDto.ProductName} in Kenya at QuickCrate" 
                        : updateProductDto.Description.Length > 250 
                            ? updateProductDto.Description.Substring(0, 247) + "..." 
                            : updateProductDto.Description;
                }

                await _context.SaveChangesAsync();

                return await GetByIdAsync(existingProduct.ProductId) ?? throw new InvalidOperationException("Failed to retrieve updated product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", updateProductDto.ProductId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid productId, string deletedBy)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);

                if (product == null) return false;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} permanently deleted by {DeletedBy}", productId, deletedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product: {ProductId}", productId);
                throw;
            }
        }
        public async Task<bool> UpdateProductAsync(string productId, string status)
        {
            try { 
                if (!Guid.TryParse(productId, out Guid guid))
                {
                    _logger.LogWarning("Invalid product ID format: {ProductId}", productId);
                    return false;
                }
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == guid && !p.IsDeleted);
                if (product == null)
                {
                    _logger.LogWarning("Product not found or deleted: {ProductId}", productId);
                    return false;
                }
                product.IsActive = true;
                product.Status = status;
                product.UpdatedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Product {ProductId} status updated to {Status}", productId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product: {ProductId}", productId);
                throw;
            }

            // Simplified placeholder implementation
            return await Task.FromResult(true);
        }

        //Slug

        public async Task<ProductResponseDto?> GetProductBySlugAsync(string slug)
        {
            try
            {
                var cacheKey = $"product_slug{slug}";

                if(_cache.TryGetValue(cacheKey,out ProductResponseDto cachedProduct))
                    return cachedProduct;

                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.SubCategory)
                    .Where(p => p.Slug == slug && !p.IsDeleted && p.IsActive)
                    .FirstOrDefaultAsync();

                if (product == null)
                {
                     var redirect = await _context.SlugRedirects
                        .Where(sr => sr.OldSlug == slug)
                        .OrderByDescending(sr => sr.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (redirect != null)
                    {
                        product = await _context.Products
                            .Include(p => p.Category)
                            .Where(p => p.Slug == redirect.NewSlug && !p.IsDeleted && p.IsActive)
                            .FirstOrDefaultAsync();
                    }
                }

                _cache.Set(cacheKey, _mapper.Map<ProductResponseDto>(product), TimeSpan.FromHours(1));

                return product != null ? _mapper.Map<ProductResponseDto>(product) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product by slug: {Slug}", slug);
                return null;
            }
        }

        public async Task<bool> UpdateProductSlugAsync(Guid productId, string newSlug, string updatedBy)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) return false;

                var oldSlug = product.Slug;
                product.Slug = newSlug;
                product.SlugUpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(oldSlug) && oldSlug != newSlug)
                {
                    _context.SlugRedirects.Add(new SlugRedirect
                    {
                        OldSlug = oldSlug,
                        NewSlug = newSlug,
                        ProductId = productId
                    });
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slug");
                return false;
            }
        }
        #endregion

        #region Advanced Operations

        public async Task<bool> SoftDeleteAsync(Guid productId, string deletedBy)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && !p.IsDeleted);

                if (product == null) return false;

                product.IsDeleted = true;
                product.DeletedBy = deletedBy;
                product.DeletedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} soft deleted by {DeletedBy}", productId, deletedBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<bool> RestoreAsync(Guid productId, string restoredBy)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId && p.IsDeleted);

                if (product == null) return false;

                product.IsDeleted = false;
                product.DeletedBy = null;
                product.DeletedOn = null;
                product.UpdatedBy = restoredBy;
                product.UpdatedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} restored by {RestoredBy}", productId, restoredBy);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring product: {ProductId}", productId);
                throw;
            }
        }

        public async Task<Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>> GetDeletedProductsAsync(Guid merchantId, ProductFilterDto filter)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.SubCategory)
                    .Include(p => p.SubSubCategory)
                    .Where(p => p.MerchantID == merchantId && p.IsDeleted);

                query = ApplyFilters(query, filter);

                return await GetPagedResultAsync(query, filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving deleted products for merchant: {MerchantId}", merchantId);
                throw;
            }
        }

        #endregion

        #region Helper Methods

        private IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductFilterDto filter)
        {
            // --- Filtering ---
            //if (filter.MerchantId.HasValue)
            //    query = query.Where(p => p.MerchantID == filter.MerchantId.Value);

            //if (filter.CategoryId.HasValue)
            //    query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            //if (filter.SubCategoryId.HasValue)
            //    query = query.Where(p => p.SubCategoryId == filter.SubCategoryId.Value);

            //if (filter.SubSubCategoryId.HasValue)
            //    query = query.Where(p => p.SubSubCategoryId == filter.SubSubCategoryId.Value);

            //if (!string.IsNullOrWhiteSpace(filter.ProductName))
            //    query = query.Where(p => p.ProductName.Contains(filter.ProductName.Trim()));

            //if (!string.IsNullOrWhiteSpace(filter.SKU))
            //    query = query.Where(p => p.SKU.Contains(filter.SKU.Trim()));

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            //if (filter.IsActive.HasValue)
            //    query = query.Where(p => p.IsActive == filter.IsActive.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(p => p.Status == filter.Status.Trim());

            //if (!string.IsNullOrWhiteSpace(filter.ProductType))
            //    query = query.Where(p => p.ProductType.Contains(filter.ProductType.Trim()));

            //if (filter.CreatedFrom.HasValue)
            //    query = query.Where(p => p.CreatedOn >= filter.CreatedFrom.Value);

            //if (filter.CreatedTo.HasValue)
            //    query = query.Where(p => p.CreatedOn <= filter.CreatedTo.Value);

            //if (filter.MinStock.HasValue)
            //    query = query.Where(p => p.StockQuantity >= filter.MinStock.Value);

            //if (filter.MaxStock.HasValue)
            //    query = query.Where(p => p.StockQuantity <= filter.MaxStock.Value);


            // --- Sorting ---
            var sortBy = filter.SortBy?.ToLower() ?? "createdon";
            var sortDirection = filter.SortDirection?.ToUpper() ?? "DESC";

            query = (sortBy, sortDirection) switch
            {
                ("productname", "ASC") => query.OrderBy(p => p.ProductName),
                ("productname", "DESC") => query.OrderByDescending(p => p.ProductName),

                ("price", "ASC") => query.OrderBy(p => p.Price),
                ("price", "DESC") => query.OrderByDescending(p => p.Price),

                ("stockquantity", "ASC") => query.OrderBy(p => p.StockQuantity),
                ("stockquantity", "DESC") => query.OrderByDescending(p => p.StockQuantity),

                ("updatedon", "ASC") => query.OrderBy(p => p.UpdatedOn),
                ("updatedon", "DESC") => query.OrderByDescending(p => p.UpdatedOn),

                _ when sortDirection == "ASC" => query.OrderBy(p => p.CreatedOn),
                _ => query.OrderByDescending(p => p.CreatedOn)
            };

            return query;
        }

        private IQueryable<Product> ApplyFiltersWithoutNavigation(IQueryable<Product> query, ProductFilterDto filter)
        {
            // Apply filters that don't require navigation properties
            //if (filter.MerchantId.HasValue)
            //    query = query.Where(p => p.MerchantID == filter.MerchantId.Value);

            //if (filter.SubCategoryId.HasValue)
            //    query = query.Where(p => p.SubCategoryId == filter.SubCategoryId.Value);

            //if (filter.SubSubCategoryId.HasValue)
            //    query = query.Where(p => p.SubSubCategoryId == filter.SubSubCategoryId.Value);

            //if (!string.IsNullOrWhiteSpace(filter.ProductName))
            //    query = query.Where(p => p.ProductName.Contains(filter.ProductName.Trim()));

            //if (!string.IsNullOrWhiteSpace(filter.SKU))
            //    query = query.Where(p => p.SKU.Contains(filter.SKU.Trim()));

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            //if (filter.IsActive.HasValue)
            //    query = query.Where(p => p.IsActive == filter.IsActive.Value);

            if (filter.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == filter.IsFeatured.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(p => p.Status == filter.Status.Trim());

            //if (!string.IsNullOrWhiteSpace(filter.ProductType))
            //    query = query.Where(p => p.ProductType.Contains(filter.ProductType.Trim()));

            //if (filter.CreatedFrom.HasValue)
            //    query = query.Where(p => p.CreatedOn >= filter.CreatedFrom.Value);

            //if (filter.CreatedTo.HasValue)
            //    query = query.Where(p => p.CreatedOn <= filter.CreatedTo.Value);

            //if (filter.MinStock.HasValue)
            //    query = query.Where(p => p.StockQuantity >= filter.MinStock.Value);

            //if (filter.MaxStock.HasValue)
            //    query = query.Where(p => p.StockQuantity <= filter.MaxStock.Value);

            // Apply sorting
            var sortBy = filter.SortBy?.ToLower() ?? "createdon";
            var sortDirection = filter.SortDirection?.ToUpper() ?? "DESC";

            query = (sortBy, sortDirection) switch
            {
                ("productname", "ASC") => query.OrderBy(p => p.ProductName),
                ("productname", "DESC") => query.OrderByDescending(p => p.ProductName),
                ("price", "ASC") => query.OrderBy(p => p.Price),
                ("price", "DESC") => query.OrderByDescending(p => p.Price),
                ("stockquantity", "ASC") => query.OrderBy(p => p.StockQuantity),
                ("stockquantity", "DESC") => query.OrderByDescending(p => p.StockQuantity),
                ("updatedon", "ASC") => query.OrderBy(p => p.UpdatedOn),
                ("updatedon", "DESC") => query.OrderByDescending(p => p.UpdatedOn),
                _ when sortDirection == "ASC" => query.OrderBy(p => p.CreatedOn),
                _ => query.OrderByDescending(p => p.CreatedOn)
            };

            return query;
        }


        private async Task<Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>> GetPagedResultAsync(IQueryable<Product> query, ProductFilterDto filter)
        {
            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var productDtos = _mapper.Map<List<ProductListDto>>(products);

            return new Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>
            {
                Data = productDtos,
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }

        #endregion

        // Legacy methods and all other interface implementations
        public async Task<IEnumerable<Product>> GetAllProducts()
        {
            return await _context.Products
                .Where(p => !p.IsDeleted && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> FetchAllProducts()
        {
            return await _context.Products
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> LoadProductImages(string productId)
        {
            if (Guid.TryParse(productId, out Guid guid))
            {
                return await _context.Products
                    .Where(w => w.ProductId == guid && !w.IsDeleted)
                    .ToListAsync();
            }
            return new List<Product>();
        }

        public async Task<IEnumerable<CartResults>> GetProductsByCategory(int? categoryId)
        {
            return await _context.Products
                .Where(p => !p.IsDeleted && p.IsActive)
                .Select(tp => new CartResults
                {
                    productID = tp.ProductId,
                    ProductName = tp.ProductName,
                    ProductImage = tp.ImageUrls.FirstOrDefault() ?? "",
                    InStock = tp.StockQuantity > 0,
                    price = tp.Price,
                    MerchantId = tp.MerchantID
                })
                .Take(50)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(string productId)
        {
            if (Guid.TryParse(productId, out Guid guid))
            {
                return await _context.Products
                    .Include(p => p.OrderItems)
                    .FirstOrDefaultAsync(p => p.ProductId == guid && !p.IsDeleted);
            }
            return null;
        }

        public async Task<IEnumerable<Product>> GetProductsByIdsAsync(IEnumerable<string> productIds)
        {
            var guidIds = productIds
                .Where(id => Guid.TryParse(id, out _))
                .Select(id => Guid.Parse(id))
                .ToList();

            return await _context.Products
                .Where(p => guidIds.Contains(p.ProductId) && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId, int limit, string excludeProductId)
        {
            if (Guid.TryParse(excludeProductId, out Guid excludeGuid))
            {
                return await _context.Products
                    .Where(p => p.ProductId != excludeGuid &&
                               p.IsActive && !p.IsDeleted &&
                               p.StockQuantity > 0)
                    .Include(p => p.OrderItems)
                    .OrderByDescending(p => p.OrderItems.Count)
                    .Take(limit)
                    .ToListAsync();
            }
            return new List<Product>();
        }

        public async Task<IEnumerable<Product>> GetProductsBySubCategoryAsync(int subCategoryId, int limit, string excludeProductId)
        {
            if (Guid.TryParse(excludeProductId, out Guid excludeGuid))
            {
                return await _context.Products
                    .Where(p => p.ProductId != excludeGuid &&
                               p.IsActive && !p.IsDeleted &&
                               p.StockQuantity > 0)
                    .Include(p => p.OrderItems)
                    .OrderByDescending(p => p.OrderItems.Count)
                    .Take(limit)
                    .ToListAsync();
            }
            return new List<Product>();
        }

        public async Task<IEnumerable<Product>> GetProductsByKeywordsAsync(IEnumerable<string> keywords, int limit, string excludeProductId)
        {
            if (!Guid.TryParse(excludeProductId, out Guid excludeGuid))
            {
                return new List<Product>();
            }

            var query = _context.Products
                .Where(p => p.ProductId != excludeGuid && 
                           p.IsActive && !p.IsDeleted && 
                           p.StockQuantity > 0)
                .Include(p => p.OrderItems);

            var result = new List<Product>();

            foreach (var keyword in keywords.Where(k => k.Length > 3))
            {
                var matches = await query
                    .Where(p => p.ProductName.Contains(keyword) || 
                               p.Description.Contains(keyword) ||
                               p.Features.Contains(keyword))
                    .OrderByDescending(p => p.OrderItems.Count)
                    .Take(limit)
                    .ToListAsync();

                result.AddRange(matches);
                if (result.Count >= limit) break;
            }

            return result.Distinct().Take(limit);
        }

        public async Task<IEnumerable<Product>> GetPopularProductsAsync(int limit, string excludeProductId)
        {
            if (Guid.TryParse(excludeProductId, out Guid excludeGuid))
            {
                return await _context.Products
                    .Where(p => p.ProductId != excludeGuid && 
                               p.IsActive && !p.IsDeleted && 
                               p.StockQuantity > 0)
                    .Include(p => p.OrderItems)
                    .OrderByDescending(p => p.OrderItems.Count)
                    .ThenBy(p => EF.Functions.Random())
                    .Take(limit)
                    .ToListAsync();
            }
            return new List<Product>();
        }

        /// <summary>
        /// Get featured products without merchant filter
        /// </summary>
        /// <param name="count">Number of featured products to return</param>
        /// <returns>List of featured products from all merchants</returns>
        public async Task<List<ProductListDto>> GetFeaturedProductsAsync(int count)
        {
            try
            {
                _logger.LogInformation("Getting {Count} featured products from all merchants", count);

                var featuredProducts = await _context.Products
                    .Include(p => p.Category) // Include Category navigation property
                    .Where(p => p.IsFeatured == true && p.IsActive && !p.IsDeleted)
                    .OrderByDescending(p => p.CreatedOn)
                    .ThenBy(p => EF.Functions.Random()) // Add some randomization
                    .Take(count)
                    .Select(p => new ProductListDto
                    {
                        ProductId = p.ProductId,
                        CategoryId = p.CategoryId,
                        SubCategoryId = p.SubCategoryId ?? Guid.Empty,
                        CategoryName = p.Category != null ? p.Category.Name : "Unknown Category",
                        SubCategoryName = p.SubCategoryName,
                        SubSubCategoryName = p.SubSubCategoryName,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Price = p.Price,
                        Discount = p.Discount,
                        StockQuantity = p.StockQuantity,
                        SKU = p.SKU,
                        ProductDescription = p.ProductDescription,
                        ProductSpecification = p.ProductSpecification,
                        BoxContents = p.BoxContents,
                        Features = p.Features,
                        ImageUrls = p.ImageUrls,
                        IsActive = p.IsActive,
                        IsFeatured = p.IsFeatured ,
                        Status = p.Status ?? "Unknown",
                        MerchantID = p.MerchantID,
                        CreatedOn = p.CreatedOn,
                        UpdatedOn = p.UpdatedOn
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} featured products from all merchants", featuredProducts.Count);
                return featuredProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {Count} featured products from all merchants", count);
                return new List<ProductListDto>();
            }
        }

        /// <summary>
        /// Get featured products with enhanced filtering options
        /// </summary>
        /// <param name="merchantId">Optional merchant ID filter</param>
        /// <param name="count">Number of featured products to return</param>
        /// <param name="categoryId">Optional category ID filter</param>
        /// <returns>List of featured products</returns>
        public async Task<List<ProductListDto>> GetFeaturedProductsAsync(Guid? merchantId, int count, Guid? categoryId)
        {
            try
            {
                _logger.LogInformation("Getting featured products: MerchantId={MerchantId}, Count={Count}, CategoryId={CategoryId}", 
                    merchantId, count, categoryId);

                var query = _context.Products
                    .Include(p => p.Category) // Include Category navigation property
                    .Where(p => p.IsFeatured == true && p.IsActive && !p.IsDeleted);

                // Apply merchant filter if provided
                if (merchantId.HasValue)
                {
                    query = query.Where(p => p.MerchantID == merchantId.Value);
                }

                // Apply category filter if provided
                if (categoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == categoryId.Value);
                }

                var featuredProducts = await query
                    .OrderByDescending(p => p.CreatedOn)
                    .ThenBy(p => EF.Functions.Random()) // Add some randomization
                    .Take(count)
                    .Select(p => new ProductListDto
                    {
                        ProductId = p.ProductId,
                        CategoryId = p.CategoryId,
                        SubCategoryId = p.SubCategoryId ?? Guid.Empty,
                        CategoryName = p.Category != null ? p.Category.Name : "Unknown Category",
                        SubCategoryName = p.SubCategoryName,
                        SubSubCategoryName = p.SubSubCategoryName,
                        ProductName = p.ProductName,
                        Description = p.Description,
                        Price = p.Price,
                        Discount = p.Discount,
                        StockQuantity = p.StockQuantity,
                        SKU = p.SKU,
                        ProductDescription = p.ProductDescription,
                        ProductSpecification = p.ProductSpecification,
                        BoxContents = p.BoxContents,
                        Features = p.Features,
                        ImageUrls = p.ImageUrls,
                        IsActive = p.IsActive,
                        IsFeatured = p.IsFeatured,
                        Status = p.Status ?? "Unknown",
                        MerchantID = p.MerchantID,
                        CreatedOn = p.CreatedOn,
                        UpdatedOn = p.UpdatedOn
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} featured products with filters: MerchantId={MerchantId}, CategoryId={CategoryId}", 
                    featuredProducts.Count, merchantId, categoryId);
                return featuredProducts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured products with filters: MerchantId={MerchantId}, CategoryId={CategoryId}", 
                    merchantId, categoryId);
                return new List<ProductListDto>();
            }
        }

        public async Task<bool> BulkUpdateStatusAsync(BulkUpdateProductStatusDto bulkUpdateDto, string updatedBy) => true;
        public async Task<bool> BulkDeleteAsync(BulkDeleteProductDto bulkDeleteDto, string deletedBy) => true;
        public async Task<bool> BulkSoftDeleteAsync(BulkDeleteProductDto bulkDeleteDto, string deletedBy) => true;
        public async Task<bool> UpdateStatusAsync(Guid productId, string status, string updatedBy) => true;
        public async Task<bool> ToggleActiveStatusAsync(Guid productId, string updatedBy) => true;
        public async Task<bool> ToggleFeaturedStatusAsync(Guid productId, string updatedBy) => true;
        public async Task<bool> UpdateStockAsync(Guid productId, int newStock, string updatedBy) => true;
        public async Task<bool> AdjustStockAsync(Guid productId, int adjustment, string updatedBy, string reason) => true;
        public async Task<List<ProductSummaryDto>> GetLowStockProductsAsync(Guid merchantId, int threshold = 10) => new();
        public async Task<List<ProductSummaryDto>> GetOutOfStockProductsAsync(Guid merchantId) => new();
        //public async Task<Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>> GetProductsBySubCategoryAsync(Guid subCategoryId, ProductFilterDto filter) => new();
        public async Task<Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>> GetProductsBySubSubCategoryAsync(Guid subSubCategoryId, ProductFilterDto filter) => new();
        public async Task<Minimart_Api.DTOS.General.PagedResultDto<ProductListDto>> SearchProductsAsync(string searchTerm, Guid? merchantId, ProductFilterDto filter) => new();
        public async Task<List<ProductListDto>> GetFeaturedProductsAsync(Guid merchantId, int count = 10) => new();
        public async Task<List<ProductSummaryDto>> GetRecentProductsAsync(Guid merchantId, int count = 10) => new();
        public async Task<ProductStatisticsDto> GetProductStatisticsAsync(Guid merchantId) => new();
        public async Task<Dictionary<string, int>> GetProductCountByCategoryAsync(Guid merchantId) => new();
        public async Task<Dictionary<string, decimal>> GetInventoryValueByCategoryAsync(Guid merchantId) => new();
        public async Task<bool> IsSkuUniqueAsync(string sku, Guid merchantId, Guid? excludeProductId = null) => true;
        public async Task<bool> ProductExistsAsync(Guid productId) => true;
        public async Task<bool> ProductBelongsToMerchantAsync(Guid productId, Guid merchantId) => true;
        public async Task<List<ProductResponseDto>> ImportProductsAsync(List<CreateProductDto> products, string createdBy) => new();
        public async Task<byte[]> ExportProductsAsync(Guid merchantId, ProductFilterDto filter) => Array.Empty<byte>();
        public async Task<bool> UpdateProductImagesAsync(Guid productId, List<string> imageUrls, string updatedBy) => true;
        public async Task<bool> AddProductImageAsync(Guid productId, string imageUrl, string updatedBy) => true;
        public async Task<bool> RemoveProductImageAsync(Guid productId, string imageUrl, string updatedBy) => true;
        public async Task<bool> UpdatePriceAsync(Guid productId, decimal newPrice, string updatedBy) => true;
        public async Task<bool> ApplyDiscountAsync(Guid productId, decimal discount, string updatedBy) => true;
        public async Task<bool> BulkUpdatePricesAsync(List<Guid> productIds, decimal priceAdjustment, bool isPercentage, string updatedBy) => true;
        public async Task<ProductResponseDto> DuplicateProductAsync(Guid productId, string newProductName, string createdBy) => new();
        public async Task<List<ProductResponseDto>> CopyProductsToMerchantAsync(List<Guid> productIds, Guid targetMerchantId, string createdBy) => new();
        public async Task<bool> ApproveProductAsync(string productId, string status, string approvedBy)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productId))
                {
                    _logger.LogWarning("Product ID is null or empty");
                    return false;
                }

                if (!Guid.TryParse(productId, out Guid guid))
                {
                    _logger.LogWarning("Invalid product ID format: {ProductId}", productId);
                    return false;
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == guid && !p.IsDeleted);

                if (product == null)
                {
                    _logger.LogWarning("Product not found or deleted: {ProductId}", productId);
                    return false;
                }

                // Validate status
                var validStatuses = new[] { "Approved", "Rejected", "Pending", "Active", "Inactive" };
                if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Invalid status '{Status}' for product {ProductId}", status, productId);
                    return false;
                }

                // Update product
                product.Status = status;
                product.UpdatedBy = approvedBy;
                product.UpdatedOn = DateTime.UtcNow;

                // If approved, set as active
                if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                {
                    product.IsActive = true;
                }
                else if (status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                {
                    product.IsActive = false;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Product {ProductId} approval status updated to '{Status}' by {ApprovedBy}", 
                    productId, status, approvedBy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving product {ProductId}", productId);
                throw;
            }
        }

        /// <summary>
        /// Diagnostic method to help debug category query issues
        /// </summary>
        public async Task<object> DiagnoseCategoryQuery(Guid categoryId)
        {
            try
            {
                // Check if products exist at all
                var totalProducts = await _context.Products.CountAsync();
                
                // Check products with this category ID
                var productsInCategory = await _context.Products
                    .Where(p => p.CategoryId == categoryId)
                    .CountAsync();
                    
                // Check non-deleted products with this category ID
                var activeProductsInCategory = await _context.Products
                    .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                    .CountAsync();
                    
                // Check if category exists
                var categoryExists = await _context.Categories
                    .AnyAsync(c => c.CategoryId == categoryId);
                    
                // Sample products in this category
                var sampleProducts = await _context.Products
                    .Where(p => p.CategoryId == categoryId)
                    .Take(3)
                    .Select(p => new {
                        p.ProductId,
                        p.ProductName,
                        p.CategoryId,
                        p.IsDeleted,
                        p.IsActive
                    })
                    .ToListAsync();

                return new
                {
                    CategoryId = categoryId,
                    TotalProductsInDatabase = totalProducts,
                    ProductsInCategory = productsInCategory,
                    ActiveProductsInCategory = activeProductsInCategory,
                    CategoryExists = categoryExists,
                    SampleProducts = sampleProducts,
                    Diagnosis = activeProductsInCategory == 0 
                        ? "No active products found in this category" 
                        : $"Found {activeProductsInCategory} active products"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DiagnoseCategoryQuery for categoryId: {CategoryId}", categoryId);
                return new { Error = ex.Message, CategoryId = categoryId };
            }
        }
    }
}
