
using CampusPlacementManagementAPI.DTOs.Auth;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);

        Task<bool> ChangePasswordAsync(
            string userId,
            ChangePasswordDto dto);

        Task<IdentityResult?> ResetUserPasswordAsync(
            AdminResetPasswordDto dto);

        Task<IdentityResult?> AssignStudentRoleAsync(string email);
    }
}
