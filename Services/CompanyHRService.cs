using CampusPlacementManagementAPI.DTOs.CompanyHR;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagementAPI.Services
{
    public class CompanyHRService : ICompanyHRService
    {
        private readonly ICompanyHRRepository _companyHRRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompanyHRService(
            ICompanyHRRepository companyHRRepository,
            ICompanyRepository companyRepository,
            UserManager<ApplicationUser> userManager)
        {
            _companyHRRepository = companyHRRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
        }

        public async Task<CompanyHRResponseDto> RegisterCompanyHRAsync(
            CompanyHRRegisterDto dto)
        {
            var company = await _companyRepository.GetByIdAsync(dto.CompanyId);

            if (company == null)
            {
                throw new InvalidOperationException("Company not found.");
            }

            if (company.Status != CompanyStatus.Approved)
            {
                throw new InvalidOperationException(
                    "Only approved companies can register Company HR.");
            }

            var existingUser = await _userManager.FindByEmailAsync(
                dto.Email.Trim());

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "Email is already registered.");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email.Trim(),
                Email = dto.Email.Trim(),
                FullName = dto.FullName.Trim(),
                AccountStatus = AccountStatus.Pending,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(
                user, dto.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    ", ",
                    createResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(errors);
            }

            var companyHR = new CompanyHR
            {
                UserId = user.Id,
                CompanyId = dto.CompanyId,
                Designation = dto.Designation?.Trim()
            };

            try
            {
                await _companyHRRepository.AddAsync(companyHR);
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            return MapToResponse(companyHR, user, company);
        }

        public async Task<List<CompanyHRResponseDto>> GetAllCompanyHRsAsync()
        {
            var companyHRs = await _companyHRRepository.GetAllAsync();

            return companyHRs
                .Select(x => MapToResponse(x, x.User!, x.Company!))
                .ToList();
        }

        public async Task<List<CompanyHRResponseDto>> GetPendingCompanyHRsAsync()
        {
            var companyHRs = await _companyHRRepository.GetPendingAsync();

            return companyHRs
                .Where(x => x.User != null && x.Company != null)
                .Select(x => MapToResponse(x, x.User!, x.Company!))
                .ToList();
        }

        public async Task<CompanyHRResponseDto?> GetCompanyHRByIdAsync(
            int companyHRId)
        {
            var companyHR = await _companyHRRepository.GetByIdAsync(companyHRId);

            if (companyHR == null ||
                companyHR.User == null ||
                companyHR.Company == null)
            {
                return null;
            }

            return MapToResponse(
                companyHR,
                companyHR.User,
                companyHR.Company);
        }

        public async Task<bool> UpdateCompanyHRStatusAsync(
            int companyHRId,
            string adminUserId,
            bool approve)
        {
            var companyHR = await _companyHRRepository.GetByIdAsync(companyHRId);

            if (companyHR == null || companyHR.User == null)
            {
                return false;
            }

            var user = companyHR.User;

            if (user.AccountStatus != AccountStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending Company HR registrations can be reviewed.");
            }

            if (approve)
            {
                user.AccountStatus = AccountStatus.Approved;
                companyHR.ApprovedBy = adminUserId;
                companyHR.ApprovedAt = DateTime.UtcNow;

                var roleResult = await _userManager.AddToRoleAsync(
                    user, "CompanyHR");

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        roleResult.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(errors);
                }
            }
            else
            {
                user.AccountStatus = AccountStatus.Rejected;
            }

            user.UpdatedAt = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);
            await _companyHRRepository.UpdateAsync(companyHR);

            return true;
        }

        private static CompanyHRResponseDto MapToResponse(
            CompanyHR companyHR,
            ApplicationUser user,
            Company company)
        {
            return new CompanyHRResponseDto
            {
                CompanyHRId = companyHR.CompanyHRId,
                UserId = companyHR.UserId,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                CompanyId = companyHR.CompanyId,
                CompanyName = company.CompanyName,
                Designation = companyHR.Designation,
                Status = user.AccountStatus,
                ApprovedBy = companyHR.ApprovedBy,
                ApprovedAt = companyHR.ApprovedAt
            };
        }
    }
}