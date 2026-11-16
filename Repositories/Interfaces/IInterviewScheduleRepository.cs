
using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface IInterviewScheduleRepository
    {
        Task<IEnumerable<InterviewSchedule>> GetByApplicationIdAsync(
            int applicationId);

        Task<InterviewSchedule?> GetByIdAsync(
            int interviewScheduleId);

        Task<bool> HasScheduleConflictAsync(
            int applicationId,
            int interviewRoundId);

        Task AddAsync(InterviewSchedule interviewSchedule);

        Task UpdateAsync(InterviewSchedule interviewSchedule);
    }
}
