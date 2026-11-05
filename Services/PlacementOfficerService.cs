
using System.Security.Claims;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementManagementAPI.Services
{
    public class PlacementOfficerService : IPlacementOfficerService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IPlacementOfficerRepository _officerRepository;
        private readonly ICollegeRepository _collegeRepository;

        public PlacementOfficerService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IPlacementOfficerRepository officerRepository,
            ICollegeRepository collegeRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _officerRepository = officerRepository;
            _collegeRepository = collegeRepository;
        }

        // 1. Register Placement Officer
        public async Task<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto>
            RegisterAsync(
                CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerRegisterDto dto)
        {
            var college = await _collegeRepository.GetByIdAsync(dto.CollegeId);

            if (college == null)
            {
                throw new InvalidOperationException("College not found.");
            }

            if (college.Status != CollegeStatus.Approved)
            {
                throw new InvalidOperationException(
                    "Only approved colleges can register placement officers.");
            }

            var email = dto.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "Email is already registered.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                PhoneNumber = dto.PhoneNumber.Trim(),
                FullName = dto.FullName.Trim(),
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(
                user, dto.Password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        createResult.Errors.Select(e => e.Description)));
            }

            var officer = new PlacementOfficer
            {
                UserId = user.Id,
                CollegeId = dto.CollegeId,
                Designation = dto.Designation.Trim(),
                Status = PlacementOfficerStatus.Pending
            };

            try
            {
                await _officerRepository.AddAsync(officer);
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            return MapToDto(officer, user, college);
        }

        // 2. Get all Placement Officers
        public async Task<List<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto>>
            GetAllAsync()
        {
            var officers = await _officerRepository.GetAllAsync();

            return officers.Select(officer =>
                MapToDto(officer, officer.User, officer.College)).ToList();
        }

        // 3. Get pending Placement Officers
        public async Task<List<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto>>
            GetPendingAsync()
        {
            var officers = await _officerRepository.GetPendingAsync();

            return officers.Select(officer =>
                MapToDto(officer, officer.User, officer.College)).ToList();
        }

        // 4. Get Placement Officer by ID
        public async Task<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto?>
            GetByIdAsync(int id)
        {
            var officer = await _officerRepository.GetByIdAsync(id);

            if (officer == null)
            {
                return null;
            }

            return MapToDto(officer, officer.User, officer.College);
        }

        // 5. Get Placement Officer by User ID
        public async Task<CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto?>
            GetByUserIdAsync(string userId)
        {
            var officer = await _officerRepository.GetByUserIdAsync(userId);

            if (officer == null)
            {
                return null;
            }

            return MapToDto(officer, officer.User, officer.College);
        }

        // 6. Approve Placement Officer
        public async Task<bool> ApproveAsync(int id, string approvedBy)
        {
            var officer = await _officerRepository.GetByIdAsync(id);

            if (officer == null)
            {
                return false;
            }

            if (officer.Status != PlacementOfficerStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending placement officers can be approved.");
            }

            var user = await _userManager.FindByIdAsync(officer.UserId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            var college = await _collegeRepository.GetByIdAsync(
                officer.CollegeId);

            if (college == null || college.Status != CollegeStatus.Approved)
            {
                throw new InvalidOperationException(
                    "The officer's college must be approved.");
            }

            officer.Status = PlacementOfficerStatus.Approved;
            officer.ApprovedBy = approvedBy;
            officer.ApprovedAt = DateTime.UtcNow;

            user.AccountStatus = AccountStatus.Approved;
            user.UpdatedAt = DateTime.UtcNow;

            // Create role if it does not exist
            if (!await _roleManager.RoleExistsAsync("PlacementOfficer"))
            {
                var roleResult = await _roleManager.CreateAsync(
                    new IdentityRole("PlacementOfficer"));

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            roleResult.Errors.Select(e => e.Description)));
                }
            }

            // Assign role
            if (!await _userManager.IsInRoleAsync(user, "PlacementOfficer"))
            {
                var roleResult = await _userManager.AddToRoleAsync(
                    user, "PlacementOfficer");

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            roleResult.Errors.Select(e => e.Description)));
                }
            }

            // Remove old claims, if present
            var existingClaims = await _userManager.GetClaimsAsync(user);

            var oldCollegeClaim = existingClaims.FirstOrDefault(
                c => c.Type == "CollegeId");

            if (oldCollegeClaim != null)
            {
                var removeResult = await _userManager.RemoveClaimAsync(
                    user, oldCollegeClaim);

                if (!removeResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Unable to update the college claim.");
                }
            }

            var oldOfficerClaim = existingClaims.FirstOrDefault(
                c => c.Type == "PlacementOfficerId");

            if (oldOfficerClaim != null)
            {
                var removeResult = await _userManager.RemoveClaimAsync(
                    user, oldOfficerClaim);

                if (!removeResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "Unable to update the placement officer claim.");
                }
            }

            // Add current claims
            var claimResult = await _userManager.AddClaimsAsync(
                user,
                new List<Claim>
                {
                    new Claim("CollegeId", officer.CollegeId.ToString()),
                    new Claim(
                        "PlacementOfficerId",
                        officer.PlacementOfficerId.ToString())
                });

            if (!claimResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        claimResult.Errors.Select(e => e.Description)));
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        updateResult.Errors.Select(e => e.Description)));
            }

            await _officerRepository.UpdateAsync(officer);

            return true;
        }

        // 7. Reject Placement Officer
        public async Task<bool> RejectAsync(int id)
        {
            var officer = await _officerRepository.GetByIdAsync(id);

            if (officer == null)
            {
                return false;
            }

            if (officer.Status != PlacementOfficerStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending placement officers can be rejected.");
            }

            officer.Status = PlacementOfficerStatus.Rejected;

            var user = await _userManager.FindByIdAsync(officer.UserId);

            if (user != null)
            {
                user.AccountStatus = AccountStatus.Rejected;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            await _officerRepository.UpdateAsync(officer);

            return true;
        }

        // 8. Suspend Placement Officer
        public async Task<bool> SuspendAsync(int id)
        {
            var officer = await _officerRepository.GetByIdAsync(id);

            if (officer == null)
            {
                return false;
            }

            if (officer.Status != PlacementOfficerStatus.Approved)
            {
                throw new InvalidOperationException(
                    "Only approved placement officers can be suspended.");
            }

            officer.Status = PlacementOfficerStatus.Suspended;

            var user = await _userManager.FindByIdAsync(officer.UserId);

            if (user != null)
            {
                user.AccountStatus = AccountStatus.Suspended;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            await _officerRepository.UpdateAsync(officer);

            return true;
        }

        // Common response mapping
        private static CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto
            MapToDto(
                PlacementOfficer officer,
                ApplicationUser? user,
                College? college)
        {
            return new CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerResponseDto
            {
                PlacementOfficerId = officer.PlacementOfficerId,
                UserId = officer.UserId,
                FullName = user?.FullName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty,
                CollegeId = officer.CollegeId,
                CollegeName = college?.CollegeName ?? string.Empty,
                Designation = officer.Designation,
                Status = officer.Status,
                ApprovedBy = officer.ApprovedBy,
                ApprovedAt = officer.ApprovedAt,
                CreatedAt = user?.CreatedAt ?? DateTime.UtcNow
            };
        }
    }
}
