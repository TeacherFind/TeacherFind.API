using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TeacherFind.API.Hubs;
using TeacherFind.Application.Abstractions.Identity;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Application.Features.Auth;
using TeacherFind.Application.Features.Bookings;
using TeacherFind.Application.Features.Chat;
using TeacherFind.Application.Features.Favorites;
using TeacherFind.Application.Features.Listings;
using TeacherFind.Application.Features.Notifications;
using TeacherFind.Application.Features.Reports;
using TeacherFind.Application.Features.Reviews;
using TeacherFind.Application.Features.Students;
using TeacherFind.Application.Features.Tutors;
using TeacherFind.Infrastructure.Identity;
using TeacherFind.Infrastructure.Persistence;
using TeacherFind.Infrastructure.Persistence.Repositories;
using TeacherFind.Infrastructure.Persistence.Seed;
using TeacherFind.Infrastructure.Seeds;
using TeacherFind.Infrastructure.Services.Admin;
using TeacherFind.Infrastructure.Services.Education;


var builder = WebApplication.CreateBuilder(args);

// =====================================================
// Configuration
// =====================================================

builder.Configuration.AddJsonFile(
    "appsettings.Local.json",
    optional: true,
    reloadOnChange: true);

// =====================================================
// Database
// =====================================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================================================
// JWT Authentication
// =====================================================

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key bulunamadı.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };

        // SignalR için JWT desteği
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken) &&
                    path.StartsWithSegments("/chat"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

// =====================================================
// Authorization Policies
// =====================================================

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StudentOnly", policy =>
        policy.RequireRole("Student"));

    options.AddPolicy("TutorOnly", policy =>
        policy.RequireRole("Tutor"));

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));

    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireRole("SuperAdmin"));
});

// =====================================================
// CORS
// =====================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:3000",
                "https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// =====================================================
// Framework Services
// =====================================================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

// =====================================================
// Swagger + JWT
// =====================================================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TeacherFind API",
        Version = "v1",
        Description = "TeacherFind backend API dokümantasyonu"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token girin. Örnek: Bearer abc123..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =====================================================
// Dependency Injection - Repositories
// =====================================================

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IVerificationRepository, VerificationRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();

// =====================================================
// Dependency Injection - Application Services
// =====================================================

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<ITutorService, TutorService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IEducationService, EducationService>();
builder.Services.AddScoped<IStudentService, StudentService>();



// =====================================================
// Dependency Injection - Admin Services
// =====================================================

builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminActionLogService, AdminActionLogService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAdminListingService, AdminListingService>();
builder.Services.AddScoped<IAdminInvitationService, AdminInvitationService>();

var app = builder.Build();

// =====================================================
// Development Tools
// =====================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =====================================================
// Middleware Pipeline
// =====================================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

// =====================================================
// Endpoints
// =====================================================

app.MapHub<ChatHub>("/chat");

app.MapControllers();

// =====================================================
// Database Migration + Seed
// =====================================================

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        db.Database.Migrate();

        await CitySeed.SeedAsync(db);
        await DistrictSeed.SeedAsync(db);
        await NeighborhoodSeed.SeedAsync(db);
        await SubjectSeed.SeedAsync(db);

        await SuperAdminSeed.SeedAsync(db, builder.Configuration);

        Console.WriteLine("Database migrated and seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration/Seed error: {ex.Message}");
    }
}

app.Run();