
using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface IInterviewRoundRepository
    {
        Task<IEnumerable<InterviewRound>> GetByJobIdAsync(int jobId);

        Task<InterviewRound?> GetByIdAsync(int interviewRoundId);

        Task<bool> RoundNumberExistsAsync(int jobId, int roundNumber);

        Task AddAsync(InterviewRound interviewRound);

        Task UpdateAsync(InterviewRound interviewRound);
    }
}
