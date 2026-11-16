
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Repositories
{
    public class InterviewScheduleRepository : IInterviewScheduleRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewScheduleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InterviewSchedule>>
            GetByApplicationIdAsync(int applicationId)
        {
            return await _context.InterviewSchedules
                .Include(x => x.InterviewRound)
                .Where(x => x.ApplicationId == applicationId)
                .OrderBy(x => x.ScheduledAt)
                .ToListAsync();
        }

        public async Task<InterviewSchedule?> GetByIdAsync(
            int interviewScheduleId)
        {
            return await _context.InterviewSchedules
                .Include(x => x.InterviewRound)
                .Include(x => x.JobApplication)
                    .ThenInclude(x => x!.Job)
                .FirstOrDefaultAsync(x =>
                    x.InterviewScheduleId == interviewScheduleId);
        }

        public async Task<bool> HasScheduleConflictAsync(
            int applicationId,
            int interviewRoundId)
        {
            return await _context.InterviewSchedules
                .AnyAsync(x =>
                    x.ApplicationId == applicationId &&
                    x.InterviewRoundId == interviewRoundId);
        }

        public async Task AddAsync(InterviewSchedule interviewSchedule)
        {
            await _context.InterviewSchedules.AddAsync(interviewSchedule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InterviewSchedule interviewSchedule)
        {
            _context.InterviewSchedules.Update(interviewSchedule);
            await _context.SaveChangesAsync();
        }
    }
}
