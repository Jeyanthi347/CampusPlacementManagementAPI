
using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        public string UserId { get; set; } = string.Empty;

        public int CollegeId { get; set; }

        public int PlacementOfficerId { get; set; }

        public string RegisterNumber { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Degree { get; set; } = string.Empty;

        public int GraduationYear { get; set; }

        public decimal CGPA { get; set; }

        public int ActiveBacklogs { get; set; }

        public string? ResumeUrl { get; set; }

        public StudentStatus Status { get; set; }
            = StudentStatus.Pending;

        public DateTime? ApprovedAt { get; set; }

        public ApplicationUser? User { get; set; }
    }
}
