
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.DTOs.Auth;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CampusPlacementManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return null;

            if (user.AccountStatus != AccountStatus.Approved)
                return null;

            var passwordValid = await _userManager.CheckPasswordAsync(
                user,
                dto.Password);

            if (!passwordValid)
                return null;

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Count == 0)
                return null;

            var role = roles[0];

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, role)
            };

            // Add Placement Officer details to JWT
            if (role == "PlacementOfficer")
            {
                var placementOfficer = await _context.PlacementOfficers
                    .FirstOrDefaultAsync(x => x.UserId == user.Id);

                if (placementOfficer == null)
                    return null;

                claims.Add(new Claim(
                    "CollegeId",
                    placementOfficer.CollegeId.ToString()));

                claims.Add(new Claim(
                    "PlacementOfficerId",
                    placementOfficer.PlacementOfficerId.ToString()));
            }

            var key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("JWT Key is missing.");

            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is missing.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience is missing.");

            var expiryMinutes = Convert.ToDouble(
                _configuration["Jwt:ExpiryMinutes"] ?? "60");

            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                securityKey,
                SecurityAlgorithms.HmacSha256);

            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return new LoginResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = role,
                AccessToken = accessToken,
                ExpiresAt = expiresAt
            };
        }

        public async Task<bool> ChangePasswordAsync(
            string userId,
            ChangePasswordDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword);

            return result.Succeeded;
        }

        public async Task<IdentityResult?> ResetUserPasswordAsync(
            AdminResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return null;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                dto.NewPassword);

            return result;
        }

        // Assign Student role to an existing user
        public async Task<IdentityResult?> AssignStudentRoleAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return null;

            // Check whether the user already has the Student role
            var alreadyAssigned = await _userManager.IsInRoleAsync(
                user,
                "Student");

            if (alreadyAssigned)
                return IdentityResult.Success;

            // Assign Student role
            var result = await _userManager.AddToRoleAsync(
                user,
                "Student");

            return result;
        }
    }
}
