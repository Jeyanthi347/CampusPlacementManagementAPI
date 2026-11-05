
using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface IJobRepository
    {
        Task<IEnumerable<JobOpening>> GetAllAsync();

        Task<IEnumerable<JobOpening>> GetByCompanyHRIdAsync(int companyHRId);

        Task<JobOpening?> GetByIdAsync(int jobId);

        Task AddAsync(JobOpening job);

        Task UpdateAsync(JobOpening job);

        Task DeleteAsync(JobOpening job);
    }
}
