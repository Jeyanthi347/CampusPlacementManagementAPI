
using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Models
{
    public class ApplicationStatusHistory
    {
        public int HistoryId { get; set; }

        public int ApplicationId { get; set; }

        public ApplicationStatus? PreviousStatus { get; set; }

        public ApplicationStatus NewStatus { get; set; }

        public string UpdatedByUserId { get; set; } = string.Empty;

        public string? Remarks { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public JobApplication? Application { get; set; }

        public ApplicationUser? UpdatedByUser { get; set; }
    }
}
