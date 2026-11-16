
using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.DTOs.InterviewSchedule
{
    public class InterviewScheduleResponseDto
    {
        public int InterviewScheduleId { get; set; }

        public int ApplicationId { get; set; }

        public int InterviewRoundId { get; set; }

        public string RoundName { get; set; } = string.Empty;

        public DateTime ScheduledAt { get; set; }

        public string? MeetingLink { get; set; }

        public string? Venue { get; set; }

        public InterviewScheduleStatus Status { get; set; }

        public string? Feedback { get; set; }
    }
}
