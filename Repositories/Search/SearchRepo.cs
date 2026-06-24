using Dapper;
using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Cart;
using Minimart_Api.DTOS.General;
using Minimart_Api.DTOS.Products;
using Minimart_Api.DTOS.Search;
using Minimart_Api.Models;
using Minimart_Api.Repositories.Search;
using OpenSearch.Client;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace Minimart_Api.Repositories.Search
{
    public class SearchRepo : ISearchRepo
    {
        private readonly MinimartDBContext _context;
        private readonly ILogger<SearchRepo> _logger;
        private readonly IConnectionMultiplexer _redis;
        private const string PopularKey = "search:popular";

        public SearchRepo(MinimartDBContext context, ILogger<SearchRepo> logger, IConnectionMultiplexer redis)
        {
            _context = context;
            _logger = logger;
            _redis = redis;
        }

        // Spec keys that are too noisy or non-filterable to show as facets
        private static readonly HashSet<string> ExcludedSpecKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "model", "design", "power_consumption", "color_support",
            "viewing_angle", "box_contents", "mounting_type", "vesa_mount",
            "aspect_ratio", "usb-c_power_delivery"
        };

        public async Task<SearchResponse> SearchAsync(DTOS.Search.SearchRequest req)
        {
            var query = req.Q.Trim();
            var tsQuery = BuildTsQuery(query);

            var conn = _context.Database.GetDbConnection();

            // Build spec filter JOINs dynamically from SpecFilters dictionary
            var specJoins = BuildSpecJoins(req.SpecFilters);

            // ── Core SQL ─────────────────────────────────────────────────────────
            var baseSql = $@"
            SELECT
                p.""ProductId"",
                p.""ProductName"",
                p.""Description"",
                p.""Price"",
                p.""Discount"",
                p.""Brand"",
                p.""CategoryId""::text,
                p.""CategoryName"",
                p.""SubCategoryId""::text,
                p.""SubCategoryName"",
                p.""Slug"",
                p.""IsFeatured"",
                p.""IsActive"",
                p.""StockQuantity"",
                p.""MerchantID""::text,
                p.""MetaTitle"",
                p.""MetaDescription"",
                -- Extract first image from Postgres text[] array
                p.""ImageUrls""[1] AS ImageUrl,

                -- Full-text relevance score (weights: A=1.0, B=0.4, C=0.2, D=0.1)
                ts_rank_cd(p.search_vector, to_tsquery('english', @tsQuery), 32) AS TextScore,
                -- Rating signals from Reviews
                COALESCE(r.avg_rating, 0)   AS RatingScore,
                COALESCE(r.review_count, 0) AS ReviewCount
            FROM ""Products"" p
            {specJoins}
            LEFT JOIN (
                SELECT
                    ""ProductId"",
                    AVG(""Rating"")::float  AS avg_rating,
                    COUNT(*)::int          AS review_count
                FROM ""Reviews""
                WHERE ""IsVisible"" = true
                GROUP BY ""ProductId""
            ) r ON r.""ProductId"" = p.""ProductId""
            WHERE
                p.""IsActive""   = true
                AND p.""IsDeleted"" = false
                AND (
                    p.search_vector @@ to_tsquery('english', @tsQuery)
                    OR p.""ProductName"" ILIKE @likeQuery
                )
                AND (@categoryId::uuid    IS NULL OR p.""CategoryId""    = @categoryId::uuid)
                AND (@subCategoryId::uuid IS NULL OR p.""SubCategoryId"" = @subCategoryId::uuid)
                AND (@brand      IS NULL OR LOWER(p.""Brand"")      = LOWER(@brand))
                AND (@merchantId IS NULL OR p.""MerchantID""::text  = @merchantId)
                AND (@minPrice   IS NULL OR p.""Price"" >= @minPrice)
                AND (@maxPrice   IS NULL OR p.""Price"" <= @maxPrice)";

            // ── ORDER BY ─────────────────────────────────────────────────────────
            var orderBy = req.SortBy switch
            {
                "price_asc" => @"ORDER BY p.""Price"" ASC",
                "price_desc" => @"ORDER BY p.""Price"" DESC",
                "newest" => @"ORDER BY p.""CreatedOn"" DESC",
                "rating" => @"ORDER BY RatingScore DESC, ReviewCount DESC",
                // Default: composite relevance score (Amazon-style)
                // text rank × featured boost × stock boost + rating nudge
                _ => @"ORDER BY (
                        ts_rank_cd(p.search_vector, to_tsquery('english', @tsQuery), 32)
                        * CASE WHEN p.""IsFeatured""    THEN 1.5 ELSE 1.0 END
                        * CASE WHEN p.""StockQuantity"" > 0 THEN 1.0 ELSE 0.2 END
                        + (COALESCE(r.avg_rating, 0) / 5.0 * 0.2)
                   ) DESC"
            };

            var parameters = new
            {
                tsQuery,
                likeQuery = $"%{query}%",
                categoryId = req.CategoryId,
                subCategoryId = req.SubCategoryId,
                brand = req.Brand,
                merchantId = req.MerchantId,
                minPrice = req.MinPrice,
                maxPrice = req.MaxPrice,
                pageSize = req.SafePageSize,
                offset = req.Offset
            };

            var countSql = $"SELECT COUNT(*) FROM ({baseSql}) AS _count";
            var pageSql = $"{baseSql} {orderBy} LIMIT @pageSize OFFSET @offset";

            // Run count + page query in parallel
            var countTask = await conn.ExecuteScalarAsync<int>(countSql, parameters);
            var productsTask = await conn.QueryAsync<ProductSearchResult>(pageSql, parameters);

            //await Task.WhenAll(countTask, productsTask);

            var total =  countTask;
            var products =  productsTask;

            // Build facets in parallel with the main query results
            var facets = await BuildFacetsAsync(tsQuery, req);

            return new SearchResponse
            {
                Query = query,
                TotalCount = total,
                Page = req.Page,
                PageSize = req.SafePageSize,
                Items = products.ToList(),
                Facets = facets
            };
        }

        // ── Facet builder ─────────────────────────────────────────────────────────

        //private async Task<SearchFacets> BuildFacetsAsync(string tsQuery, DTOS.Search.SearchRequest req)
        //{
        //    var conn = _context.Database.GetDbConnection();
        //    var p = new { tsQuery };

        //    // Category counts
        //    var categorySql = @"
        //    SELECT
        //        c.""CategoryId""::text AS Id,
        //        c.""Name""             AS Name,
        //        COUNT(p.""ProductId"") AS Count
        //    FROM ""Products"" p
        //    JOIN ""Categories"" c ON c.""CategoryId"" = p.""CategoryId""
        //    WHERE p.search_vector @@ to_tsquery('english', @tsQuery)
        //      AND p.""IsActive""   = true
        //      AND p.""IsDeleted""  = false
        //    GROUP BY c.""CategoryId"", c.""Name""
        //    ORDER BY Count DESC
        //    LIMIT 10";

        //    // Brand counts
        //    var brandSql = @"
        //    SELECT
        //        p.""Brand"" AS Id,
        //        p.""Brand"" AS Name,
        //        COUNT(*)    AS Count
        //    FROM ""Products"" p
        //    WHERE p.search_vector @@ to_tsquery('english', @tsQuery)
        //      AND p.""IsActive""  = true
        //      AND p.""IsDeleted"" = false
        //      AND p.""Brand""     IS NOT NULL
        //      AND p.""Brand""     != ''
        //    GROUP BY p.""Brand""
        //    ORDER BY Count DESC
        //    LIMIT 15";

        //    // Price histogram — 10 equal buckets up to 50,000 KES
        //    var priceSql = @"
        //    SELECT
        //        width_bucket(""Price"", 0, 50000, 10) AS Bucket,
        //        MIN(""Price"")::numeric               AS MinPrice,
        //        MAX(""Price"")::numeric               AS MaxPrice,
        //        COUNT(*)::int                         AS Count
        //    FROM ""Products""
        //    WHERE search_vector @@ to_tsquery('english', @tsQuery)
        //      AND ""IsActive""   = true
        //      AND ""IsDeleted""  = false
        //    GROUP BY Bucket
        //    ORDER BY Bucket";

        //    // Dynamic spec facets from ProductSpecFacets table
        //    // Builds: Panel Type → [IPS (3), TN (1)], Refresh Rate → [75Hz (2)] etc.
        //    var specFacetsSql = @"
        //    SELECT
        //        sf.""FacetKey""        AS FacetKey,
        //        sf.""NormalizedKey""   AS NormalizedKey,
        //        sf.""FacetValue""      AS FacetValue,
        //        sf.""NormalizedValue"" AS NormalizedValue,
        //        COUNT(*)::int          AS Count
        //    FROM ""ProductSpecFacets"" sf
        //    JOIN ""Products"" p ON p.""ProductId"" = sf.""ProductId""
        //    WHERE p.search_vector @@ to_tsquery('english', @tsQuery)
        //      AND p.""IsActive""  = true
        //      AND p.""IsDeleted"" = false
        //    GROUP BY
        //        sf.""FacetKey"",
        //        sf.""NormalizedKey"",
        //        sf.""FacetValue"",
        //        sf.""NormalizedValue""
        //    HAVING COUNT(*) >= 1
        //    ORDER BY sf.""NormalizedKey"", Count DESC";

        //    // Run all facet queries in parallel
        //    var categoriesTask = conn.QueryAsync<FacetItem>(categorySql, p);
        //    var brandsTask = conn.QueryAsync<FacetItem>(brandSql, p);
        //    var priceTask = conn.QueryAsync<PriceBucket>(priceSql, p);
        //    var specFacetsTask = conn.QueryAsync<SpecFacetRow>(specFacetsSql, p);

        //    await Task.WhenAll(categoriesTask, brandsTask, priceTask, specFacetsTask);

        //    // Group spec rows into SpecFacetGroup objects
        //    // and filter out noisy keys that don't make good filters
        //    var specFacets = (await specFacetsTask)
        //        .Where(r => !ExcludedSpecKeys.Contains(r.NormalizedKey))
        //        .GroupBy(r => new { r.FacetKey, r.NormalizedKey })
        //        .Select(g => new SpecFacetGroup
        //        {
        //            FacetKey = g.Key.FacetKey,
        //            NormalizedKey = g.Key.NormalizedKey,
        //            Values = g.Select(r => new SpecFacetValue
        //            {
        //                Label = r.FacetValue,
        //                NormalizedValue = r.NormalizedValue,
        //                Count = r.Count
        //            }).ToList()
        //        })
        //        .ToList();

        //    return new SearchFacets
        //    {
        //        Categories = (await categoriesTask).ToList(),
        //        Brands = (await brandsTask).ToList(),
        //        PriceBuckets = (await priceTask).ToList(),
        //        SpecFacets = specFacets
        //    };
        //}

        private async Task<SearchFacets> BuildFacetsAsync(string tsQuery, DTOS.Search.SearchRequest req)
        {
            var conn = _context.Database.GetDbConnection();
            var p = new { tsQuery };

            // All 4 queries in one round trip using Dapper QueryMultiple
            var combinedSql = $@"
        -- 1. Categories
        SELECT c.""CategoryId""::text AS Id, c.""Name"", COUNT(p.""ProductId"")::int AS Count
        FROM ""Products"" p
        JOIN ""Categories"" c ON c.""CategoryId"" = p.""CategoryId""
        WHERE p.search_vector @@ to_tsquery('english', @tsQuery)
          AND p.""IsActive"" = true AND p.""IsDeleted"" = false
        GROUP BY c.""CategoryId"", c.""Name""
        ORDER BY Count DESC LIMIT 10;

        -- 2. Brands
        SELECT p.""Brand"" AS Id, p.""Brand"" AS Name, COUNT(*)::int AS Count
        FROM ""Products"" p
        WHERE p.search_vector @@ to_tsquery('english', @tsQuery)
          AND p.""IsActive"" = true AND p.""IsDeleted"" = false
          AND p.""Brand"" IS NOT NULL AND p.""Brand"" != ''
        GROUP BY p.""Brand""
        ORDER BY Count DESC LIMIT 15;

        -- 3. Price buckets
        SELECT
            width_bucket(""Price"", 0, 50000, 10) AS Bucket,
            MIN(""Price"")::numeric AS MinPrice,
            MAX(""Price"")::numeric AS MaxPrice,
            COUNT(*)::int AS Count
        FROM ""Products""
        WHERE search_vector @@ to_tsquery('english', @tsQuery)
          AND ""IsActive"" = true AND ""IsDeleted"" = false
        GROUP BY Bucket ORDER BY Bucket;

        -- 4. Spec facets
        SELECT
            sf.""FacetKey"", sf.""NormalizedKey"",
            sf.""FacetValue"", sf.""NormalizedValue"",
            COUNT(*)::int AS Count
        FROM ""ProductSpecFacets"" sf
        JOIN ""Products"" p ON p.""ProductId"" = sf.""ProductId""
        WHERE p.search_vector @@ to_tsquery('english', @tsQuery)
          AND p.""IsActive"" = true AND p.""IsDeleted"" = false
        GROUP BY sf.""FacetKey"", sf.""NormalizedKey"",
                 sf.""FacetValue"", sf.""NormalizedValue""
        HAVING COUNT(*) >= 1
        ORDER BY sf.""NormalizedKey"", Count DESC;";

            using var multi = await conn.QueryMultipleAsync(combinedSql, p);

            var categories = await multi.ReadAsync<FacetItem>();
            var brands = await multi.ReadAsync<FacetItem>();
            var prices = await multi.ReadAsync<PriceBucket>();
            var specRows = await multi.ReadAsync<SpecFacetRow>();

            var specFacets = specRows
                .Where(r => !ExcludedSpecKeys.Contains(r.NormalizedKey))
                .GroupBy(r => new { r.FacetKey, r.NormalizedKey })
                .Select(g => new SpecFacetGroup
                {
                    FacetKey = g.Key.FacetKey,
                    NormalizedKey = g.Key.NormalizedKey,
                    Values = g.Select(r => new SpecFacetValue
                    {
                        Label = r.FacetValue,
                        NormalizedValue = r.NormalizedValue,
                        Count = r.Count
                    }).ToList()
                })
                .ToList();

            return new SearchFacets
            {
                Categories = categories.ToList(),
                Brands = brands.ToList(),
                PriceBuckets = prices.ToList(),
                SpecFacets = specFacets
            };
        }

        // ── Spec filter JOIN builder ───────────────────────────────────────────────
        // Each active spec filter becomes an INNER JOIN — acts as AND between filters
        // e.g. panel_type=ips AND refresh_rate=75 hz

        private static string BuildSpecJoins(Dictionary<string, string>? filters)
        {
            if (filters == null || filters.Count == 0)
                return "";

            var sb = new StringBuilder();
            int i = 0;

            foreach (var (key, value) in filters)
            {
                // Sanitize to prevent SQL injection (only allow safe chars)
                var safeKey = Regex.Replace(key, @"[^a-z0-9_]", "", RegexOptions.IgnoreCase);
                var safeValue = value.Replace("'", "''").ToLower();

                if (string.IsNullOrWhiteSpace(safeKey) || string.IsNullOrWhiteSpace(safeValue))
                    continue;

                sb.AppendLine($@"
            JOIN ""ProductSpecFacets"" sf{i}
                ON  sf{i}.""ProductId""       = p.""ProductId""
                AND sf{i}.""NormalizedKey""   = '{safeKey}'
                AND sf{i}.""NormalizedValue"" = '{safeValue}'");
                i++;
            }

            return sb.ToString();
        }

        // ── tsquery builder ───────────────────────────────────────────────────────
        // "lenovo monitor" → "lenovo:* & monitor:*"   (prefix match on every word)

        private static string BuildTsQuery(string input)
        {
            var words = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => Regex.Replace(w, @"[^a-zA-Z0-9]", ""))
                .Where(w => w.Length > 1)
                .Select(w => $"{w}:*");

            var joined = string.Join(" & ", words);

            // Fallback: if input had no valid words, use raw ILIKE only
            return string.IsNullOrEmpty(joined) ? "a:*" : joined;
        }

        public async Task<AutocompleteResponse> GetAutocompleteSuggestions(string prefix)
        {
            var db = _redis.GetDatabase();
            var cacheKey = $"autocomplete:{prefix.ToLower()}";

            var cachedData = await db.StringGetAsync(cacheKey);
            if (cachedData.HasValue)
                return JsonSerializer.Deserialize<AutocompleteResponse>(cachedData)!;

            var p = new { prefix = $"{prefix}%", tsQuery = $"{prefix}:*", limit = 10 };
            var conn = _context.Database.GetDbConnection();

            var products = await conn.QueryAsync<AutocompleteSuggestion>(@"
        SELECT
            p.""ProductId""::text  AS Id,
            p.""ProductName""      AS Label,
            'product'              AS Type,
            p.""ImageUrls""[1]     AS ImageUrl,
            p.""Price"",
            p.""Brand""
        FROM ""Products"" p
        WHERE p.""IsActive"" = true
          AND p.""IsDeleted"" = false
          AND (
            p.""ProductName"" ILIKE @prefix
            OR to_tsvector('english', p.""ProductName"") @@ to_tsquery('english', @tsQuery)
          )
        ORDER BY ts_rank(
            to_tsvector('english', p.""ProductName""),
            to_tsquery('english', @tsQuery)
        ) DESC
        LIMIT @limit", p); // ✅ p passed here

            var categories = await conn.QueryAsync<AutocompleteSuggestion>(@"
        SELECT
            c.""CategoryId""::text  AS Id,
            c.""Name""              AS Label,
            'category'              AS Type,
            c.""ImageUrl""          AS ImageUrl,
            NULL::numeric           AS Price,
            NULL::text              AS Brand
        FROM ""Categories"" c
        WHERE c.""IsActive"" = true
          AND c.""Name"" ILIKE @prefix
        ORDER BY c.""Name"" ASC
        LIMIT @limit", p); // ✅ p passed here

            var brands = await conn.QueryAsync<AutocompleteSuggestion>(@"
        SELECT DISTINCT
            p.""Brand""  AS Id,
            p.""Brand""  AS Label,
            'brand'      AS Type,
            NULL         AS ImageUrl,
            NULL         AS Price,
            p.""Brand""  AS Brand
        FROM ""Products"" p
        WHERE p.""IsActive"" = true
          AND p.""IsDeleted"" = false   -- ✅ = not ==
          AND p.""Brand"" ILIKE @prefix
          AND p.""Brand"" IS NOT NULL
          AND p.""Brand"" != ''
        ORDER BY p.""Brand"" ASC
        LIMIT @limit", p); // ✅ p passed here

            var popular = await db.SortedSetRangeByScoreAsync(PopularKey, order: StackExchange.Redis.Order.Descending);
            var popularMatches = popular
                .Select(x => x.ToString())
                .Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Take(3)
                .Select(x => new AutocompleteSuggestion { Id = x, Label = x, Type = "popular" });

            var response = new AutocompleteResponse
            {
                Products = products.ToList(),
                Categories = categories.ToList(),
                Brands = brands.ToList(),
                Popular = popularMatches.ToList()
            };

            await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(response), TimeSpan.FromMinutes(5));
            return response;
        }

        public async Task<IEnumerable<string>> GetSearchSuggestion(string queryName, int limit = 10)
        {
            try
            {
                var suggestions = new List<string>();

                // Get product name suggestions
                var productSuggestions = await _context.Products
                    .Where(p => p.ProductName.Contains(queryName) && p.IsActive && !p.IsDeleted)
                    .Select(p => p.ProductName)
                    .Distinct()
                    .Take(limit)
                    .ToListAsync();

                suggestions.AddRange(productSuggestions);

                // Get category suggestions if we need more
                if (suggestions.Count < limit)
                {
                    var categorySuggestions = await _context.Categories
                        .Where(c => c.Name.Contains(queryName) && c.IsActive)
                        .Select(c => c.Name)
                        .Distinct()
                        .Take(limit - suggestions.Count)
                        .ToListAsync();

                    suggestions.AddRange(categorySuggestions);
                }

                return suggestions.Take(limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search suggestions for query: {QueryName}", queryName);
                return Enumerable.Empty<string>();
            }
        }


        public async Task LogSearchAsync(SearchLog log)
        {
            try
            {
                await _context.Database.GetDbConnection().ExecuteAsync(@"
                INSERT INTO ""SearchLogs"" (""Query"", ""ResultCount"", ""UserId"", ""SessionId"", ""CreatedOn"")
                VALUES (@Query, @ResultCount, @UserId, @SessionId, @CreatedOn)", log);

                // Increment Redis popularity score so this query surfaces in autocomplete
                await IncrementPopularityAsync(log.Query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log search query '{Query}'", log.Query);
            }
        }

        public async Task LogClickAsync(ClickEvent evt)
        {
            try
            {
                await _context.Database.GetDbConnection().ExecuteAsync(@"
                INSERT INTO ""SearchLogs"" (""Query"", ""ClickedProductId"", ""SessionId"", ""CreatedOn"")
                VALUES (@Query, @ProductId, @SessionId, @CreatedOn)",
                    new { evt.Query, evt.ProductId, evt.SessionId, CreatedOn = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log click event for query '{Query}'", evt.Query);
            }
        }

        // ── Postgres queries ─────────────────────────────────────────────────────

        private async Task<AutocompleteResponse> QueryPostgresAsync(string prefix)
        {
            var conn = _context.Database.GetDbConnection();
            var tsQuery = BuildTsQuery(prefix);
            var p = new { prefix = $"{prefix}%", tsQuery, limit = 10};

            // Products — ranked by FTS then featured/stock
            var productsTask = conn.QueryAsync<AutocompleteSuggestion>(@"
            SELECT
                p.""ProductId""::text  AS Id,
                p.""ProductName""      AS Label,
                'product'              AS Type,
                p.""ImageUrls""[1]     AS ImageUrl,
                p.""Price"",
                p.""Brand""
            FROM ""Products"" p
            WHERE p.""IsActive""   = true
              AND p.""IsDeleted""  = false
              AND (
                    p.search_vector @@ to_tsquery('english', @tsQuery)
                    OR p.""ProductName"" ILIKE @prefix
                  )
            ORDER BY
                p.""IsFeatured""    DESC,
                p.""StockQuantity"" DESC,
                ts_rank(p.search_vector, to_tsquery('english', @tsQuery)) DESC
            LIMIT @limit", p);

            // Categories — active only, alphabetical
            var categoriesTask = conn.QueryAsync<AutocompleteSuggestion>(@"
            SELECT
                c.""CategoryId""::text AS Id,
                c.""Name""             AS Label,
                'category'             AS Type,
                c.""ImageUrl""         AS ImageUrl,
                NULL::numeric          AS Price,
                NULL::text             AS Brand
            FROM ""Categories"" c
            WHERE c.""IsActive"" = true
              AND c.""Name""     ILIKE @prefix
            ORDER BY c.""Name"" ASC
            LIMIT 3", p);

            // Brands — distinct, from active non-deleted products
            var brandsTask = conn.QueryAsync<AutocompleteSuggestion>(@"
            SELECT DISTINCT
                p.""Brand"" AS Id,
                p.""Brand"" AS Label,
                'brand'     AS Type,
                NULL        AS ImageUrl,
                NULL        AS Price,
                p.""Brand"" AS Brand
            FROM ""Products"" p
            WHERE p.""IsActive""  = true
              AND p.""IsDeleted"" = false
              AND p.""Brand""     ILIKE @prefix
              AND p.""Brand""     IS NOT NULL
              AND p.""Brand""     != ''
            ORDER BY p.""Brand"" ASC
            LIMIT 3", p);

            await Task.WhenAll(productsTask, categoriesTask, brandsTask);

            // Popular searches from Redis sorted set
            var popularMatches = await GetPopularMatchesAsync(prefix);

            return new AutocompleteResponse
            {
                Products = (await productsTask).ToList(),
                Categories = (await categoriesTask).ToList(),
                Brands = (await brandsTask).ToList(),
                Popular = popularMatches
            };
        }

        // ── Popularity ───────────────────────────────────────────────────────────

        public async Task IncrementPopularityAsync(string query)
        {
            query = query.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(query)) return;

            try
            {
                var db = _redis.GetDatabase();
                await db.SortedSetIncrementAsync(PopularKey, query, 1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to increment search popularity for query '{Query}'", query);
            }
        }

        private async Task<List<AutocompleteSuggestion>> GetPopularMatchesAsync(string prefix)
        {
            try
            {
                var db = _redis.GetDatabase();
                var popular = await db.SortedSetRangeByScoreAsync(
                    PopularKey,
                    order: StackExchange.Redis.Order.Descending,
                    take: 50); // get top 50 and filter client-side

                return popular
                    .Select(x => x.ToString())
                    .Where(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    .Take(3)
                    .Select(x => new AutocompleteSuggestion
                    {
                        Id = x,
                        Label = x,
                        Type = "popular"
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch popular searches from Redis");
                return new List<AutocompleteSuggestion>();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static string AutocompleteBuildTsQuery(string input)
        {
            var words = input
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => Regex.Replace(w, @"[^a-zA-Z0-9]", ""))
                .Where(w => w.Length > 1)
                .Select(w => $"{w}:*");

            var joined = string.Join(" & ", words);
            return string.IsNullOrEmpty(joined) ? $"{input}:*" : joined;
        }

        public async Task<IEnumerable<GetProductsDto>> SearchProductsAsync(string queryName)
        {
            try
            {
                var products = await _context.Products
                    .Where(p => (p.ProductName.Contains(queryName) || 
                                p.Description.Contains(queryName) ||
                                p.ProductDescription.Contains(queryName)) && 
                                p.IsActive && !p.IsDeleted)
                    .Include(p => p.Category)
                    .Include(p => p.Merchant)
                    .Take(50) // Limit results for performance
                    .ToListAsync();

                return products.Select(p => new GetProductsDto
                {
                    ProductId = p.ProductId.ToString(), // Convert Guid to string
                    ProductName = p.ProductName,
                    ProductDescription = p.ProductDescription,
                    Price = (double)p.Price, // Convert decimal to double  
                    Discount = (double)p.Discount,
                    ImageUrl = p.ImageUrls?.FirstOrDefault() ?? "",
                    CategoryName = p.CategoryName,
                    InStock = p.StockQuantity > 0,
                    StockQuantity = p.StockQuantity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products for query: {QueryName}", queryName);
                return Enumerable.Empty<GetProductsDto>();
            }
        }

        public async Task<IEnumerable<Models.Category>> GetSearchResults(string queryname)
        {
            try
            {
                return await _context.Categories
                    .Where(c => c.Name.Contains(queryname) && c.IsActive)
                    .Take(20)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search results for query: {QueryName}", queryname);
                return Enumerable.Empty<Models.Category>();
            }
        }

        public async Task<Status> UpdateColumnJson()
        {
            try
            {
                // Implementation for updating column JSON
                // This might be for updating search index or similar
                await Task.CompletedTask; // Placeholder

                return new Status
                {
                    ResponseCode = 200,
                    ResponseMessage = "Column JSON updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating column JSON");
                return new Status
                {
                    ResponseCode = 500,
                    ResponseMessage = "Error updating column JSON"
                };
            }
        }

        public async Task<IEnumerable<CartResults>> GetSearchProducts(int CategoryID)
        {
            try
            {
                // For legacy support with int CategoryID
                return await _context.Products
                    .Where(p => p.IsActive && !p.IsDeleted && p.StockQuantity > 0)
                    .Select(p => new CartResults
                    {
                        productID = p.ProductId,
                        ProductName = p.ProductName,
                        ProductImage = p.ImageUrls.FirstOrDefault() ?? "",
                        ProductDescription = p.ProductDescription,
                        price = p.Price,
                        InStock = p.StockQuantity > 0,
                        MerchantId = p.MerchantID
                    })
                    .Take(50)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting search products for category: {CategoryId}", CategoryID);
                return Enumerable.Empty<CartResults>();
            }
        }

        public async Task<PaginatedResult<Product>> GetFilteredProducts(ProductFilterParams filterParams)
        {
            try
            {
                var query = _context.Products
                    .Where(p => p.IsActive && !p.IsDeleted)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filterParams.SearchTerm))
                {
                    query = query.Where(p => p.ProductName.Contains(filterParams.SearchTerm) ||
                                           p.Description.Contains(filterParams.SearchTerm));
                }

                if (filterParams.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == filterParams.CategoryId);
                }

                if (filterParams.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= filterParams.MinPrice);
                }

                if (filterParams.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= filterParams.MaxPrice);
                }

                if (filterParams.InStock)
                {
                    query = query.Where(p => p.StockQuantity > 0);
                }

                // Apply sorting - using filterParams properties directly
                if (!string.IsNullOrEmpty(filterParams.SortBy))
                {
                    query = filterParams.SortBy.ToLower() switch
                    {
                        "price" => filterParams.SortOrder == "desc" 
                            ? query.OrderByDescending(p => p.Price) 
                            : query.OrderBy(p => p.Price),
                        "name" => filterParams.SortOrder == "desc" 
                            ? query.OrderByDescending(p => p.ProductName) 
                            : query.OrderBy(p => p.ProductName),
                        "date" => filterParams.SortOrder == "desc" 
                            ? query.OrderByDescending(p => p.CreatedOn) 
                            : query.OrderBy(p => p.CreatedOn),
                        _ => query.OrderByDescending(p => p.CreatedOn)
                    };
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination - using filterParams.PageNumber and filterParams.PageSize
                var products = await query
                    .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
                    .Take(filterParams.PageSize)
                    .ToListAsync();

                return new PaginatedResult<Product>
                {
                    Items = products,
                    TotalCount = totalCount,
                    PageNumber = filterParams.PageNumber,
                    PageSize = filterParams.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filterParams.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting filtered products");
                return new PaginatedResult<Product>
                {
                    Items = new List<Product>(),
                    TotalCount = 0,
                    PageNumber = filterParams.PageNumber,
                    PageSize = filterParams.PageSize,
                    TotalPages = 0
                };
            }
        }
    }
}
