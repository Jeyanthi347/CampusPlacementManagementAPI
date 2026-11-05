
using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface IPlacementOfficerRepository
    {
        Task<List<PlacementOfficer>> GetAllAsync();

        Task<List<PlacementOfficer>> GetPendingAsync();

        Task<PlacementOfficer?> GetByIdAsync(int id);

        Task<PlacementOfficer?> GetByUserIdAsync(string userId);

        Task AddAsync(PlacementOfficer placementOfficer);

        Task UpdateAsync(PlacementOfficer placementOfficer);
    }
}
