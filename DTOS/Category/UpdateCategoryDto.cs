using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Category
{
    public class UpdateCategoryDto
    {
        [MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string Slug { get; set; }

        public bool? IsActive { get; set; }

        public int? SortOrder { get; set; }

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
