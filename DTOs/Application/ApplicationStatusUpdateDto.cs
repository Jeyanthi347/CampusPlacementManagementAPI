using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.Application
{
    public class ApplicationStatusUpdateDto
    {
        [MaxLength(500)]
        public string? Remarks { get; set; }
    }
}