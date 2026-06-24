using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Category;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.SubCategory;
using Minimart_Api.Exceptions;
using Minimart_Api.Models;

namespace Minimart_Api.Repositories.Category
{
    public class CategoryRepo : ICategoryRepo
    {
        private readonly MinimartDBContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryRepo> _logger;

        public CategoryRepo(
            MinimartDBContext context,
            IMapper mapper,
            ILogger<CategoryRepo> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<PagedResultDto<CategoryResponseDto>> GetCategoriesAsync(CategoryQueryDto query)
        {
            try
            {
                _logger.LogInformation("Fetching categories with query: {@Query}", query);

                var queryable = _context.Categories
                    .Include(c => c.SubCategories)
                        .ThenInclude(sc=> sc.Products.Where(p=> !p.IsDeleted && p.IsActive))
                    .Include(c => c.SubCategories)
                        .ThenInclude(sc => sc.SubSubCategories)
                    .Where(c => c.IsActive); // Only active categories by default

                // Apply filters
                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    queryable = queryable.Where(c =>
                        c.Name.Contains(query.SearchTerm) ||
                        (c.Description != null && c.Description.Contains(query.SearchTerm)));
                }

                if (query.IsActive.HasValue)
                {
                    queryable = queryable.Where(c => c.IsActive == query.IsActive.Value);
                }

                if (query.ParentId.HasValue)
                {
                    queryable = queryable.Where(c => c.ParentId == query.ParentId.Value);
                }

                // Apply sorting
                queryable = query.SortBy?.ToLower() switch
                {
                    "name" => query.SortOrder?.ToLower() == "desc"
                        ? queryable.OrderByDescending(c => c.Name)
                        : queryable.OrderBy(c => c.Name),
                    "createdon" => query.SortOrder?.ToLower() == "desc"
                        ? queryable.OrderByDescending(c => c.CreatedOn)
                        : queryable.OrderBy(c => c.CreatedOn),
                    "sortorder" => query.SortOrder?.ToLower() == "desc"
                        ? queryable.OrderByDescending(c => c.SortOrder)
                        : queryable.OrderBy(c => c.SortOrder),
                    _ => queryable.OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
                };

                // Get total count
                var totalCount = await queryable.CountAsync();

                // Apply pagination
                var categories = await queryable
                    .Skip((query.PageNumber - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .ToListAsync();

                //var mappedCategories = _mapper.Map<List<CategoryResponseDto>>(categories);
                var mappedCategories = categories.Select(category =>
                {
                    var dto = _mapper.Map<CategoryResponseDto>(category);

                    dto.ProductCount = category.SubCategories?
                        .Where(sc => sc.IsActive)
                        .Sum(sc => sc.Products?.Count(p => !p.IsDeleted && p.IsActive) ?? 0) ?? 0;

                    if (dto.SubCategories != null)
                    {
                        foreach (var subCategoryDto in dto.SubCategories)
                        {
                            var subCategory = category.SubCategories?
                                .FirstOrDefault(sc => sc.SubCategoryId == subCategoryDto.SubCategoryId);

                            if (subCategory != null)
                            {
                                subCategoryDto.ProductCount = subCategory.Products?
                                    .Count(p => !p.IsDeleted && p.IsActive) ?? 0;
                            }
                        }
                    }
                    return dto;
                }).ToList();

                return new PagedResultDto<CategoryResponseDto>
                {
                    Data = mappedCategories,
                    TotalCount = totalCount,
                    PageNumber = query.PageNumber,
                    PageSize = query.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching categories");
                return new PagedResultDto<CategoryResponseDto>();
            }
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(Guid categoryId)
        {
            try
            {
                _logger.LogInformation("Fetching category with ID: {CategoryId}", categoryId);

                var category = await _context.Categories
                    .Include(c => c.SubCategories)
                        .ThenInclude(sc => sc.SubSubCategories)
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (category == null)
                {
                    _logger.LogWarning("Category not found. CategoryId: {CategoryId}", categoryId);
                    return null;
                }

                return _mapper.Map<CategoryResponseDto>(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching category {CategoryId}", categoryId);
                return null;
            }
        }

        public async Task<List<CategoryResponseDto>> GetCategoriesByParentIdAsync(Guid? parentId)
        {
            try
            {
                _logger.LogInformation("Fetching categories by parent ID: {ParentId}", parentId);

                var categories = await _context.Categories
                    .Include(c => c.SubCategories)
                        .ThenInclude(sc => sc.SubSubCategories)
                    .Where(c => c.ParentId == parentId && c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                return _mapper.Map<List<CategoryResponseDto>>(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching categories by parent ID {ParentId}", parentId);
                return new List<CategoryResponseDto>();
            }
        }

        public async Task<List<CategoryResponseDto>> GetRootCategoriesAsync()
        {
            try
            {
                _logger.LogInformation("Fetching root categories");
                return await GetCategoriesByParentIdAsync(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching root categories");
                return new List<CategoryResponseDto>();
            }
        }

        public async Task<List<CategoryResponseDto>> GetCategoriesByMerchantIdAsync(Guid merchantId)
        {
            try
            {
                _logger.LogInformation("Fetching categories by merchant ID: {MerchantId}", merchantId);

                var categories = await _context.Categories
                    .Include(c => c.SubCategories)
                        .ThenInclude(sc => sc.SubSubCategories)
                    .Where(c => c.MerchantID == merchantId && c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();

                return _mapper.Map<List<CategoryResponseDto>>(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching categories by merchant ID {MerchantId}", merchantId);
                return new List<CategoryResponseDto>();
            }
        }

        // Additional methods for creating, updating, and deleting categories can be added here

        public async Task<PagedResultDto<CategoryResponseDto>> GetCategoriesAsync(
      Guid merchantId,
      CategoryQueryDto query)
        {
            // Base query (IQueryable<Category>)
            IQueryable<Minimart_Api.Models.Category> queryable = _context.Categories
                .Where(c => c.MerchantID == merchantId)
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.SubSubCategories);

            // Apply filters
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                queryable = queryable.Where(c =>
                    c.Name.Contains(query.SearchTerm) ||
                    (c.Description != null && c.Description.Contains(query.SearchTerm)));
            }

            if (query.IsActive.HasValue)
            {
                queryable = queryable.Where(c => c.IsActive == query.IsActive.Value);
            }

            // Apply sorting
            queryable = query.SortBy?.ToLower() switch
            {
                "name" => query.SortOrder == "desc"
                    ? queryable.OrderByDescending(c => c.Name)
                    : queryable.OrderBy(c => c.Name),
                "createdon" => query.SortOrder == "desc"
                    ? queryable.OrderByDescending(c => c.CreatedOn)
                    : queryable.OrderBy(c => c.CreatedOn),
                _ => queryable.OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            };

            // Pagination
            var totalCount = await queryable.CountAsync();

            var categories = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var result = _mapper.Map<List<CategoryResponseDto>>(categories);

            return new PagedResultDto<CategoryResponseDto>
            {
                Data = result,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }


        public async Task<CategoryResponseDto> GetCategoryByIdAsync(Guid categoryId, Guid merchantId)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                    .ThenInclude(sc => sc.SubSubCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.MerchantID == merchantId);

            if (category == null)
            {
                throw new NotFoundException($"Category with ID {categoryId} not found.");
            }

            _logger.LogInformation(
                "Category retrieved: {CategoryId} for merchant {MerchantId}",
                categoryId, merchantId);

            return _mapper.Map<CategoryResponseDto>(category);
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(
      CreateCategoryDto dto,
      Guid merchantId,
      string userId)
        {
            // Generate slug if not provided
            var slug = !string.IsNullOrEmpty(dto.Slug)
                ? dto.Slug.ToLowerInvariant()
                : GenerateSlug(dto.Name);

            // Ensure slug is unique
            slug = await EnsureUniqueSlugAsync(slug, merchantId);

            var category = new Models.Category
            {
                Name = dto.Name,
                Description = dto.Description,
                Slug = slug,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder,
                MerchantID = merchantId,
                ParentId = dto.ParentId,
                ImageUrl = dto.ImageUrl,
                MetaTitle = dto.MetaTitle,
                MetaDescription = dto.MetaDescription,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Category created: {CategoryId} by {UserId}",
                category.CategoryId, userId);

            return _mapper.Map<CategoryResponseDto>(category);
        }


        public async Task<CategoryResponseDto> UpdateCategoryAsync(
        Guid CategoryId,
        UpdateCategoryDto dto,
        Guid merchantID,
        string userId)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == CategoryId && c.MerchantID == merchantID);

            if (category == null)
            {
                throw new NotFoundException($"Category with ID {CategoryId} not found.");
            }

            // Update fields
            if (!string.IsNullOrEmpty(dto.Name))
                category.Name = dto.Name;

            if (dto.Description != null)
                category.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Slug))
            {
                var slug = dto.Slug.ToLowerInvariant();
                if (slug != category.Slug)
                {
                    category.Slug = await EnsureUniqueSlugAsync(slug, merchantID, CategoryId);
                }
            }

            if (dto.IsActive.HasValue)
                category.IsActive = dto.IsActive.Value;

            if (dto.SortOrder.HasValue)
                category.SortOrder = dto.SortOrder.Value;

            if (dto.ParentId.HasValue)
                category.ParentId = dto.ParentId.Value;

            if (dto.ImageUrl != null)
                category.ImageUrl = dto.ImageUrl;

            if (dto.MetaTitle != null)
                category.MetaTitle = dto.MetaTitle;

            if (dto.MetaDescription != null)
                category.MetaDescription = dto.MetaDescription;

            category.UpdatedBy = userId;
            category.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Category updated: {CategoryId} by {UserId}",
                category.CategoryId, userId);

            return _mapper.Map<CategoryResponseDto>(category);
        }

        public async Task DeleteCategoryAsync(Guid CategoryId, Guid merchantId)
        {
            var category = await _context.Categories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == CategoryId && c.MerchantID == merchantId);

            if (category == null)
            {
                throw new NotFoundException($"Category with ID {CategoryId} not found.");
            }

            // Check if category has products
            if (category.Products.Any())
            {
                throw new BadRequestException(
                    "Cannot delete category with associated products. " +
                    "Please move or delete products first.");
            }

            // Check if category has subcategories
            if (category.SubCategories.Any())
            {
                throw new BadRequestException(
                    "Cannot delete category with subcategories. " +
                    "Please delete subcategories first.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Category deleted: {CategoryId}", category.CategoryId);
        }

        private string GenerateSlug(string name)
        {
            return name
                .ToLowerInvariant()
                .Replace(" ", "-")
                .Replace("&", "and")
                .Replace("/", "-")
                .Replace("\\", "-")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("'", "")
                .Replace("\"", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "")
                .Trim('-');
        }

        private async Task<string> EnsureUniqueSlugAsync(
            string baseSlug,
            Guid merchantId,
            Guid? excludeId = null)
        {
            var slug = baseSlug;
            var counter = 1;

            while (await SlugExistsAsync(slug, merchantId, excludeId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }


        public async Task<List<SubCategoryResponseDto>> GetSubCategoriesAsync(
       Guid categoryId,
       Guid merchantId)
        {


            var subCategories = await _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId && sc.MerchantID == merchantId)
                .Include(sc => sc.SubSubCategories)
                .OrderBy(sc => sc.SortOrder)
                .ThenBy(sc => sc.Name)
                .ToListAsync();



            _logger.LogInformation(
                "SubCategories retrieved for Category: {CategoryId}, Merchant: {MerchantId}, Count: {Count}",
                categoryId, merchantId, subCategories.Count);

            return _mapper.Map<List<SubCategoryResponseDto>>(subCategories);
        }

        public async Task<List<SubCategoryResponseDto>> GetAllSubCategoriesAsync(Guid merchantId, bool includeProducts = false)
        {
            try
            {
                _logger.LogInformation("Fetching all subcategories with parent category for merchant: {MerchantId}, includeProducts: {IncludeProducts}", merchantId, includeProducts);

                IQueryable<SubCategory> query = _context.SubCategories
                    .Where(sc => sc.MerchantID == merchantId && sc.IsActive) // Only active subcategories
                    .Include(sc => sc.Category); // Always include parent category

                // Conditionally include products
                if (includeProducts)
                {
                    query = query.Include(sc => sc.Products.Where(p => !p.IsDeleted && p.IsActive));
                }

                var subCategories = await query
                    .OrderBy(sc => sc.Category.SortOrder) // Order by parent category first
                    .ThenBy(sc => sc.SortOrder)
                    .ThenBy(sc => sc.Name)
                    .ToListAsync();

                _logger.LogInformation(
                    "All SubCategories retrieved for Merchant: {MerchantId}, Count: {Count}",
                    merchantId, subCategories.Count);

                return _mapper.Map<List<SubCategoryResponseDto>>(subCategories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all subcategories for merchant {MerchantId}", merchantId);
                return new List<SubCategoryResponseDto>();
            }
        }


        public async Task<SubCategoryResponseDto> GetSubCategoryByIdAsync(
       Guid subCategoryId,
       Guid merchantId)
        {
            var subCategory = await _context.SubCategories
                .Include(sc => sc.SubSubCategories)
                .Include(sc => sc.Category)
                .FirstOrDefaultAsync(sc => sc.SubCategoryId == subCategoryId && sc.MerchantID == merchantId);

            if (subCategory == null)
            {
                throw new NotFoundException($"SubCategory with ID {subCategoryId} not found.");
            }

            _logger.LogInformation(
                "SubCategory retrieved: {SubCategoryId} for merchant {MerchantId}",
                subCategoryId, merchantId);

            return _mapper.Map<SubCategoryResponseDto>(subCategory);
        }


        public async Task<SubCategoryResponseDto> CreateSubCategoryAsync(
         Guid categoryId,
         CreateSubCategoryDto dto,
         Guid merchantId,
         string userId)
        {
            // Verify category exists and belongs to merchant
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.MerchantID == merchantId);

            if (category == null)
            {
                throw new NotFoundException($"Category with ID {categoryId} not found.");
            }

            // Generate slug if not provided
            var slug = !string.IsNullOrEmpty(dto.Slug)
                ? dto.Slug.ToLowerInvariant()
                : GenerateSlug(dto.Name);

            // Ensure slug is unique within merchant scope
            slug = await EnsureUniqueSubCategorySlugAsync(slug, merchantId);

            var subCategory = new SubCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                Slug = slug,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder,
                CategoryId = categoryId,
                MerchantID = merchantId,
                ImageUrl = dto.ImageUrl,
                CreatedBy = userId,
                CreatedOn = DateTime.UtcNow
            };

            _context.SubCategories.Add(subCategory);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "SubCategory created: {SubCategoryId} under Category {CategoryId} by {UserId}",
                subCategory.SubCategoryId, categoryId, userId);

            return _mapper.Map<SubCategoryResponseDto>(subCategory);
        }

        public async Task<SubCategoryResponseDto> UpdateSubCategoryAsync(
    Guid SubCategoryId,
    UpdateSubCategoryDto dto,
    Guid merchantId,
    string userId)
        {
            var subCategory = await _context.SubCategories
                .FirstOrDefaultAsync(sc => sc.SubCategoryId == SubCategoryId && sc.MerchantID == merchantId);

            if (subCategory == null)
            {
                throw new NotFoundException($"SubCategory with ID {subCategory.SubCategoryId} not found.");
            }

            // Update fields
            if (!string.IsNullOrEmpty(dto.Name))
                subCategory.Name = dto.Name;

            if (dto.Description != null)
                subCategory.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Slug))
            {
                var slug = dto.Slug.ToLowerInvariant();
                if (slug != subCategory.Slug)
                {
                    subCategory.Slug = await EnsureUniqueSubCategorySlugAsync(slug, merchantId, SubCategoryId);
                }
            }

            if (dto.IsActive.HasValue)
                subCategory.IsActive = dto.IsActive.Value;

            if (dto.SortOrder.HasValue)
                subCategory.SortOrder = dto.SortOrder.Value;

            if (dto.CategoryId.HasValue)
            {
                // Verify new category exists and belongs to merchant
                var newCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId.Value && c.MerchantID == merchantId);

                if (newCategory == null)
                {
                    throw new NotFoundException($"Category with ID {dto.CategoryId.Value} not found.");
                }

                subCategory.CategoryId = dto.CategoryId.Value;
            }

            if (dto.ImageUrl != null)
                subCategory.ImageUrl = dto.ImageUrl;

            subCategory.UpdatedBy = userId;
            subCategory.UpdatedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "SubCategory updated: {SubCategoryId} by {UserId}",
                subCategory.SubCategoryId, userId);

