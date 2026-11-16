
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.DTOs.InterviewSchedule;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Services
{
    public class InterviewScheduleService : IInterviewScheduleService
    {
        private readonly IInterviewScheduleRepository _interviewScheduleRepository;
        private readonly ApplicationDbContext _context;

        public InterviewScheduleService(
            IInterviewScheduleRepository interviewScheduleRepository,
            ApplicationDbContext context)
        {
            _interviewScheduleRepository = interviewScheduleRepository;
            _context = context;
        }

        // ==========================================
        // GET APPROVED COMPANY HR
        // ==========================================

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

        // ==========================================
        // GET APPLICATION
        // ==========================================

        private async Task<JobApplication> GetApplicationAsync(
            int applicationId)
        {
            var application = await _context.JobApplications
                .Include(x => x.Job)
                .Include(x => x.Student)
                .FirstOrDefaultAsync(x =>
                    x.ApplicationId == applicationId);

            if (application == null)
                throw new KeyNotFoundException(
                    "Job application not found.");

            return application;
        }

        // ==========================================
        // CHECK COMPANY HR OWNERSHIP
        // ==========================================

        private async Task<InterviewSchedule> GetOwnedScheduleAsync(
            int interviewScheduleId,
            string userId)
        {
            var schedule =
                await _interviewScheduleRepository.GetByIdAsync(
                    interviewScheduleId);

            if (schedule == null)
                throw new KeyNotFoundException(
                    "Interview schedule not found.");

            var hr = await GetCompanyHRAsync(userId);

            if (schedule.JobApplication?.Job == null ||
                schedule.JobApplication.Job.CompanyId != hr.CompanyId)
                throw new UnauthorizedAccessException(
                    "You cannot manage another company's interview schedule.");

            return schedule;
        }

        // ==========================================
        // MAP TO RESPONSE DTO
        // ==========================================

        private static InterviewScheduleResponseDto MapToDto(
            InterviewSchedule schedule)
        {
            return new InterviewScheduleResponseDto
            {
                InterviewScheduleId = schedule.InterviewScheduleId,
                ApplicationId = schedule.ApplicationId,
                InterviewRoundId = schedule.InterviewRoundId,
                RoundName = schedule.InterviewRound?.RoundName
                    ?? string.Empty,
                ScheduledAt = schedule.ScheduledAt,
                MeetingLink = schedule.MeetingLink,
                Venue = schedule.Venue,
                Status = schedule.Status,
                Feedback = schedule.Feedback
            };
        }

        // ==========================================
        // CREATE INTERVIEW SCHEDULE
        // ==========================================

        public async Task<InterviewScheduleResponseDto>
            CreateInterviewScheduleAsync(
                int applicationId,
                InterviewScheduleCreateDto dto,
                string userId)
        {
            var hr = await GetCompanyHRAsync(userId);

            var application = await GetApplicationAsync(applicationId);

            if (application.Job == null ||
                application.Job.CompanyId != hr.CompanyId)
                throw new UnauthorizedAccessException(
                    "You cannot schedule interviews for another company's job.");

            var interviewRound = await _context.InterviewRounds
                .FirstOrDefaultAsync(x =>
                    x.InterviewRoundId == dto.InterviewRoundId);

            if (interviewRound == null)
                throw new KeyNotFoundException(
                    "Interview round not found.");

            if (interviewRound.JobId != application.JobId)
                throw new InvalidOperationException(
                    "The interview round does not belong to this job.");

            var conflict =
                await _interviewScheduleRepository.HasScheduleConflictAsync(
                    applicationId,
                    dto.InterviewRoundId);

            if (conflict)
                throw new InvalidOperationException(
                    "An interview is already scheduled for this application and round.");

            var schedule = new InterviewSchedule
            {
                ApplicationId = applicationId,
                InterviewRoundId = dto.InterviewRoundId,
                ScheduledAt = dto.ScheduledAt,
                MeetingLink = dto.MeetingLink?.Trim(),
                Venue = dto.Venue?.Trim(),
                Status = InterviewScheduleStatus.Scheduled
            };

            await _interviewScheduleRepository.AddAsync(schedule);

            var savedSchedule =
                await _interviewScheduleRepository.GetByIdAsync(
                    schedule.InterviewScheduleId);

            return MapToDto(savedSchedule!);
        }

        // ==========================================
        // GET INTERVIEW SCHEDULES BY APPLICATION
        // ==========================================

        public async Task<IEnumerable<InterviewScheduleResponseDto>>
            GetInterviewSchedulesByApplicationAsync(
                int applicationId,
                string userId,
                string role)
        {
            var application = await GetApplicationAsync(applicationId);

            if (role == "CompanyHR")
            {
                var hr = await GetCompanyHRAsync(userId);

                if (application.Job == null ||
                    application.Job.CompanyId != hr.CompanyId)
                    throw new UnauthorizedAccessException(
                        "You cannot view another company's interviews.");
            }
            else if (role == "Student")
            {
                if (application.Student == null ||
                    application.Student.UserId != userId)
                    throw new UnauthorizedAccessException(
                        "You can only view your own interview schedules.");
            }
            else if (role == "PlacementOfficer")
            {
                var officer = await _context.PlacementOfficers
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (officer == null)
                    throw new InvalidOperationException(
                        "Placement Officer profile not found.");

                if (application.Student == null ||
                    application.Student.CollegeId != officer.CollegeId)
                    throw new UnauthorizedAccessException(
                        "You cannot view another college's interviews.");
            }
            else if (role != "SuperAdmin")
            {
                throw new UnauthorizedAccessException(
                    "You are not authorized to view interview schedules.");
            }

            var schedules =
                await _interviewScheduleRepository.GetByApplicationIdAsync(
                    applicationId);

            return schedules.Select(MapToDto);
        }

        // ==========================================
        // UPDATE INTERVIEW SCHEDULE
        // ==========================================

        public async Task<InterviewScheduleResponseDto>
            UpdateInterviewScheduleAsync(
                int interviewScheduleId,
                InterviewScheduleUpdateDto dto,
                string userId)
        {
            var schedule = await GetOwnedScheduleAsync(
                interviewScheduleId,
                userId);

            schedule.ScheduledAt = dto.ScheduledAt;
            schedule.MeetingLink = dto.MeetingLink?.Trim();
            schedule.Venue = dto.Venue?.Trim();

            await _interviewScheduleRepository.UpdateAsync(schedule);

            var updatedSchedule =
                await _interviewScheduleRepository.GetByIdAsync(
                    interviewScheduleId);

            return MapToDto(updatedSchedule!);
        }

        // ==========================================
        // UPDATE INTERVIEW FEEDBACK
        // ==========================================

        public async Task<InterviewScheduleResponseDto>
            UpdateInterviewFeedbackAsync(
                int interviewScheduleId,
                string feedback,
                string userId)
        {
            var schedule = await GetOwnedScheduleAsync(
                interviewScheduleId,
                userId);

            schedule.Feedback = feedback.Trim();

            await _interviewScheduleRepository.UpdateAsync(schedule);

            var updatedSchedule =
                await _interviewScheduleRepository.GetByIdAsync(
                    interviewScheduleId);

            return MapToDto(updatedSchedule!);
        }
    }
}
