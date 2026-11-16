
using System.ComponentModel.DataAnnotations;

namespace CampusPlacementManagementAPI.DTOs.InterviewSchedule
{
    public class InterviewScheduleCreateDto
    {
        [Required]
        public int InterviewRoundId { get; set; }

        [Required]
        public DateTime ScheduledAt { get; set; }

        [MaxLength(500)]
        public string? MeetingLink { get; set; }

        [MaxLength(255)]
        public string? Venue { get; set; }
    }
}
