using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minimart_Api.Models
{
    /// <summary>
    /// Tracks old product slugs for 301 redirects when product names change
    /// </summary>
    [Table("SlugRedirects")]
    public class SlugRedirect
    {
        [Key]
        [Column("RedirectId")]
        public Guid RedirectId { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(300)]
        [Column("OldSlug")]
        public string OldSlug { get; set; } = string.Empty;

        [Required]
        [StringLength(300)]
        [Column("NewSlug")]
        public string NewSlug { get; set; } = string.Empty;

        [Required]
        [Column("ProductId")]
        public Guid ProductId { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("IsActive")]
        public bool IsActive { get; set; } = true;

        // Navigation Property
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}