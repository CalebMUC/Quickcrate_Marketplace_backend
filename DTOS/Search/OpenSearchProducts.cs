using OpenSearch.Client;
using OpenSearch.Net;

namespace Minimart_Api.DTOS.Search
{
    public class OpenSearchProducts
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string Category { get; set; }
        public CompletionField Suggest { get; set; }
    }
}
