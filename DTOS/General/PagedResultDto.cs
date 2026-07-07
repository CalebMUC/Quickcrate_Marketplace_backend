using Minimart_Api.DTOS.Category;

namespace Minimart_Api.DTOS.General
{
    public class PagedResultDto<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
    }

    // Application/DTOs/PagedResultDto.cs
    public sealed class ProductPagedResultDto<T>
    {
        public required IReadOnlyList<T> Products { get; init; }
        public required PaginationDto Pagination { get; init; }
        public required CategoryFacetsDto Facets { get; init; }
        public required AppliedFiltersDto AppliedFilters { get; init; }
        public required CategoryMetaDto Meta { get; init; }
    }

    public sealed class PaginationDto
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalItems { get; init; }
        public int TotalPages { get; init; }
        public bool HasPrevious { get; init; }
        public bool HasNext { get; init; }
    }

    public sealed class AppliedFiltersDto
    {
        public IReadOnlyList<string> Brands { get; init; } = [];
        public Guid? SubCategoryId { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }
        public int? MinDiscount { get; init; }
        public bool InStockOnly { get; init; }
        public string SortBy { get; init; } = "CreatedOn";
        public string SortDirection { get; init; } = "DESC";
    }

    public sealed class CategoryMetaDto
    {
        public Guid CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public long QueryDuration { get; init; }   // milliseconds — useful for monitoring
    }
}