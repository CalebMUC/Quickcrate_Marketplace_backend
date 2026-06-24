using Minimart_Api.Services.OpenSearchService;
using Minimart_Api.Models;
using OpenSearch.Client;

//using OpenSearch.Net;
using System.Collections.Generic;
using System.Threading.Tasks;

public class OpenSearchService : IOpenSearchService
{
    private readonly IOpenSearchClient _client;

    public OpenSearchService(IOpenSearchClient client)
    {
        _client = client;
    }

    // Create the index if it doesn't exist
    public async Task CreateIndexAsync(string indexName)
    {
        var createIndexResponse = await _client.Indices.CreateAsync(indexName, c => c
            .Map<Product>(m => m
                .AutoMap()
                .Properties(p => p
                    .Text(t => t.Name(n => n.ProductName))
                    .Text(t => t.Name(n => n.Description))
                    .Number(n => n.Name(n => n.Price).Type(NumberType.Double))
                    .Text(t => t.Name(n => n.CategoryName))
                )
            )
        );

        if (!createIndexResponse.IsValid)
        {
            throw new Exception("Failed to create index: " + createIndexResponse.DebugInformation);
        }
    }

    // Index a single product
    public async Task IndexProductAsync(Product product)
    {
        var indexResponse = await _client.IndexAsync(product, i => i.Index("searchproducts").Id(product.ProductId));

        if (!indexResponse.IsValid)
        {
            throw new Exception("Failed to index product: " + indexResponse.DebugInformation);
        }
    }

    // Search for products
    public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Index("searchproducts")
            .Query(q => q
                .MultiMatch(m => m
                    .Fields(f => f
                        .Field(p => p.ProductName)
                        .Field(p => p.Description)
                    )
                    .Query(query)
                )
            )
        );

        return response.Documents;
    }

    // Autocomplete suggestions
    public async Task<IEnumerable<string>> AutocompleteAsync(string query)
    {
        var response = await _client.SearchAsync<Product>(s => s
            .Index("searchproducts")
            .Suggest(su => su
                .Completion("product-suggestions", c => c
                    .Field("suggest")
                    .Prefix(query)
                    .Fuzzy(f => f.Fuzziness(Fuzziness.Auto))
                    .Size(5)
                )
            )
        );

        return response.Suggest["product-suggestions"]
            .SelectMany(s => s.Options)
            .Select(o => o.Text);
    }
}