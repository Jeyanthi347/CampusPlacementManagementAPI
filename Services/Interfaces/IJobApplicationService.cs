using CampusPlacementManagementAPI.DTOs.Application;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface IJobApplicationService
    {
        Task<JobApplicationResponseDto> ApplyForJobAsync(
            string userId,
            JobApplicationCreateDto dto);

        Task<List<JobApplicationResponseDto>> GetMyApplicationsAsync(
            string userId);

        Task<JobApplicationResponseDto?> GetApplicationByIdAsync(
            int applicationId,
            string userId,
            string userRole);

        Task<List<JobApplicationResponseDto>> GetJobApplicationsAsync(
            int jobId,
            string userId);

        Task UpdateApplicationStatusAsync(
            int applicationId,
            string userId,
            ApplicationStatusUpdateDto dto,
            string newStatus);

        Task WithdrawApplicationAsync(
            int applicationId,
            string userId);
    }
}