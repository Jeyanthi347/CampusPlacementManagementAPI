using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Models
{
    public class PlacementOfficer
    {
        public int PlacementOfficerId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int CollegeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Designation { get; set; } = string.Empty;

        public PlacementOfficerStatus Status { get; set; }
            = PlacementOfficerStatus.Pending;

        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedAt { get; set; }

        // Navigation properties

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        [ForeignKey(nameof(CollegeId))]
        public College? College { get; set; }

        [ForeignKey(nameof(ApprovedBy))]
        public ApplicationUser? Approver { get; set; }

        public ICollection<Student> Students { get; set; }
            = new List<Student>();
    }
}