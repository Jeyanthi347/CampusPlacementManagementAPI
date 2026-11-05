
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services;
using CampusPlacementManagementAPI.Services.Interfaces;
using CampusPlacementManagementAPI.Enums;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Identity Configuration
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

// 3. JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)),

        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.Name,
        ClockSkew = TimeSpan.Zero
    };
});

// 4. Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlacementOfficerOnly",
        policy => policy.RequireRole("PlacementOfficer"));

    options.AddPolicy("StudentOnly",
        policy => policy.RequireRole("Student"));

    options.AddPolicy("StudentManagement",
        policy => policy.RequireRole("SuperAdmin", "PlacementOfficer"));

    options.AddPolicy("StudentAccess",
        policy => policy.RequireRole("SuperAdmin", "PlacementOfficer", "Student"));
});

// 5. Dependency Injection
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 6. Create Initial SuperAdmin
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Create SuperAdmin role if it does not exist
        if (!await roleManager.RoleExistsAsync("SuperAdmin"))
        {
            var role = new IdentityRole("SuperAdmin");
            var roleResult = await roleManager.CreateAsync(role);

            if (!roleResult.Succeeded)
            {
                throw new Exception("Failed to create SuperAdmin role: " +
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        var adminEmail = builder.Configuration["SeedAdmin:Email"]
            ?? throw new InvalidOperationException("SeedAdmin Email is missing.");

        var adminPassword = builder.Configuration["SeedAdmin:Password"]
            ?? throw new InvalidOperationException("SeedAdmin Password is missing.");

        var adminFullName = builder.Configuration["SeedAdmin:FullName"]
            ?? throw new InvalidOperationException("SeedAdmin FullName is missing.");

        var admin = await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = adminFullName,
                AccountStatus = AccountStatus.Approved,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(admin, adminPassword);

            if (!createResult.Succeeded)
            {
                throw new Exception("Failed to create SuperAdmin: " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
        }

        // Ensure admin account is approved
        if (admin.AccountStatus != AccountStatus.Approved)
        {
            admin.AccountStatus = AccountStatus.Approved;
            admin.UpdatedAt = DateTime.UtcNow;

            var updateResult = await userManager.UpdateAsync(admin);

            if (!updateResult.Succeeded)
            {
                throw new Exception("Failed to approve SuperAdmin account.");
            }
        }

        // Assign SuperAdmin role
        if (!await userManager.IsInRoleAsync(admin, "SuperAdmin"))
        {
            var roleResult = await userManager.AddToRoleAsync(admin, "SuperAdmin");

            if (!roleResult.Succeeded)
            {
                throw new Exception("Failed to assign SuperAdmin role: " +
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}

// 7. HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
