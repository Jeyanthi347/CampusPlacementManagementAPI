
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllAsync()
        {
            return await _context.Companies
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Company>> GetPendingAsync()
        {
            return await _context.Companies
                .Where(c => c.Status == CompanyStatus.Pending)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Company?> GetByIdAsync(int companyId)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);
        }

        public async Task<Company?> GetByEmailAsync(string email)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyEmail == email);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Companies
                .AnyAsync(c => c.CompanyEmail == email);
        }

        public async Task AddAsync(Company company)
        {
            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Company company)
        {
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
        }
    }
}
