using System.ComponentModel.DataAnnotations;

namespace Minimart_Api.DTOS.Authorization
{
    public class ChangePasswordDto
    {
        [Required]
        [MinLength(6)]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmNewPassword { get; set; }
    }
}