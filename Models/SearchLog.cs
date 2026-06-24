namespace Minimart_Api.Models
{
    public class SearchLog
    {
        public long Id { get; set; }
        public string Query { get; set; } = "";
        public int ResultCount { get; set; }
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
        public Guid? ClickedProductId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

    public class ClickEvent
    {
        public string Query { get; set; } = "";
        public Guid? ProductId { get; set; }
        // product | brand | category | search
        public string Type { get; set; } = "";
        public string? SessionId { get; set; }
    }
}
