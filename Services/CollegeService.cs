
using CampusPlacementManagementAPI.DTOs.College;
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

            return colleges.Select(c => MapToDto(c)).ToList();
        }

        public async Task<CollegeResponseDto?> GetCollegeByIdAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
                return null;

            return MapToDto(college);
        }

        public async Task<CollegeResponseDto> CreateCollegeAsync(
            CollegeCreateDto dto)
        {
            var collegeCode = dto.CollegeCode.Trim();
            var email = dto.Email.Trim();

            if (await _collegeRepository.ExistsByCodeAsync(collegeCode))
                throw new InvalidOperationException("College code already exists.");

            if (await _collegeRepository.ExistsByEmailAsync(email))
                throw new InvalidOperationException("College email already exists.");

            var college = new College
            {
                CollegeName = dto.CollegeName.Trim(),
                CollegeCode = collegeCode,
                Address = dto.Address.Trim(),
                City = dto.City.Trim(),
                State = dto.State.Trim(),
                Email = email,
                PhoneNumber = dto.PhoneNumber.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _collegeRepository.AddAsync(college);

            return MapToDto(college);
        }

        public async Task<CollegeResponseDto?> UpdateCollegeAsync(
            int collegeId,
            CollegeUpdateDto dto)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
                return null;

            var collegeCode = dto.CollegeCode.Trim();
            var email = dto.Email.Trim();

            var allColleges = await _collegeRepository.GetAllAsync();

            if (allColleges.Any(c =>
                c.CollegeId != collegeId &&
                c.CollegeCode.Equals(collegeCode, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("College code already exists.");
            }

            if (allColleges.Any(c =>
                c.CollegeId != collegeId &&
                c.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("College email already exists.");
            }

            college.CollegeName = dto.CollegeName.Trim();
            college.CollegeCode = collegeCode;
            college.Address = dto.Address.Trim();
            college.City = dto.City.Trim();
            college.State = dto.State.Trim();
            college.Email = email;

            college.PhoneNumber = dto.PhoneNumber.Trim();

            await _collegeRepository.UpdateAsync(college);

            return MapToDto(college);
        }

        public async Task<bool> DeleteCollegeAsync(int collegeId)
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);

            if (college == null)
                return false;

            await _collegeRepository.DeleteAsync(college);

            return true;
        }

        private static CollegeResponseDto MapToDto(College college)
        {
            return new CollegeResponseDto
            {
                CollegeId = college.CollegeId,
                CollegeName = college.CollegeName,
                CollegeCode = college.CollegeCode,
                Address = college.Address,
                City = college.City,
                State = college.State,
                Email = college.Email,
                PhoneNumber = college.PhoneNumber,
                CreatedAt = college.CreatedAt
            };
        }
    }
}
