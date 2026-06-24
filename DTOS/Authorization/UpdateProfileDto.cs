using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Authorization
{
    public class UpdateProfileDto
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [MaxLength(100)]
        public string? DisplayName { get; set; }

        [Phone]
        [MaxLength(15)]
        public string? PhoneNumber { get; set; }
    }
}