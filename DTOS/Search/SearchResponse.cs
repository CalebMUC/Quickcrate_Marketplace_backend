namespace Minimart_Api.DTOS.Search
{
    public class SearchResponse
    {
        public string Query { get; set; } = "";
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public List<ProductSearchResult> Items { get; set; } = new();
        public SearchFacets Facets { get; set; } = new();
    }

    public class ProductSearchResult
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public string? Brand { get; set; }
        public string? ImageUrls { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Slug { get; set; }
        public bool IsFeatured { get; set; }
        public int StockQuantity { get; set; }

        // Computed by the SQL query — not stored columns
        public double TextScore { get; set; }
        public double RatingScore { get; set; }
        public int ReviewCount { get; set; }

        // Convenience helpers
        public decimal EffectivePrice => Discount.HasValue && Discount > 0
                                             ? Price - Discount.Value
                                             : Price;
        public bool IsInStock => StockQuantity > 0;
    }
    public class SearchFacets
    {
        public List<FacetItem> Categories { get; set; } = new();
        public List<FacetItem> Brands { get; set; } = new();
        public List<PriceBucket> PriceBuckets { get; set; } = new();
        public List<SpecFacetGroup> SpecFacets { get; set; } = new();  // ← ADD
    }

    public class FacetItem
    {
        public string Id { get; set; } = "";  // CategoryId or brand string
        public string Name { get; set; } = "";
        public int Count { get; set; }
    }

    public class PriceBucket
    {
        public int Bucket { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int Count { get; set; }
    }

    public class SpecFacetRow
    {
        public string FacetKey { get; set; } = "";
        public string NormalizedKey { get; set; } = "";
        public string FacetValue { get; set; } = "";
        public string NormalizedValue { get; set; } = "";
        public int Count { get; set; }
    }

    public class SpecFacetGroup
    {
        public string FacetKey { get; set; } = "";   // "Panel Type"
        public string NormalizedKey { get; set; } = "";   // "panel_type"
        public List<SpecFacetValue> Values { get; set; } = new();
    }
    public class SpecFacetValue
    {
        public string Label { get; set; } = ""; // "IPS"
        public string NormalizedValue { get; set; } = ""; // "ips"
        public int Count { get; set; }
    }
}
