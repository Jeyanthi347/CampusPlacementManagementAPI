using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.Job
{
    public class JobCreateDto
    {
        [Required]
        [MaxLength(200)]
        public string JobTitle { get; set; } = string.Empty;

        [Required]
        public string JobDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string JobType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [Range(0, 999999999999)]
        public decimal? SalaryPackage { get; set; }

        [Range(0, 10)]
        public decimal? MinimumCGPA { get; set; }

        [Range(0, 100)]
        public int? MaximumBacklogs { get; set; }

        [Required]
        public DateTime ApplicationDeadline { get; set; }

        public bool IsOpenToAllColleges { get; set; }

        public List<int> TargetCollegeIds { get; set; } = new();
    }
}