
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JobOpening>> GetAllAsync()
        {
            return await _context.JobOpenings
                .Include(j => j.Company)
                .Include(j => j.CreatedByHR)
                    .ThenInclude(hr => hr!.User)
                .Include(j => j.TargetColleges)
                    .ThenInclude(tc => tc.College)
                .ToListAsync();
        }

        public async Task<IEnumerable<JobOpening>> GetByCompanyHRIdAsync(
            int companyHRId)
        {
            return await _context.JobOpenings
                .Where(j => j.CreatedByHRId == companyHRId)
                .Include(j => j.Company)
                .Include(j => j.CreatedByHR)
                    .ThenInclude(hr => hr!.User)
                .Include(j => j.TargetColleges)
                    .ThenInclude(tc => tc.College)
                .ToListAsync();
        }

        public async Task<JobOpening?> GetByIdAsync(int jobId)
        {
            return await _context.JobOpenings
                .Include(j => j.Company)
                .Include(j => j.CreatedByHR)
                    .ThenInclude(hr => hr!.User)
                .Include(j => j.TargetColleges)
                    .ThenInclude(tc => tc.College)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task AddAsync(JobOpening job)
        {
            await _context.JobOpenings.AddAsync(job);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(JobOpening job)
        {
            _context.JobOpenings.Update(job);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobOpening job)
        {
            _context.JobOpenings.Remove(job);
            await _context.SaveChangesAsync();
        }
    }
}
