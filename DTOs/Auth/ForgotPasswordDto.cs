
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.Auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
