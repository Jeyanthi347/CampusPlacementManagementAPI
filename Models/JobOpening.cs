using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Models
{
    public class JobOpening
    {
        public int JobId { get; set; }

        public int CompanyId { get; set; }

        public int CreatedByHRId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string JobDescription { get; set; } = string.Empty;

        public string JobType { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public decimal? SalaryPackage { get; set; }

        public decimal? MinimumCGPA { get; set; }

        public int? MaximumBacklogs { get; set; }

        public DateTime ApplicationDeadline { get; set; }

        public bool IsOpenToAllColleges { get; set; }

        public JobStatus Status { get; set; } = JobStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Company? Company { get; set; }

        public CompanyHR? CreatedByHR { get; set; }

        public ICollection<JobTargetCollege> TargetColleges { get; set; }
            = new List<JobTargetCollege>();
    }
}