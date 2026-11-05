
using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetAllAsync();

        Task<List<Company>> GetPendingAsync();

        Task<Company?> GetByIdAsync(int companyId);

        Task<Company?> GetByEmailAsync(string email);

        Task<bool> ExistsByEmailAsync(string email);

        Task AddAsync(Company company);

        Task UpdateAsync(Company company);
    }
}

