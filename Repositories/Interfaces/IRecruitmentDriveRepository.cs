
using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface IRecruitmentDriveRepository
    {
        Task<IEnumerable<RecruitmentDrive>> GetAllAsync();

        Task<IEnumerable<RecruitmentDrive>> GetByCollegeIdAsync(int collegeId);

        Task<IEnumerable<RecruitmentDrive>> GetByCompanyIdAsync(int companyId);

        Task<RecruitmentDrive?> GetByIdAsync(int driveId);

        Task AddAsync(RecruitmentDrive drive);

        Task UpdateAsync(RecruitmentDrive drive);
    }
}
