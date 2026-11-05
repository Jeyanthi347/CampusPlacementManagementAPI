using CampusPlacementManagementAPI.DTOs.CompanyHR;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface ICompanyHRService
    {
        Task<CompanyHRResponseDto> RegisterCompanyHRAsync(
            CompanyHRRegisterDto dto);

        Task<List<CompanyHRResponseDto>> GetAllCompanyHRsAsync();

        Task<List<CompanyHRResponseDto>> GetPendingCompanyHRsAsync();

        Task<CompanyHRResponseDto?> GetCompanyHRByIdAsync(int companyHRId);

        Task<bool> UpdateCompanyHRStatusAsync(
            int companyHRId,
            string adminUserId,
            bool approve);
    }
}