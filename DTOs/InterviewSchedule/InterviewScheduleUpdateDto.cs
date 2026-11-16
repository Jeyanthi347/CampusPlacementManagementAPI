
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.InterviewSchedule
{
    public class InterviewScheduleUpdateDto
    {
        [Required]
        public DateTime ScheduledAt { get; set; }

        [MaxLength(500)]
        public string? MeetingLink { get; set; }

        [MaxLength(255)]
        public string? Venue { get; set; }
    }
}
