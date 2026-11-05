
using System.Text;
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// 1. DATABASE CONFIGURATION
// --------------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------------------------------
// 2. IDENTITY CONFIGURATION
// --------------------------------------------------

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --------------------------------------------------
// 3. JWT AUTHENTICATION CONFIGURATION
// --------------------------------------------------

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is missing.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero,

            NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,

            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

// --------------------------------------------------
// 4. AUTHORIZATION POLICIES
// --------------------------------------------------

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlacementOfficerOnly", policy =>
        policy.RequireRole("PlacementOfficer"));

    options.AddPolicy("StudentOnly", policy =>
        policy.RequireRole("Student"));

    options.AddPolicy("StudentManagement", policy =>
        policy.RequireRole("SuperAdmin", "PlacementOfficer"));

    options.AddPolicy("StudentAccess", policy =>
        policy.RequireRole(
            "SuperAdmin",
            "PlacementOfficer",
            "Student"));
});

// --------------------------------------------------
// 5. REPOSITORY REGISTRATIONS
// --------------------------------------------------

// College
builder.Services.AddScoped<ICollegeRepository, CollegeRepository>();

// Student
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

// Placement Officer
builder.Services.AddScoped<
    IPlacementOfficerRepository,
    PlacementOfficerRepository>();

// --------------------------------------------------
// 6. SERVICE REGISTRATIONS
// --------------------------------------------------

// Authentication
builder.Services.AddScoped<IAuthService, AuthService>();

// College
builder.Services.AddScoped<ICollegeService, CollegeService>();

// Student
builder.Services.AddScoped<IStudentService, StudentService>();

// Placement Officer
builder.Services.AddScoped<
    IPlacementOfficerService,
    PlacementOfficerService>();

// --------------------------------------------------
// 7. CONTROLLERS AND OPENAPI
// --------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

// --------------------------------------------------
// 8. SUPER ADMIN SEEDING
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        // Create system roles
        string[] roles =
        {
            "SuperAdmin",
            "PlacementOfficer",
            "CompanyHR",
            "Student"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join(", ",
                            roleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        // Read SuperAdmin details from configuration
        var adminName =
            builder.Configuration["SeedAdmin:FullName"];

        var adminEmail =
            builder.Configuration["SeedAdmin:Email"];

        var adminPassword =
            builder.Configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(adminName) ||
            string.IsNullOrWhiteSpace(adminEmail) ||
            string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "SuperAdmin configuration is missing.");
        }

        var admin = await userManager.FindByEmailAsync(adminEmail);

        // Create SuperAdmin if not already present
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminName,
                AccountStatus = CampusPlacementManagementAPI.Enums.AccountStatus.Approved,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(
                admin, adminPassword);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        createResult.Errors.Select(e => e.Description)));
            }
        }

        // Ensure SuperAdmin account is approved
        if (admin.AccountStatus !=
            CampusPlacementManagementAPI.Enums.AccountStatus.Approved)
        {
            admin.AccountStatus =
                CampusPlacementManagementAPI.Enums.AccountStatus.Approved;

            admin.UpdatedAt = DateTime.UtcNow;

            var updateResult = await userManager.UpdateAsync(admin);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        updateResult.Errors.Select(e => e.Description)));
            }
        }

        // Ensure SuperAdmin role is assigned
        if (!await userManager.IsInRoleAsync(admin, "SuperAdmin"))
        {
            var roleResult = await userManager.AddToRoleAsync(
                admin, "SuperAdmin");

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ",
                        roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}

// --------------------------------------------------
// 9. HTTP REQUEST PIPELINE
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
