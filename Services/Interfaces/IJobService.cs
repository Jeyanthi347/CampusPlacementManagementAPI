
using CampusPlacementManagementAPI.DTOs.Job;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface IJobService
    {
        Task<JobResponseDto> CreateJobAsync(
            JobCreateDto dto, string userId);

        Task<IEnumerable<JobResponseDto>> GetAllJobsAsync(
            string userId, string role);

        Task<JobResponseDto?> GetJobByIdAsync(
            int jobId, string userId, string role);

        Task<JobResponseDto> UpdateJobAsync(
            int jobId, JobUpdateDto dto, string userId);

        Task<JobResponseDto> PublishJobAsync(
            int jobId, string userId);

        Task<JobResponseDto> CloseJobAsync(
            int jobId, string userId);

        Task DeleteJobAsync(int jobId, string userId);

        Task<IEnumerable<JobResponseDto>> GetEligibleJobsAsync(
            string userId);

        Task<JobEligibilityResponseDto> CheckEligibilityAsync(
            int jobId, string userId);
    }
}
