
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Repositories
{
    public class InterviewRoundRepository : IInterviewRoundRepository
    {
        private readonly ApplicationDbContext _context;

        public InterviewRoundRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InterviewRound>> GetByJobIdAsync(int jobId)
        {
            return await _context.InterviewRounds
                .Include(x => x.JobOpening)
                .Where(x => x.JobId == jobId)
                .OrderBy(x => x.RoundNumber)
                .ToListAsync();
        }

        public async Task<InterviewRound?> GetByIdAsync(int interviewRoundId)
        {
            return await _context.InterviewRounds
                .Include(x => x.JobOpening)
                .FirstOrDefaultAsync(x =>
                    x.InterviewRoundId == interviewRoundId);
        }

        public async Task<bool> RoundNumberExistsAsync(int jobId, int roundNumber)
        {
            return await _context.InterviewRounds
                .AnyAsync(x =>
                    x.JobId == jobId &&
                    x.RoundNumber == roundNumber);
        }

        public async Task AddAsync(InterviewRound interviewRound)
        {
            await _context.InterviewRounds.AddAsync(interviewRound);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InterviewRound interviewRound)
        {
            _context.InterviewRounds.Update(interviewRound);
            await _context.SaveChangesAsync();
        }
    }
}