            return _mapper.Map<SubCategoryResponseDto>(subCategory);
        }

        public async Task DeleteSubCategoryAsync(Guid subCategoryId, Guid merchantId)
        {
            var subCategory = await _context.SubCategories
                .Include(sc => sc.SubSubCategories)
                .Include(sc => sc.Products)
                .FirstOrDefaultAsync(sc => sc.SubCategoryId == subCategoryId && sc.MerchantID == merchantId);

            if (subCategory == null)
            {
                throw new NotFoundException($"SubCategory with ID {subCategoryId} not found.");
            }

            // Check if subcategory has products
            if (subCategory.Products.Any())
            {
                throw new BadRequestException(
                    "Cannot delete subcategory with associated products. " +
                    "Please move or delete products first.");
            }

            // Check if subcategory has sub-subcategories
            if (subCategory.SubSubCategories.Any())
            {
                throw new BadRequestException(
                    "Cannot delete subcategory with sub-subcategories. " +
                    "Please delete sub-subcategories first.");
            }

            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "SubCategory deleted: {SubCategoryId}", subCategory.SubCategoryId);
        }

        // Helper methods for slug uniqueness
        private async Task<string> EnsureUniqueSubCategorySlugAsync(
            string baseSlug,
           Guid merchantId,
            Guid? excludeId = null)
        {
            var slug = baseSlug;
            var counter = 1;

            while (await SubCategorySlugExistsAsync(slug, merchantId, excludeId))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }

        private async Task<bool> SubCategorySlugExistsAsync(
            string slug,
            Guid merchantId,
            Guid? excludeId = null)
        {
            return await _context.SubCategories
                .AnyAsync(sc =>
                    sc.Slug == slug &&
                    sc.MerchantID == merchantId &&
                    (excludeId == null || sc.SubCategoryId != excludeId));
        }

        private async Task<bool> SlugExistsAsync(
            string slug,
            Guid merchantId,
            Guid? excludeId = null)
        {
            return await _context.Categories
                .AnyAsync(c =>
                    c.Slug == slug &&
                    c.MerchantID == merchantId &&
                    (excludeId == null || c.CategoryId != excludeId));
        }
    }
}
