using System;
using System.Text.RegularExpressions;

namespace Minimart_Api.Services.SlugService
{
    public interface ISlugService
    {
        string GenerateSlug(string productName, Guid productId);
        Guid? ExtractIdFromSlug(string slug);
        bool IsValidSlug(string slug);
    }

    public class SlugService : ISlugService
    {
        /// <summary>
        /// Generates SEO-friendly slug from product name and ID
        /// Example: "Getac K120 Core i5" + "6949aa56-..." → "getac-k120-core-i5-6949aa56"
        /// </summary>
        public string GenerateSlug(string productName, Guid productId)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new ArgumentException("Product name cannot be empty", nameof(productName));

            // Get last 8 characters of GUID (short ID)
            string shortId = productId.ToString().Substring(24, 8);

            // Normalize product name
            string slug = productName.ToLowerInvariant();

            // Remove special characters, keep only alphanumeric and hyphens
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");

            // Replace multiple spaces/hyphens with single hyphen
            slug = Regex.Replace(slug, @"[\s-]+", "-");

            // Trim hyphens from start/end
            slug = slug.Trim('-');

            // Append short ID
            slug = $"{slug}-{shortId}";

            // Ensure max length
            if (slug.Length > 300)
                slug = slug.Substring(0, 291) + "-" + shortId;

            return slug;
        }

        /// <summary>
        /// Extracts product ID from slug
        /// Example: "getac-k120-core-i5-6949aa56" → "6949aa56"
        /// </summary>
        public Guid? ExtractIdFromSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return null;

            // Get last 8 characters (short ID)
            var parts = slug.Split('-');
            if (parts.Length < 2)
                return null;

            string shortId = parts[^1]; // Last part

            if (shortId.Length != 8)
                return null;

            return null; // We'll use slug directly for lookup in database
        }

        /// <summary>
        /// Validates slug format
        /// </summary>
        public bool IsValidSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            // Must match pattern: lowercase-words-8chars
            return Regex.IsMatch(slug, @"^[a-z0-9]+(-[a-z0-9]+)*-[a-z0-9]{8}$");
        }
    }
}