
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Repositories
{
    public class RecruitmentDriveRepository : IRecruitmentDriveRepository
    {
        private readonly ApplicationDbContext _context;

        public RecruitmentDriveRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RecruitmentDrive>> GetAllAsync()
        {
            return await _context.RecruitmentDrives
                .Include(d => d.JobOpening)
                    .ThenInclude(j => j!.Company)
                .Include(d => d.College)
                .ToListAsync();
        }

        public async Task<IEnumerable<RecruitmentDrive>> GetByCollegeIdAsync(
            int collegeId)
        {
            return await _context.RecruitmentDrives
                .Where(d => d.CollegeId == collegeId)
                .Include(d => d.JobOpening)
                    .ThenInclude(j => j!.Company)
                .Include(d => d.College)
                .ToListAsync();
        }

        public async Task<IEnumerable<RecruitmentDrive>> GetByCompanyIdAsync(
            int companyId)
        {
            return await _context.RecruitmentDrives
                .Where(d => d.JobOpening != null &&
                            d.JobOpening.CompanyId == companyId)
                .Include(d => d.JobOpening)
                    .ThenInclude(j => j!.Company)
                .Include(d => d.College)
                .ToListAsync();
        }

        public async Task<RecruitmentDrive?> GetByIdAsync(int driveId)
        {
            return await _context.RecruitmentDrives
                .Include(d => d.JobOpening)
                    .ThenInclude(j => j!.Company)
                .Include(d => d.College)
                .FirstOrDefaultAsync(d => d.DriveId == driveId);
        }

        public async Task AddAsync(RecruitmentDrive drive)
        {
            await _context.RecruitmentDrives.AddAsync(drive);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(RecruitmentDrive drive)
        {
            _context.RecruitmentDrives.Update(drive);
            await _context.SaveChangesAsync();
        }
    }
}
