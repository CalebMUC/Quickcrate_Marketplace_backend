namespace Minimart_Api.DTOS.Category
{
    // Application/DTOs/CategoryFacetsDto.cs
    public sealed class CategoryFacetsDto
    {
        public required IReadOnlyList<BrandFacetDto> Brands { get; init; }
        public required IReadOnlyList<SubCategoryFacetDto> SubCategories { get; init; }
        public required PriceRangeFacetDto PriceRange { get; init; }
        public required IReadOnlyList<DiscountFacetDto> Discounts { get; init; }
        public required StockFacetDto InStock { get; init; }
    }

    public sealed class BrandFacetDto
    {
        public required string Name { get; init; }
        public int Count { get; init; }
        public bool Selected { get; init; }
    }

    public sealed class SubCategoryFacetDto
    {
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public int Count { get; init; }
        public bool Selected { get; init; }
    }

    public sealed class PriceRangeFacetDto
    {
        public decimal Min { get; init; }
        public decimal Max { get; init; }
        public decimal? SelectedMin { get; init; }
        public decimal? SelectedMax { get; init; }
    }

    public sealed class DiscountFacetDto
    {
        public int Value { get; init; }   // percentage threshold: 10, 20, 30, 50
        public int Count { get; init; }
        public bool Selected { get; init; }
    }

    public sealed class StockFacetDto
    {
        public int TotalInStock { get; init; }
        public bool Selected { get; init; }
    }
}
