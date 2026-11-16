
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.RecruitmentDrive
{
    public class RecruitmentDriveUpdateDto
    {
        [Required]
        public DateTime DriveDate { get; set; }

        [MaxLength(255)]
        public string? Venue { get; set; }

        public string? Description { get; set; }
    }
}
