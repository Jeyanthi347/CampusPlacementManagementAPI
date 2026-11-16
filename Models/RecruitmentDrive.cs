
using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Models
{
    public class RecruitmentDrive
    {
        public int DriveId { get; set; }

        public int JobId { get; set; }

        public int CollegeId { get; set; }

        public DateTime DriveDate { get; set; }

        public string? Venue { get; set; }

        public string? Description { get; set; }

        public RecruitmentDriveStatus Status { get; set; }
            = RecruitmentDriveStatus.Scheduled;

        // Navigation properties
        public JobOpening? JobOpening { get; set; }

        public College? College { get; set; }
    }
}
