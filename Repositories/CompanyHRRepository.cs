using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Repositories
{
    public class CompanyHRRepository : ICompanyHRRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyHRRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CompanyHR>> GetAllAsync()
        {
            return await _context.CompanyHRs
                .Include(x => x.User)
                .Include(x => x.Company)
                .ToListAsync();
        }

        public async Task<List<CompanyHR>> GetPendingAsync()
        {
            return await _context.CompanyHRs
                .Include(x => x.User)
                .Include(x => x.Company)
                .Where(x => x.User != null &&
                            x.User.AccountStatus == AccountStatus.Pending)
                .ToListAsync();
        }

        public async Task<CompanyHR?> GetByIdAsync(int companyHRId)
        {
            return await _context.CompanyHRs
                .Include(x => x.User)
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.CompanyHRId == companyHRId);
        }

        public async Task<CompanyHR?> GetByUserIdAsync(string userId)
        {
            return await _context.CompanyHRs
                .Include(x => x.User)
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task AddAsync(CompanyHR companyHR)
        {
            await _context.CompanyHRs.AddAsync(companyHR);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CompanyHR companyHR)
        {
            _context.CompanyHRs.Update(companyHR);
            await _context.SaveChangesAsync();
        }
    }
}