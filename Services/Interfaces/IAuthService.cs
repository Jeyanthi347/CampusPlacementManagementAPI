
using CampusPlacementManagementAPI.DTOs.Auth;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);

        Task<bool> ChangePasswordAsync(
            string userId,
            ChangePasswordDto dto);
    }
}
