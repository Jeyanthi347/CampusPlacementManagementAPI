
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.DTOs.RecruitmentDrive;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Services
{
    public class RecruitmentDriveService : IRecruitmentDriveService
    {
        private readonly IRecruitmentDriveRepository _driveRepository;
        private readonly ApplicationDbContext _context;

        public RecruitmentDriveService(
            IRecruitmentDriveRepository driveRepository,
            ApplicationDbContext context)
        {
            _driveRepository = driveRepository;
            _context = context;
        }

        // Get approved Company HR details
        private async Task<CompanyHR> GetCompanyHRAsync(string userId)
        {
            var hr = await _context.CompanyHRs
                .Include(h => h.Company)
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (hr == null)
                throw new Exception("Company HR profile not found.");

            if (hr.ApprovedAt == null)
                throw new Exception("Your Company HR account is not approved.");

            if (hr.Company == null)
                throw new Exception("Company details not found.");

            if (hr.Company.Status != CompanyStatus.Approved)
                throw new Exception("Your company is not approved.");

            return hr;
        }

        // Get a drive based on the user's role and ownership
        private async Task<RecruitmentDrive> GetManagedDriveAsync(
            int driveId, string userId, string role)
        {
            var drive = await _driveRepository.GetByIdAsync(driveId);

            if (drive == null)
                throw new Exception("Recruitment drive not found.");

            // SuperAdmin can manage any recruitment drive
            if (role == "SuperAdmin")
                return drive;

            // CompanyHR can manage only their company's drives
            if (role == "CompanyHR")
            {
                var hr = await GetCompanyHRAsync(userId);

                if (drive.JobOpening == null ||
                    drive.JobOpening.CompanyId != hr.CompanyId)
                {
                    throw new UnauthorizedAccessException(
                        "You are not authorized to manage this drive.");
                }

                return drive;
            }

            throw new UnauthorizedAccessException(
                "You are not authorized to manage recruitment drives.");
        }

        // Convert RecruitmentDrive model into response DTO
        private static RecruitmentDriveResponseDto MapToDto(
            RecruitmentDrive drive)
        {
            return new RecruitmentDriveResponseDto
            {
                DriveId = drive.DriveId,
                JobId = drive.JobId,
                JobTitle = drive.JobOpening?.JobTitle ?? string.Empty,
                CollegeId = drive.CollegeId,
                CollegeName = drive.College?.CollegeName ?? string.Empty,
                DriveDate = drive.DriveDate,
                Venue = drive.Venue,
                Description = drive.Description,
                Status = (int)drive.Status
            };
        }

        // Create Recruitment Drive
        public async Task<RecruitmentDriveResponseDto> CreateDriveAsync(
            RecruitmentDriveCreateDto dto,
            string userId,
            string role)
        {
            // CompanyHR must be approved and can create drives
            // only for jobs belonging to their company.
            if (role == "CompanyHR")
            {
                var hr = await GetCompanyHRAsync(userId);

                var hrJob = await _context.JobOpenings
                    .FirstOrDefaultAsync(j => j.JobId == dto.JobId);

                if (hrJob == null)
                    throw new Exception("Job opening not found.");

                if (hrJob.CompanyId != hr.CompanyId)
                    throw new UnauthorizedAccessException(
                        "You can create drives only for your company's jobs.");
            }
            else if (role != "SuperAdmin")
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to create recruitment drives.");
            }

            var job = await _context.JobOpenings
                .FirstOrDefaultAsync(j => j.JobId == dto.JobId);

            if (job == null)
                throw new Exception("Job opening not found.");

            var college = await _context.Colleges
                .FirstOrDefaultAsync(c => c.CollegeId == dto.CollegeId);

            if (college == null)
                throw new Exception("College not found.");

            if (college.Status != CollegeStatus.Approved)
                throw new Exception("Selected college is not approved.");

            if (!job.IsOpenToAllColleges &&
                !await _context.JobTargetColleges.AnyAsync(t =>
                    t.JobId == dto.JobId &&
                    t.CollegeId == dto.CollegeId))
            {
                throw new Exception(
                    "This job is not targeted to the selected college.");
            }

            if (dto.DriveDate <= DateTime.UtcNow)
                throw new Exception("Drive date must be in the future.");

            var drive = new RecruitmentDrive
            {
                JobId = dto.JobId,
                CollegeId = dto.CollegeId,
                DriveDate = dto.DriveDate,
                Venue = dto.Venue,
                Description = dto.Description,
                Status = RecruitmentDriveStatus.Scheduled
            };

            await _driveRepository.AddAsync(drive);

            var createdDrive = await _driveRepository.GetByIdAsync(
                drive.DriveId);

            if (createdDrive == null)
                throw new Exception("Unable to retrieve created drive.");

            return MapToDto(createdDrive);
        }

        // Get all Recruitment Drives based on user role
        public async Task<IEnumerable<RecruitmentDriveResponseDto>>
            GetAllDrivesAsync(string userId, string role)
        {
            IEnumerable<RecruitmentDrive> drives;

            if (role == "PlacementOfficer")
            {
                var officer = await _context.PlacementOfficers
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (officer == null)
                    throw new Exception(
                        "Placement officer profile not found.");

                drives = await _driveRepository.GetByCollegeIdAsync(
                    officer.CollegeId);
            }
            else if (role == "CompanyHR")
            {
                var hr = await GetCompanyHRAsync(userId);

                drives = await _driveRepository.GetByCompanyIdAsync(
                    hr.CompanyId);
            }
            else if (role == "SuperAdmin" || role == "Student")
            {
                drives = await _driveRepository.GetAllAsync();
            }
            else
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view recruitment drives.");
            }

            return drives.Select(MapToDto);
        }

        // Get Recruitment Drive by ID
        public async Task<RecruitmentDriveResponseDto?> GetDriveByIdAsync(
            int driveId, string userId, string role)
        {
            var drive = await _driveRepository.GetByIdAsync(driveId);

            if (drive == null)
                return null;

            if (role == "PlacementOfficer")
            {
                var officer = await _context.PlacementOfficers
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (officer == null ||
                    officer.CollegeId != drive.CollegeId)
                {
                    throw new UnauthorizedAccessException(
                        "You are not authorized to view this drive.");
                }
            }
            else if (role == "CompanyHR")
            {
                var hr = await GetCompanyHRAsync(userId);

                if (drive.JobOpening?.CompanyId != hr.CompanyId)
                    throw new UnauthorizedAccessException(
                        "You are not authorized to view this drive.");
            }
            else if (role != "SuperAdmin" && role != "Student")
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view this drive.");
            }

            return MapToDto(drive);
        }

        // Update Recruitment Drive
        public async Task<RecruitmentDriveResponseDto> UpdateDriveAsync(
            int driveId,
            RecruitmentDriveUpdateDto dto,
            string userId,
            string role)
        {
            var drive = await GetManagedDriveAsync(
                driveId, userId, role);

            if (drive.Status == RecruitmentDriveStatus.Cancelled)
                throw new Exception(
                    "Cancelled drives cannot be updated.");

            if (drive.Status == RecruitmentDriveStatus.Completed)
                throw new Exception(
                    "Completed drives cannot be updated.");

            if (dto.DriveDate <= DateTime.UtcNow)
                throw new Exception(
                    "Drive date must be in the future.");

            drive.DriveDate = dto.DriveDate;
            drive.Venue = dto.Venue;
            drive.Description = dto.Description;

            await _driveRepository.UpdateAsync(drive);

            return MapToDto(drive);
        }

        // Cancel Recruitment Drive
        public async Task<RecruitmentDriveResponseDto> CancelDriveAsync(
            int driveId,
            string userId,
            string role)
        {
            var drive = await GetManagedDriveAsync(
                driveId, userId, role);

            if (drive.Status == RecruitmentDriveStatus.Completed)
                throw new Exception(
                    "Completed drives cannot be cancelled.");

            if (drive.Status == RecruitmentDriveStatus.Cancelled)
                throw new Exception(
                    "This drive is already cancelled.");

            drive.Status = RecruitmentDriveStatus.Cancelled;

            await _driveRepository.UpdateAsync(drive);

            return MapToDto(drive);
        }
    }
}
