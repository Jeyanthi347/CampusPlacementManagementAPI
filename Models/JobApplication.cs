using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Models
{
    public class JobApplication
    {
        public int ApplicationId { get; set; }

        public int JobId { get; set; }

        public int StudentId { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public ApplicationStatus Status { get; set; }
            = ApplicationStatus.Applied;

        public string? ResumeUrl { get; set; }

        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        public JobOpening? Job { get; set; }

        public Student? Student { get; set; }

        public ICollection<ApplicationStatusHistory> StatusHistory
        { get; set; } = new List<ApplicationStatusHistory>();
    }
}