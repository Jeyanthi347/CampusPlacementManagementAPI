
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Repositories
{
    public class CollegeRepository : ICollegeRepository
    {
        private readonly ApplicationDbContext _context;

        public CollegeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<College>> GetAllAsync()
        {
            return await _context.Colleges
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<College?> GetByIdAsync(int collegeId)
        {
            return await _context.Colleges
                .FirstOrDefaultAsync(c => c.CollegeId == collegeId);
        }

        public async Task<bool> ExistsByCodeAsync(string collegeCode)
        {
            return await _context.Colleges
                .AnyAsync(c => c.CollegeCode == collegeCode);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Colleges
                .AnyAsync(c => c.Email == email);
        }

        public async Task AddAsync(College college)
        {
            await _context.Colleges.AddAsync(college);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(College college)
        {
            _context.Colleges.Update(college);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(College college)
        {
            _context.Colleges.Remove(college);
            await _context.SaveChangesAsync();
        }
    }
}
