using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.DTOs.Application;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Services
{
    public class JobApplicationService : IJobApplicationService
    {
        private readonly IJobApplicationRepository _applicationRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobApplicationService(
            IJobApplicationRepository applicationRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _applicationRepository = applicationRepository;
            _context = context;
            _userManager = userManager;
        }

        // Student applies for a job
        public async Task<JobApplicationResponseDto> ApplyForJobAsync(
            string userId,
            JobApplicationCreateDto dto)
        {
            var student = await _context.Students
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (student == null)
            {
                throw new InvalidOperationException(
                    "Student profile not found.");
            }

            var job = await _context.JobOpenings
                .Include(x => x.Company)
                .Include(x => x.TargetColleges)
                .FirstOrDefaultAsync(x => x.JobId == dto.JobId);

            if (job == null)
            {
                throw new InvalidOperationException(
                    "Job opening not found.");
            }

            if (job.Status != JobStatus.Published)
            {
                throw new InvalidOperationException(
                    "This job is not available for applications.");
            }

            if (job.ApplicationDeadline <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "Application deadline has passed.");
            }

            if (!job.IsOpenToAllColleges &&
                !job.TargetColleges.Any(x => x.CollegeId == student.CollegeId))
            {
                throw new InvalidOperationException(
                    "Your college is not eligible for this job.");
            }

            if (job.MinimumCGPA.HasValue &&
                student.CGPA < job.MinimumCGPA.Value)
            {
                throw new InvalidOperationException(
                    "You do not meet the minimum CGPA requirement.");
            }

            if (job.MaximumBacklogs.HasValue &&
                student.ActiveBacklogs > job.MaximumBacklogs.Value)
            {
                throw new InvalidOperationException(
                    "You do not meet the backlog requirement.");
            }

            var alreadyApplied =
                await _applicationRepository.HasStudentAppliedAsync(
                    student.StudentId,
                    job.JobId);

            if (alreadyApplied)
            {
                throw new InvalidOperationException(
                    "You have already applied for this job.");
            }

            var application = new JobApplication
            {
                JobId = job.JobId,
                StudentId = student.StudentId,
                AppliedAt = DateTime.UtcNow,
                Status = ApplicationStatus.Applied,
                ResumeUrl = dto.ResumeUrl,
                LastUpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddAsync(application);
            await _applicationRepository.SaveChangesAsync();

            // Record initial application status
            var history = new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                PreviousStatus = null,
                NewStatus = ApplicationStatus.Applied,
                UpdatedByUserId = userId,
                Remarks = "Application submitted.",
                UpdatedAt = DateTime.UtcNow
            };

            _context.ApplicationStatusHistories.Add(history);
            await _context.SaveChangesAsync();

            return MapToResponse(application, job, student);
        }

        // Student views their own applications
        public async Task<List<JobApplicationResponseDto>> GetMyApplicationsAsync(
            string userId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (student == null)
            {
                throw new InvalidOperationException(
                    "Student profile not found.");
            }

            var applications =
                await _applicationRepository.GetByStudentIdAsync(
                    student.StudentId);

            return applications
                .Select(x => MapToResponse(x, x.Job!, x.Student!))
                .ToList();
        }

        // View one application with ownership validation
        public async Task<JobApplicationResponseDto?> GetApplicationByIdAsync(
            int applicationId,
            string userId,
            string userRole)
        {
            var application =
                await _applicationRepository.GetByIdAsync(applicationId);

            if (application == null)
            {
                return null;
            }

            bool isAuthorized = false;

            if (userRole == "Student")
            {
                isAuthorized =
                    application.Student?.UserId == userId;
            }
            else if (userRole == "CompanyHR")
            {
                var companyHR = await _context.CompanyHRs
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                isAuthorized =
                    companyHR != null &&
                    application.Job?.CompanyId == companyHR.CompanyId;
            }
            else if (userRole == "PlacementOfficer")
            {
                var placementOfficer = await _context.PlacementOfficers
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                isAuthorized =
                    placementOfficer != null &&
                    application.Student?.PlacementOfficerId ==
                    placementOfficer.PlacementOfficerId;
            }

            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view this application.");
            }

            return MapToResponse(
                application,
                application.Job!,
                application.Student!);
        }

        // Company HR views applications for their own job
        public async Task<List<JobApplicationResponseDto>> GetJobApplicationsAsync(
            int jobId,
            string userId)
        {
            var companyHR = await _context.CompanyHRs
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (companyHR == null)
            {
                throw new UnauthorizedAccessException(
                    "Company HR profile not found.");
            }

            var job = await _context.JobOpenings
                .FirstOrDefaultAsync(x => x.JobId == jobId);

            if (job == null)
            {
                throw new InvalidOperationException(
                    "Job opening not found.");
            }

            if (job.CompanyId != companyHR.CompanyId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot view applications for another company's job.");
            }

            var applications =
                await _applicationRepository.GetByJobIdAsync(jobId);

            return applications
                .Select(x => MapToResponse(x, x.Job!, x.Student!))
                .ToList();
        }

        // Company HR shortlists or rejects an application
        public async Task UpdateApplicationStatusAsync(
            int applicationId,
            string userId,
            ApplicationStatusUpdateDto dto,
            string newStatus)
        {
            var application =
                await _applicationRepository.GetByIdAsync(applicationId);

            if (application == null)
            {
                throw new InvalidOperationException(
                    "Application not found.");
            }

            var companyHR = await _context.CompanyHRs
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (companyHR == null ||
                application.Job?.CompanyId != companyHR.CompanyId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot update this application.");
            }

            if (!Enum.TryParse<ApplicationStatus>(
                newStatus, true, out var status))
            {
                throw new InvalidOperationException(
                    "Invalid application status.");
            }

            if (status != ApplicationStatus.Shortlisted &&
                status != ApplicationStatus.Rejected)
            {
                throw new InvalidOperationException(
                    "Company HR can only shortlist or reject applications.");
            }

            if (application.Status == ApplicationStatus.Withdrawn ||
                application.Status == ApplicationStatus.Selected ||
                application.Status == ApplicationStatus.NotSelected ||
                application.Status == ApplicationStatus.Rejected)
            {
                throw new InvalidOperationException(
                    "This application can no longer be updated.");
            }

            var previousStatus = application.Status;

            application.Status = status;
            application.LastUpdatedAt = DateTime.UtcNow;

            await _applicationRepository.UpdateAsync(application);
            await _applicationRepository.SaveChangesAsync();

            var history = new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                PreviousStatus = previousStatus,
                NewStatus = status,
                UpdatedByUserId = userId,
                Remarks = dto.Remarks,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ApplicationStatusHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        // Student withdraws their application
        public async Task WithdrawApplicationAsync(
            int applicationId,
            string userId)
        {
            var application =
                await _applicationRepository.GetByIdAsync(applicationId);

            if (application == null)
            {
                throw new InvalidOperationException(
                    "Application not found.");
            }

            if (application.Student?.UserId != userId)
            {
                throw new UnauthorizedAccessException(
                    "You can only withdraw your own application.");
            }

            if (application.Status != ApplicationStatus.Applied &&
                application.Status != ApplicationStatus.UnderReview)
            {
                throw new InvalidOperationException(
                    "This application cannot be withdrawn at its current status.");
            }

            var previousStatus = application.Status;

            application.Status = ApplicationStatus.Withdrawn;
            application.LastUpdatedAt = DateTime.UtcNow;

            await _applicationRepository.UpdateAsync(application);
            await _applicationRepository.SaveChangesAsync();

            var history = new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                PreviousStatus = previousStatus,
                NewStatus = ApplicationStatus.Withdrawn,
                UpdatedByUserId = userId,
                Remarks = "Application withdrawn by student.",
                UpdatedAt = DateTime.UtcNow
            };

            _context.ApplicationStatusHistories.Add(history);
            await _context.SaveChangesAsync();
        }

        // Convert entity into response DTO
        private static JobApplicationResponseDto MapToResponse(
            JobApplication application,
            JobOpening job,
            Student student)
        {
            return new JobApplicationResponseDto
            {
                ApplicationId = application.ApplicationId,
                JobId = job.JobId,
                JobTitle = job.JobTitle,
                CompanyName = job.Company?.CompanyName ?? string.Empty,
                StudentId = student.StudentId,
                StudentName = student.User?.FullName ?? string.Empty,
                AppliedAt = application.AppliedAt,
                Status = application.Status,
                ResumeUrl = application.ResumeUrl,
                LastUpdatedAt = application.LastUpdatedAt
            };
        }
    }
}