
using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.DTOs.College
{
    public class CollegeResponseDto
    {
        public int CollegeId { get; set; }

        public string CollegeName { get; set; } = string.Empty;

        public string CollegeCode { get; set; } = string.Empty;

        public string? UniversityName { get; set; }

        public string Address { get; set; } = string.Empty;

        public string OfficialEmail { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Website { get; set; }

        public CollegeStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
