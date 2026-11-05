
using CampusPlacementManagementAPI.DTOs.College;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;

namespace CampusPlacementManagementAPI.Services
{
    public class CollegeService : ICollegeService
    {
        private readonly ICollegeRepository _collegeRepository;

        public CollegeService(ICollegeRepository collegeRepository)
        {
            _collegeRepository = collegeRepository;
        }

        public async Task<List<CollegeResponseDto>> GetAllCollegesAsync()
        {
            var colleges = await _collegeRepository.GetAllAsync();

            return colleges.Select(MapToResponse).ToList();
        }

        public async Task<CollegeResponseDto?> GetCollegeByIdAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            return college == null ? null : MapToResponse(college);
        }

        public async Task<CollegeResponseDto> CreateCollegeAsync(
            CollegeCreateDto dto)
        {
            string collegeCode = dto.CollegeCode.Trim();
            string officialEmail = dto.OfficialEmail.Trim();

            if (await _collegeRepository.ExistsByCodeAsync(collegeCode))
            {
                throw new InvalidOperationException(
                    "College code already exists.");
            }

            if (await _collegeRepository.ExistsByEmailAsync(officialEmail))
            {
                throw new InvalidOperationException(
                    "College official email already exists.");
            }

            var college = new College
            {
                CollegeName = dto.CollegeName.Trim(),
                CollegeCode = collegeCode,
                UniversityName = dto.UniversityName?.Trim(),
                Address = dto.Address.Trim(),
                OfficialEmail = officialEmail,
                PhoneNumber = dto.PhoneNumber.Trim(),
                Website = dto.Website?.Trim(),
                Status = CollegeStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _collegeRepository.AddAsync(college);

            return MapToResponse(college);
        }

        public async Task<CollegeResponseDto?> UpdateCollegeAsync(
            int collegeId,
            CollegeUpdateDto dto)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
            {
                return null;
            }

            string collegeCode = dto.CollegeCode.Trim();
            string officialEmail = dto.OfficialEmail.Trim();

            if (college.CollegeCode != collegeCode &&
                await _collegeRepository.ExistsByCodeAsync(collegeCode))
            {
                throw new InvalidOperationException(
                    "College code already exists.");
            }

            if (college.OfficialEmail != officialEmail &&
                await _collegeRepository.ExistsByEmailAsync(officialEmail))
            {
                throw new InvalidOperationException(
                    "College official email already exists.");
            }

            college.CollegeName = dto.CollegeName.Trim();
            college.CollegeCode = collegeCode;
            college.UniversityName = dto.UniversityName?.Trim();
            college.Address = dto.Address.Trim();
            college.OfficialEmail = officialEmail;
            college.PhoneNumber = dto.PhoneNumber.Trim();
            college.Website = dto.Website?.Trim();

            await _collegeRepository.UpdateAsync(college);

            return MapToResponse(college);
        }

        public async Task<bool> UpdateCollegeStatusAsync(
            int collegeId,
            CollegeStatus status)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
            {
                return false;
            }

            college.Status = status;

            await _collegeRepository.UpdateAsync(college);

            return true;
        }

        private static CollegeResponseDto MapToResponse(College college)
        {
            return new CollegeResponseDto
            {
                CollegeId = college.CollegeId,
                CollegeName = college.CollegeName,
                CollegeCode = college.CollegeCode,
                UniversityName = college.UniversityName,
                Address = college.Address,
                OfficialEmail = college.OfficialEmail,
                PhoneNumber = college.PhoneNumber,
                Website = college.Website,
                Status = college.Status,
                CreatedAt = college.CreatedAt
            };
        }
    }
}
