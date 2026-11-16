
using CampusPlacementManagementAPI.DTOs.InterviewSchedule;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface IInterviewScheduleService
    {
        Task<InterviewScheduleResponseDto> CreateInterviewScheduleAsync(
            int applicationId,
            InterviewScheduleCreateDto dto,
            string userId);

        Task<IEnumerable<InterviewScheduleResponseDto>>
            GetInterviewSchedulesByApplicationAsync(
                int applicationId,
                string userId,
                string role);

        Task<InterviewScheduleResponseDto> UpdateInterviewScheduleAsync(
            int interviewScheduleId,
            InterviewScheduleUpdateDto dto,
            string userId);

        Task<InterviewScheduleResponseDto> UpdateInterviewFeedbackAsync(
            int interviewScheduleId,
            string feedback,
            string userId);
    }
}
