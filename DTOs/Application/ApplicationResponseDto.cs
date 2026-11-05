using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.DTOs.Application
{
    public class ApplicationResponseDto
    {
        public int ApplicationId { get; set; }

        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StudentEmail { get; set; } = string.Empty;

        public DateTime AppliedAt { get; set; }

        public ApplicationStatus Status { get; set; }

        public string? ResumeUrl { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}