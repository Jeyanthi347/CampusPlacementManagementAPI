
using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Models
{
    public class Company
    {
        public int CompanyId { get; set; }

        public string CompanyName { get; set; } = string.Empty;

        public string CompanyEmail { get; set; } = string.Empty;

        public string? Website { get; set; }

        public string Industry { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string? Description { get; set; }

        public CompanyStatus Status { get; set; } = CompanyStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
