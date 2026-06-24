using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.SubCategory
{
    public class UpdateSubCategoryDto
    {
        [MaxLength(255)]
        public string Name { get; set; }

        public string Description { get; set; }

        [MaxLength(255)]
        public string Slug { get; set; }

        public bool? IsActive { get; set; }

        public int? SortOrder { get; set; }

        public Guid? CategoryId { get; set; }

        [MaxLength(500)]
        [Url]
        public string ImageUrl { get; set; }
    }
}
