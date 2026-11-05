
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.Student
{
    public class StudentEnrollDto
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RegisterNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Degree { get; set; } = string.Empty;

        [Range(2000, 2100)]
        public int GraduationYear { get; set; }

        [Range(typeof(decimal), "0", "10")]
        public decimal CGPA { get; set; }

        [Range(0, int.MaxValue)]
        public int ActiveBacklogs { get; set; }

        public string? ResumeUrl { get; set; }

        [Required]
        [MinLength(8)]
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}
