
using System.Security.Claims;
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.DTOs.Job;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobService(
            IJobRepository jobRepository,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _jobRepository = jobRepository;
            _context = context;
            _userManager = userManager;
        }

        private async Task<CompanyHR> GetCompanyHRAsync(string userId)
        {
            var hr = await _context.CompanyHRs
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (hr == null)
                throw new InvalidOperationException(
                    "Company HR profile not found.");

            if (hr.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You are not authorized.");

            if (hr.Company == null ||
                hr.Company.Status != CompanyStatus.Approved)
                throw new InvalidOperationException(
                    "Your company is not approved.");

            return hr;
        }

        private async Task<JobOpening> GetOwnedJobAsync(
            int jobId, string userId)
        {
            var hr = await GetCompanyHRAsync(userId);

            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            if (job.CreatedByHRId != hr.CompanyHRId)
                throw new UnauthorizedAccessException(
                    "You cannot manage another HR's job.");

            return job;
        }

        private static JobResponseDto MapToDto(JobOpening job)
        {
            return new JobResponseDto
            {
                JobId = job.JobId,
                CompanyId = job.CompanyId,
                CompanyName = job.Company?.CompanyName ?? string.Empty,
                CreatedByHRId = job.CreatedByHRId,
                JobTitle = job.JobTitle,
                JobDescription = job.JobDescription,
                JobType = job.JobType,
                Location = job.Location,
                SalaryPackage = job.SalaryPackage,
                MinimumCGPA = job.MinimumCGPA,
                MaximumBacklogs = job.MaximumBacklogs,
                ApplicationDeadline = job.ApplicationDeadline,
                IsOpenToAllColleges = job.IsOpenToAllColleges,
                Status = job.Status,
                CreatedAt = job.CreatedAt,
                TargetCollegeIds = job.TargetColleges
                    .Select(x => x.CollegeId)
                    .ToList()
            };
        }

        public async Task<JobResponseDto> CreateJobAsync(
            JobCreateDto dto, string userId)
        {
            var hr = await GetCompanyHRAsync(userId);

            if (dto.ApplicationDeadline <= DateTime.UtcNow)
                throw new InvalidOperationException(
                    "Application deadline must be in the future.");

            if (dto.IsOpenToAllColleges &&
                dto.TargetCollegeIds.Any())
                throw new InvalidOperationException(
                    "Do not provide target colleges when the job is open to all colleges.");

            if (!dto.IsOpenToAllColleges &&
                !dto.TargetCollegeIds.Any())
                throw new InvalidOperationException(
                    "Select at least one target college.");

            var collegeIds = dto.TargetCollegeIds.Distinct().ToList();

            if (collegeIds.Count != dto.TargetCollegeIds.Count)
                throw new InvalidOperationException(
                    "Duplicate college IDs are not allowed.");

            if (collegeIds.Any())
            {
                var approvedCollegeIds = await _context.Colleges
                    .Where(c => collegeIds.Contains(c.CollegeId) &&
                                c.Status == CollegeStatus.Approved)
                    .Select(c => c.CollegeId)
                    .ToListAsync();

                if (approvedCollegeIds.Count != collegeIds.Count)
                    throw new InvalidOperationException(
                        "One or more colleges do not exist or are not approved.");
            }

            var job = new JobOpening
            {
                CompanyId = hr.CompanyId,
                CreatedByHRId = hr.CompanyHRId,
                JobTitle = dto.JobTitle.Trim(),
                JobDescription = dto.JobDescription.Trim(),
                JobType = dto.JobType.Trim(),
                Location = dto.Location.Trim(),
                SalaryPackage = dto.SalaryPackage,
                MinimumCGPA = dto.MinimumCGPA,
                MaximumBacklogs = dto.MaximumBacklogs,
                ApplicationDeadline = dto.ApplicationDeadline,
                IsOpenToAllColleges = dto.IsOpenToAllColleges,
                Status = JobStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                TargetColleges = collegeIds.Select(id =>
                    new JobTargetCollege
                    {
                        CollegeId = id
                    }).ToList()
            };

            await _jobRepository.AddAsync(job);

            var savedJob = await _jobRepository.GetByIdAsync(job.JobId);

            return MapToDto(savedJob!);
        }

        public async Task<IEnumerable<JobResponseDto>> GetAllJobsAsync(
            string userId, string role)
        {
            if (role == "CompanyHR")
            {
                var hr = await GetCompanyHRAsync(userId);

                var ownJobs =
                    await _jobRepository.GetByCompanyHRIdAsync(
                        hr.CompanyHRId);

                return ownJobs.Select(MapToDto);
            }

            var jobs = await _jobRepository.GetAllAsync();

            if (role == "Student")
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (student == null)
                    throw new InvalidOperationException(
                        "Student profile not found.");

                return jobs
                    .Where(j => j.Status == JobStatus.Published &&
                        (j.IsOpenToAllColleges ||
                         j.TargetColleges.Any(t =>
                             t.CollegeId == student.CollegeId)))
                    .Select(MapToDto);
            }

            return jobs.Select(MapToDto);
        }

        public async Task<JobResponseDto?> GetJobByIdAsync(
            int jobId, string userId, string role)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                return null;

            if (role == "CompanyHR")
            {
                var hr = await GetCompanyHRAsync(userId);

                if (job.CreatedByHRId != hr.CompanyHRId)
                    throw new UnauthorizedAccessException(
                        "You cannot access another HR's job.");
            }
            else if (role == "Student")
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (student == null)
                    throw new InvalidOperationException(
                        "Student profile not found.");

                var visible = job.Status == JobStatus.Published &&
                    (job.IsOpenToAllColleges ||
                     job.TargetColleges.Any(t =>
                         t.CollegeId == student.CollegeId));

                if (!visible)
                    throw new UnauthorizedAccessException(
                        "This job is not available to you.");
            }

            return MapToDto(job);
        }

        public async Task<JobResponseDto> UpdateJobAsync(
            int jobId, JobUpdateDto dto, string userId)
        {
            var job = await GetOwnedJobAsync(jobId, userId);

            if (job.Status != JobStatus.Draft)
                throw new InvalidOperationException(
                    "Only draft jobs can be updated.");

            if (dto.ApplicationDeadline <= DateTime.UtcNow)
                throw new InvalidOperationException(
                    "Application deadline must be in the future.");

            var collegeIds = dto.TargetCollegeIds.Distinct().ToList();

            if (dto.IsOpenToAllColleges && collegeIds.Any())
                throw new InvalidOperationException(
                    "Do not provide target colleges when the job is open to all colleges.");

            if (!dto.IsOpenToAllColleges && !collegeIds.Any())
                throw new InvalidOperationException(
                    "Select at least one target college.");

            if (collegeIds.Count != dto.TargetCollegeIds.Count)
                throw new InvalidOperationException(
                    "Duplicate college IDs are not allowed.");

            var approvedIds = await _context.Colleges
                .Where(c => collegeIds.Contains(c.CollegeId) &&
                            c.Status == CollegeStatus.Approved)
                .Select(c => c.CollegeId)
                .ToListAsync();

            if (approvedIds.Count != collegeIds.Count)
                throw new InvalidOperationException(
                    "One or more colleges do not exist or are not approved.");

            job.JobTitle = dto.JobTitle.Trim();
            job.JobDescription = dto.JobDescription.Trim();
            job.JobType = dto.JobType.Trim();
            job.Location = dto.Location.Trim();
            job.SalaryPackage = dto.SalaryPackage;
            job.MinimumCGPA = dto.MinimumCGPA;
            job.MaximumBacklogs = dto.MaximumBacklogs;
            job.ApplicationDeadline = dto.ApplicationDeadline;
            job.IsOpenToAllColleges = dto.IsOpenToAllColleges;

            job.TargetColleges.Clear();

            foreach (var collegeId in collegeIds)
            {
                job.TargetColleges.Add(new JobTargetCollege
                {
                    CollegeId = collegeId
                });
            }

            await _jobRepository.UpdateAsync(job);

            return MapToDto(job);
        }

        public async Task<JobResponseDto> PublishJobAsync(
            int jobId, string userId)
        {
            var job = await GetOwnedJobAsync(jobId, userId);

            if (job.Status != JobStatus.Draft)
                throw new InvalidOperationException(
                    "Only draft jobs can be published.");

            if (job.ApplicationDeadline <= DateTime.UtcNow)
                throw new InvalidOperationException(
                    "Application deadline has passed.");

            job.Status = JobStatus.Published;

            await _jobRepository.UpdateAsync(job);

            return MapToDto(job);
        }

        public async Task<JobResponseDto> CloseJobAsync(
            int jobId, string userId)
        {
            var job = await GetOwnedJobAsync(jobId, userId);

            if (job.Status != JobStatus.Published)
                throw new InvalidOperationException(
                    "Only published jobs can be closed.");

            job.Status = JobStatus.Closed;

            await _jobRepository.UpdateAsync(job);

            return MapToDto(job);
        }

        public async Task DeleteJobAsync(int jobId, string userId)
        {
            var job = await GetOwnedJobAsync(jobId, userId);

            if (job.Status != JobStatus.Draft)
                throw new InvalidOperationException(
                    "Only draft jobs can be deleted.");

            await _jobRepository.DeleteAsync(job);
        }

        public async Task<IEnumerable<JobResponseDto>> GetEligibleJobsAsync(
            string userId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                throw new InvalidOperationException(
                    "Student profile not found.");

            var jobs = await _jobRepository.GetAllAsync();

            return jobs.Where(j =>
                    j.Status == JobStatus.Published &&
                    j.ApplicationDeadline > DateTime.UtcNow &&
                    (j.IsOpenToAllColleges ||
                     j.TargetColleges.Any(t =>
                         t.CollegeId == student.CollegeId)) &&
                    (!j.MinimumCGPA.HasValue ||
                     student.CGPA >= j.MinimumCGPA.Value) &&
                    (!j.MaximumBacklogs.HasValue ||
                     student.ActiveBacklogs <= j.MaximumBacklogs.Value))
                .Select(MapToDto);
        }

        public async Task<JobEligibilityResponseDto> CheckEligibilityAsync(
            int jobId, string userId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
                throw new InvalidOperationException(
                    "Student profile not found.");

            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            var collegeEligible = job.IsOpenToAllColleges ||
                job.TargetColleges.Any(t =>
                    t.CollegeId == student.CollegeId);

            var cgpaEligible = !job.MinimumCGPA.HasValue ||
                student.CGPA >= job.MinimumCGPA.Value;

            var backlogEligible = !job.MaximumBacklogs.HasValue ||
                student.ActiveBacklogs <= job.MaximumBacklogs.Value;

            var isEligible =
                job.Status == JobStatus.Published &&
                job.ApplicationDeadline > DateTime.UtcNow &&
                collegeEligible &&
                cgpaEligible &&
                backlogEligible;

            return new JobEligibilityResponseDto
            {
                JobId = jobId,
                IsEligible = isEligible,
                Message = isEligible
                    ? "You are eligible for this job."
                    : "You are not eligible for this job."
            };
        }
    }
}
