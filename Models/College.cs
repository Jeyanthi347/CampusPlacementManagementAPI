
namespace CampusPlacementManagementAPI.Models
{
    public class College
    {
        public int CollegeId { get; set; }

        public string CollegeName { get; set; } = string.Empty;

        public string CollegeCode { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
