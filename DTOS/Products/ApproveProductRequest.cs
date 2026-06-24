namespace Minimart_Api.DTOS.Products
{
    public class ApproveProductRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }
}
