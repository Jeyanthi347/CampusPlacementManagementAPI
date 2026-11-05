
using CampusPlacementManagementAPI.DTOs.Company;
using CampusPlacementManagementAPI.Enums;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<List<CompanyResponseDto>> GetAllCompaniesAsync();

        Task<List<CompanyResponseDto>> GetPendingCompaniesAsync();

        Task<CompanyResponseDto?> GetCompanyByIdAsync(int companyId);

        Task<CompanyResponseDto> RegisterCompanyAsync(CompanyCreateDto dto);

        Task<CompanyResponseDto> UpdateCompanyAsync(
            int companyId, CompanyUpdateDto dto);

        Task UpdateCompanyStatusAsync(
            int companyId, CompanyStatus status);
    }
}
