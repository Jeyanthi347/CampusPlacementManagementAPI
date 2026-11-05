
namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface IPlacementOfficerService
    {
        Task<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto>
            RegisterAsync(
                CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerRegisterDto dto);

        Task<List<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto>>
            GetAllAsync();

        Task<List<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto>>
            GetPendingAsync();

        Task<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto?>
            GetByIdAsync(int id);

        Task<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto?>
            GetByUserIdAsync(string userId);

        Task<bool> ApproveAsync(int id, string approvedBy);

        Task<bool> RejectAsync(int id);

        Task<bool> SuspendAsync(int id);
    }
}
