using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using TeacherFind.API.Hubs;
using TeacherFind.API.Middleware;
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
using TeacherFind.Infrastructure.Services.Admin;
using TeacherFind.Infrastructure.Services.Education;
using TeacherFind.Infrastructure.Services.Email;

internal class Program
{
    private static async Task Main(string[] args)
    {
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
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));

        // =====================================================
        // Forwarded Headers
        // =====================================================

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;

            options.ForwardLimit = 1;
        });

        // =====================================================
        // JWT Authentication
        // =====================================================

        var jwtKey = builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key bulunamadı.");

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = builder.Environment.IsProduction(),
                    ValidateAudience = builder.Environment.IsProduction(),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.Zero
                };

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
        // Rate Limiting
        // =====================================================

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                await context.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        message = "Çok fazla istek gönderdiniz. Lütfen biraz sonra tekrar deneyin."
                    }),
                    cancellationToken);
            };

            options.AddPolicy("auth-limit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            options.AddPolicy("register-limit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            options.AddPolicy("public-read-limit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            options.AddPolicy("PublicListPolicy", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            options.AddPolicy("WritePolicy", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetClientIp(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));
        });

        static string GetClientIp(HttpContext context)
            => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // =====================================================
        // Authorization Policies
        // =====================================================

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student", "Admin", "SuperAdmin"));
            options.AddPolicy("TutorOnly", policy => policy.RequireRole("Tutor", "Admin", "SuperAdmin"));
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin", "SuperAdmin"));
            options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
        });

        // =====================================================
        // CORS
        // =====================================================

        var allowedOriginsFromConfig = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        var allOrigins = allowedOriginsFromConfig.Concat(new[]
        {
            "http://localhost:5173",
            "https://localhost:5173",
            "http://localhost:3000",
            "https://localhost:3000"
        }).Distinct().ToArray();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .WithOrigins(allOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // =====================================================
        // Framework Services
        // =====================================================

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            });

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value!.Errors.Select(e => e.ErrorMessage).ToList());

                return new BadRequestObjectResult(new
                {
                    message = "İstek doğrulanamadı.",
                    errors
                });
            };
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSignalR();
        builder.Services.AddHttpContextAccessor();

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
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // =====================================================
        // Dependency Injection — Repositories
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
        // Dependency Injection — Application Services
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
        builder.Services.AddScoped<IEducationService, EducationService>();
        builder.Services.AddScoped<IStudentService, StudentService>();

        // =====================================================
        // Email Service
        // =====================================================

        builder.Services.Configure<EmailOptions>(
            builder.Configuration.GetSection("Email"));

        builder.Services.AddHttpClient<IEmailService, BrevoEmailService>();

        // =====================================================
        // Dependency Injection — Admin Services
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

        app.UseForwardedHeaders();
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("Frontend");
        app.UseRateLimiter();
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
                await UniversitySeed.SeedAsync(db);
                await DepartmentSeed.SeedAsync(db);

                await SuperAdminSeed.SeedAsync(db, builder.Configuration);

                Console.WriteLine("Database migrated and seeded successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Migration/Seed error: {ex.Message}");
            }
        }

        app.Run();
    }
}