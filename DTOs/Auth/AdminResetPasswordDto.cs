
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.Auth
{
    public class AdminResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}
