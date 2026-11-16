
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.DTOs.InterviewRound;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Services
{
    public class InterviewRoundService : IInterviewRoundService
    {
        private readonly IInterviewRoundRepository _interviewRoundRepository;
        private readonly IJobRepository _jobRepository;
        private readonly ApplicationDbContext _context;

        public InterviewRoundService(
            IInterviewRoundRepository interviewRoundRepository,
            IJobRepository jobRepository,
            ApplicationDbContext context)
        {
            _interviewRoundRepository = interviewRoundRepository;
            _jobRepository = jobRepository;
            _context = context;
        }

        private async Task<CompanyHR> GetCompanyHRAsync(string userId)
        {
            var hr = await _context.CompanyHRs
                .Include(x => x.Company)
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (hr == null)
                throw new InvalidOperationException(
                    "Company HR profile not found.");

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

        private async Task<InterviewRound> GetOwnedInterviewRoundAsync(
            int interviewRoundId, string userId)
        {
            var interviewRound =
                await _interviewRoundRepository.GetByIdAsync(
                    interviewRoundId);

            if (interviewRound == null)
                throw new KeyNotFoundException(
                    "Interview round not found.");

            await GetOwnedJobAsync(interviewRound.JobId, userId);

            return interviewRound;
        }

        private static InterviewRoundResponseDto MapToDto(
            InterviewRound interviewRound)
        {
            return new InterviewRoundResponseDto
            {
                InterviewRoundId = interviewRound.InterviewRoundId,
                JobId = interviewRound.JobId,
                JobTitle = interviewRound.JobOpening?.JobTitle
                    ?? string.Empty,
                RoundName = interviewRound.RoundName,
                RoundNumber = interviewRound.RoundNumber,
                Description = interviewRound.Description
            };
        }

        public async Task<InterviewRoundResponseDto> CreateInterviewRoundAsync(
            int jobId,
            InterviewRoundCreateDto dto,
            string userId)
        {
            var job = await GetOwnedJobAsync(jobId, userId);

            var roundNumberExists =
                await _interviewRoundRepository.RoundNumberExistsAsync(
                    jobId, dto.RoundNumber);

            if (roundNumberExists)
                throw new InvalidOperationException(
                    "This round number already exists for this job.");

            var interviewRound = new InterviewRound
            {
                JobId = jobId,
                RoundName = dto.RoundName.Trim(),
                RoundNumber = dto.RoundNumber,
                Description = dto.Description?.Trim()
            };

            await _interviewRoundRepository.AddAsync(interviewRound);

            var savedRound =
                await _interviewRoundRepository.GetByIdAsync(
                    interviewRound.InterviewRoundId);

            return MapToDto(savedRound!);
        }

        public async Task<IEnumerable<InterviewRoundResponseDto>>
            GetInterviewRoundsByJobAsync(
                int jobId,
                string userId,
                string role)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);

            if (job == null)
                throw new KeyNotFoundException("Job not found.");

            if (role == "CompanyHR")
            {
                var hr = await GetCompanyHRAsync(userId);

                if (job.CompanyId != hr.CompanyId)
                    throw new UnauthorizedAccessException(
                        "You cannot view another company's interview rounds.");
            }
            else if (role == "Student")
            {
                var student = await _context.Students
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (student == null)
                    throw new InvalidOperationException(
                        "Student profile not found.");

                var isEligibleCollege =
                    job.IsOpenToAllColleges ||
                    job.TargetColleges.Any(x =>
                        x.CollegeId == student.CollegeId);

                if (job.Status != JobStatus.Published ||
                    !isEligibleCollege)
                    throw new UnauthorizedAccessException(
                        "You cannot view interview rounds for this job.");
            }
            else if (role == "PlacementOfficer")
            {
                var officer = await _context.PlacementOfficers
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (officer == null)
                    throw new InvalidOperationException(
                        "Placement Officer profile not found.");

                var isCollegeEligible =
                    job.IsOpenToAllColleges ||
                    job.TargetColleges.Any(x =>
                        x.CollegeId == officer.CollegeId);

                if (!isCollegeEligible)
                    throw new UnauthorizedAccessException(
                        "You cannot view interview rounds for this job.");
            }
            else if (role != "SuperAdmin")
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view interview rounds.");
            }

            var rounds =
                await _interviewRoundRepository.GetByJobIdAsync(jobId);

            return rounds.Select(MapToDto);
        }

        public async Task<InterviewRoundResponseDto> UpdateInterviewRoundAsync(
            int interviewRoundId,
            InterviewRoundUpdateDto dto,
            string userId)
        {
            var interviewRound =
                await GetOwnedInterviewRoundAsync(
                    interviewRoundId, userId);

            var roundNumberExists =
                await _interviewRoundRepository.RoundNumberExistsAsync(
                    interviewRound.JobId, dto.RoundNumber);

            if (roundNumberExists &&
                interviewRound.RoundNumber != dto.RoundNumber)
                throw new InvalidOperationException(
                    "This round number already exists for this job.");

            interviewRound.RoundName = dto.RoundName.Trim();
            interviewRound.RoundNumber = dto.RoundNumber;
            interviewRound.Description = dto.Description?.Trim();

            await _interviewRoundRepository.UpdateAsync(interviewRound);

            var updatedRound =
                await _interviewRoundRepository.GetByIdAsync(
                    interviewRoundId);

            return MapToDto(updatedRound!);
        }
    }
}
