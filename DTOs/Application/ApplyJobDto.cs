using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.Application
{
    public class ApplyJobDto
    {
        [MaxLength(500)]
        public string? ResumeUrl { get; set; }
    }
}