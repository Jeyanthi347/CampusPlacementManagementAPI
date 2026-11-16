
using CampusPlacementManagementAPI.DTOs.InterviewRound;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface IInterviewRoundService
    {
        Task<InterviewRoundResponseDto> CreateInterviewRoundAsync(
            int jobId,
            InterviewRoundCreateDto dto,
            string userId);

        Task<IEnumerable<InterviewRoundResponseDto>> GetInterviewRoundsByJobAsync(
            int jobId,
            string userId,
            string role);

        Task<InterviewRoundResponseDto> UpdateInterviewRoundAsync(
            int interviewRoundId,
            InterviewRoundUpdateDto dto,
            string userId);
    }
}
