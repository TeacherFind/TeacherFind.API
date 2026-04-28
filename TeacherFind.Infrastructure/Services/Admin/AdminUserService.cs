using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;
using TeacherFind.Domain.Enums;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Services.Admin;

public class AdminUserService : IAdminUserService
{
    private readonly AppDbContext _context;
    private readonly IAdminActionLogService _adminActionLogService;

    public AdminUserService(
        AppDbContext context,
        IAdminActionLogService adminActionLogService)
    {
        _context = context;
        _adminActionLogService = adminActionLogService;
    }

    public async Task<AdminPagedResponse<AdminUserDto>> GetUsersAsync(AdminUserQuery query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var usersQuery = _context.Users
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            usersQuery = usersQuery.Where(x =>
                x.FullName.Contains(query.Search) ||
                x.Email.Contains(query.Search));
        }

        if (!string.IsNullOrWhiteSpace(query.Role) &&
            Enum.TryParse<UserRole>(query.Role, true, out var role))
        {
            usersQuery = usersQuery.Where(x => x.Role == role);
        }

        if (query.IsActive.HasValue)
        {
            usersQuery = usersQuery.Where(x => x.IsActive == query.IsActive.Value);
        }

        var totalCount = await usersQuery.CountAsync();

        var items = await usersQuery
            .OrderBy(x => x.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminUserDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Role = x.Role.ToString(),
                IsActive = x.IsActive,
                IsEmailVerified = x.IsEmailVerified,
                IsPhoneVerified = x.IsPhoneVerified,
                PhoneNumber = x.PhoneNumber
            })
            .ToListAsync();

        return new AdminPagedResponse<AdminUserDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<AdminUserDto?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AdminUserDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email,
                Role = x.Role.ToString(),
                IsActive = x.IsActive,
                IsEmailVerified = x.IsEmailVerified,
                IsPhoneVerified = x.IsPhoneVerified,
                PhoneNumber = x.PhoneNumber
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateStatusAsync(
        Guid userId,
        bool isActive,
        Guid adminUserId,
        string? ipAddress,
        string? userAgent)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user is null)
            return false;

        user.IsActive = isActive;

        await _context.SaveChangesAsync();

        await _adminActionLogService.LogAsync(
            adminUserId,
            isActive ? "ActivateUser" : "DeactivateUser",
            "User",
            user.Id,
            $"User status changed. IsActive = {isActive}",
            ipAddress,
            userAgent);

        return true;
    }

    public async Task<bool> UpdateRoleAsync(
        Guid userId,
        string role,
        Guid adminUserId,
        string? ipAddress,
        string? userAgent)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

        if (user is null)
            return false;

        if (!Enum.TryParse<UserRole>(role, true, out var parsedRole))
            return false;

        if (parsedRole is not UserRole.Student
            and not UserRole.Tutor
            and not UserRole.Admin
            and not UserRole.SuperAdmin)
        {
            return false;
        }

        user.Role = parsedRole;

        await _context.SaveChangesAsync();

        await _adminActionLogService.LogAsync(
            adminUserId,
            "ChangeUserRole",
            "User",
            user.Id,
            $"User role changed to {parsedRole}",
            ipAddress,
            userAgent);

        return true;
    }
}