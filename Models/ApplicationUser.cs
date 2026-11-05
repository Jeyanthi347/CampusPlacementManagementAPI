
using Microsoft.AspNetCore.Identity;
using CampusPlacementManagementAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(150)]
        public string FullName { get; set; } = string.Empty;

        public AccountStatus AccountStatus { get; set; }
            = AccountStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Student? Student { get; set; }

        public PlacementOfficer? PlacementOfficer { get; set; }
    }
}
