
using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface ICollegeRepository
    {
        Task<List<College>> GetAllAsync();

        Task<College?> GetByIdAsync(int collegeId);

        Task<bool> ExistsByCodeAsync(string collegeCode);

        Task<bool> ExistsByEmailAsync(string officialEmail);

        Task AddAsync(College college);

        Task UpdateAsync(College college);

        Task DeleteAsync(College college);
    }
}
