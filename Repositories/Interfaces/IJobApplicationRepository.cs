using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface IJobApplicationRepository
    {
        Task<JobApplication?> GetByIdAsync(int applicationId);

        Task<List<JobApplication>> GetByStudentIdAsync(int studentId);

        Task<List<JobApplication>> GetByJobIdAsync(int jobId);

        Task<bool> HasStudentAppliedAsync(int studentId, int jobId);

        Task AddAsync(JobApplication application);

        Task UpdateAsync(JobApplication application);

        Task SaveChangesAsync();
    }
}