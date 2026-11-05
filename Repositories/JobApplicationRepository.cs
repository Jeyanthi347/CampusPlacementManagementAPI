using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Repositories
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<JobApplication?> GetByIdAsync(int applicationId)
        {
            return await _context.JobApplications
                .Include(x => x.Job)
                    .ThenInclude(x => x!.Company)
                .Include(x => x.Student)
                    .ThenInclude(x => x!.User)
                .Include(x => x.StatusHistory)
                .FirstOrDefaultAsync(x => x.ApplicationId == applicationId);
        }

        public async Task<List<JobApplication>> GetByStudentIdAsync(int studentId)
        {
            return await _context.JobApplications
                .Include(x => x.Job)
                    .ThenInclude(x => x!.Company)
                .Include(x => x.Student)
                    .ThenInclude(x => x!.User)
                .Where(x => x.StudentId == studentId)
                .OrderByDescending(x => x.AppliedAt)
                .ToListAsync();
        }

        public async Task<List<JobApplication>> GetByJobIdAsync(int jobId)
        {
            return await _context.JobApplications
                .Include(x => x.Job)
                    .ThenInclude(x => x!.Company)
                .Include(x => x.Student)
                    .ThenInclude(x => x!.User)
                .Where(x => x.JobId == jobId)
                .OrderByDescending(x => x.AppliedAt)
                .ToListAsync();
        }

        public async Task<bool> HasStudentAppliedAsync(int studentId, int jobId)
        {
            return await _context.JobApplications
                .AnyAsync(x =>
                    x.StudentId == studentId &&
                    x.JobId == jobId);
        }

        public async Task AddAsync(JobApplication application)
        {
            await _context.JobApplications.AddAsync(application);
        }

        public Task UpdateAsync(JobApplication application)
        {
            _context.JobApplications.Update(application);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}