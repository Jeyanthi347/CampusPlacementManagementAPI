using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface ICompanyHRRepository
    {
        Task<List<CompanyHR>> GetAllAsync();

        Task<List<CompanyHR>> GetPendingAsync();

        Task<CompanyHR?> GetByIdAsync(int companyHRId);

        Task<CompanyHR?> GetByUserIdAsync(string userId);

        Task AddAsync(CompanyHR companyHR);

        Task UpdateAsync(CompanyHR companyHR);
    }
}