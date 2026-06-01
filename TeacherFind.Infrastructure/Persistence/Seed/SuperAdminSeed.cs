using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TeacherFind.Domain.Entities;
using TeacherFind.Domain.Enums;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Persistence.Seed;

public static class SuperAdminSeed
{
    public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
    {
        var email = configuration["SuperAdmin:Email"];
        var password = configuration["SuperAdmin:Password"];
        var fullName = configuration["SuperAdmin:FullName"] ?? "System Owner";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var superAdminExists = await context.Users
            .AnyAsync(x => x.Role == UserRole.SuperAdmin);

        if (superAdminExists)
            return;

        var existingUser = await context.Users
            .FirstOrDefaultAsync(x => x.Email == email);

        if (existingUser is not null)
        {
            existingUser.Role = UserRole.SuperAdmin;
            existingUser.IsActive = true;
            existingUser.IsEmailVerified = true;
        }
        else
        {
            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = UserRole.SuperAdmin,
                IsActive = true,
                IsEmailVerified = true
            };

            await context.Users.AddAsync(user);
        }

        await context.SaveChangesAsync();
    }
}