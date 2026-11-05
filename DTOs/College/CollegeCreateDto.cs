
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.College
{
    public class CollegeCreateDto
    {
        [Required]
        [StringLength(150)]
        public string CollegeName { get; set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string CollegeCode { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
