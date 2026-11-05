using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.Application
{
    public class JobApplicationCreateDto
    {
        [Required]
        public int JobId { get; set; }

        [MaxLength(500)]
        public string? ResumeUrl { get; set; }
    }
}