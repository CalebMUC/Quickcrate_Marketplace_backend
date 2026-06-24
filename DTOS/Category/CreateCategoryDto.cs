using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Category
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string Slug { get; set; }

        public bool IsActive { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        public Guid? ParentId { get; set; }

        [MaxLength(500)]
        [Url]
        public string ImageUrl { get; set; }

        [MaxLength(255)]
        public string MetaTitle { get; set; }

        [MaxLength(500)]
        public string MetaDescription { get; set; }
    }
}
