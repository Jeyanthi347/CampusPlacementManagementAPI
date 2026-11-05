
using CampusPlacementManagementAPI.DTOs.College;
using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface ICollegeService
    {
        Task<List<CollegeResponseDto>> GetAllCollegesAsync();

        Task<CollegeResponseDto?> GetCollegeByIdAsync(int collegeId);

        Task<CollegeResponseDto> CreateCollegeAsync(CollegeCreateDto dto);

        Task<CollegeResponseDto?> UpdateCollegeAsync(
            int collegeId,
            CollegeUpdateDto dto);

        Task<bool> UpdateCollegeStatusAsync(
            int collegeId,
            CollegeStatus status);
    }
}
