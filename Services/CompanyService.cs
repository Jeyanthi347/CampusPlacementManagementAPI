
using CampusPlacementManagementAPI.DTOs.Company;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;

namespace CampusPlacementManagementAPI.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        public async Task<CompanyResponseDto> RegisterCompanyAsync(
            CompanyCreateDto dto)
        {
            string email = dto.CompanyEmail.Trim().ToLower();

            bool exists = await _companyRepository.ExistsByEmailAsync(email);

            if (exists)
            {
                throw new InvalidOperationException(
                    "Company email already exists.");
            }

            var company = new Company
            {
                CompanyName = dto.CompanyName.Trim(),
                CompanyEmail = email,
                Website = dto.Website?.Trim(),
                Industry = dto.Industry.Trim(),
                Address = dto.Address.Trim(),
                Description = dto.Description?.Trim(),
                Status = CompanyStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _companyRepository.AddAsync(company);

            return MapToResponse(company);
        }

        public async Task<List<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            var companies = await _companyRepository.GetAllAsync();

            return companies.Select(MapToResponse).ToList();
        }

        public async Task<List<CompanyResponseDto>> GetPendingCompaniesAsync()
        {
            var companies = await _companyRepository.GetPendingAsync();

            return companies.Select(MapToResponse).ToList();
        }

        public async Task<CompanyResponseDto?> GetCompanyByIdAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            return company == null ? null : MapToResponse(company);
        }

        public async Task<CompanyResponseDto> UpdateCompanyAsync(
            int companyId, CompanyUpdateDto dto)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null)
            {
                throw new KeyNotFoundException("Company not found.");
            }

            string email = dto.CompanyEmail.Trim().ToLower();

            var existingCompany =
                await _companyRepository.GetByEmailAsync(email);

            if (existingCompany != null &&
                existingCompany.CompanyId != companyId)
            {
                throw new InvalidOperationException(
                    "Company email already exists.");
            }

            company.CompanyName = dto.CompanyName.Trim();
            company.CompanyEmail = email;
            company.Website = dto.Website?.Trim();
            company.Industry = dto.Industry.Trim();
            company.Address = dto.Address.Trim();
            company.Description = dto.Description?.Trim();

            await _companyRepository.UpdateAsync(company);

            return MapToResponse(company);
        }

        public async Task UpdateCompanyStatusAsync(
            int companyId, CompanyStatus status)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null)
            {
                throw new KeyNotFoundException("Company not found.");
            }

            company.Status = status;

            await _companyRepository.UpdateAsync(company);
        }

        private static CompanyResponseDto MapToResponse(Company company)
        {
            return new CompanyResponseDto
            {
                CompanyId = company.CompanyId,
                CompanyName = company.CompanyName,
                CompanyEmail = company.CompanyEmail,
                Website = company.Website,
                Industry = company.Industry,
                Address = company.Address,
                Description = company.Description,
                Status = company.Status,
                CreatedAt = company.CreatedAt
            };
        }
    }
}
