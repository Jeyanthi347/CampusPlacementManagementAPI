
using CampusPlacementManagementAPI.DTOs.RecruitmentDrive;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface IRecruitmentDriveService
    {
        Task<RecruitmentDriveResponseDto> CreateDriveAsync(
            RecruitmentDriveCreateDto dto,
            string userId,
            string role);

        Task<IEnumerable<RecruitmentDriveResponseDto>> GetAllDrivesAsync(
            string userId, string role);

        Task<RecruitmentDriveResponseDto?> GetDriveByIdAsync(
            int driveId, string userId, string role);

        Task<RecruitmentDriveResponseDto> UpdateDriveAsync(
            int driveId,
            RecruitmentDriveUpdateDto dto,
            string userId,
            string role);

        Task<RecruitmentDriveResponseDto> CancelDriveAsync(
            int driveId,
            string userId,
            string role);
    }
}
