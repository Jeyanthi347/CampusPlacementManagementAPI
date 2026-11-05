using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Models
{
    public class College
    {
        public int CollegeId { get; set; }
        public string CollegeName { get; set; } = string.Empty;
        public string CollegeCode { get; set; } = string.Empty;
        public string? UniversityName { get; set; }
        public string Address { get; set; } = string.Empty;
        public string OfficialEmail { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Website { get; set; }
        public CollegeStatus Status { get; set; } = CollegeStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}