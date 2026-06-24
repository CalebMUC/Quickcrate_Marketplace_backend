namespace Minimart_Api.DTOS.Search
{
    public class AutocompleteSuggestion
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
        public string Type { get; set; } = ""; // "product" | "brand" | "category" | "popular"
        public string? ImageUrl { get; set; }
        public decimal? Price { get; set; }
        public string? Brand { get; set; }
    }

    public class AutocompleteResponse
    {
        public List<AutocompleteSuggestion> Products { get; set; } = new();
        public List<AutocompleteSuggestion> Brands { get; set; } = new();
        public List<AutocompleteSuggestion> Categories { get; set; } = new();
        public List<AutocompleteSuggestion> Popular { get; set; } = new();
    }
}
