using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.CompanyHR
{
    public class CompanyHRRegisterDto
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public int CompanyId { get; set; }

        [MaxLength(100)]
        public string? Designation { get; set; }
    }
}