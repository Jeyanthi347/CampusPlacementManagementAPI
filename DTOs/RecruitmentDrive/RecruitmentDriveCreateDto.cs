
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.RecruitmentDrive
{
    public class RecruitmentDriveCreateDto
    {
        [Range(1, int.MaxValue)]
        public int JobId { get; set; }

        [Range(1, int.MaxValue)]
        public int CollegeId { get; set; }

        [Required]
        public DateTime DriveDate { get; set; }

        [MaxLength(255)]
        public string? Venue { get; set; }

        public string? Description { get; set; }
    }
}