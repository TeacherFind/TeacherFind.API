using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Identity;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;
using TeacherFind.Domain.Entities;
using TeacherFind.Domain.Enums;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Services.Admin;

public class AdminInvitationService : IAdminInvitationService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAdminActionLogService _adminActionLogService;

    public AdminInvitationService(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        IAdminActionLogService adminActionLogService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _adminActionLogService = adminActionLogService;
    }

    public async Task<AdminInvitationCreatedResponse?> CreateAsync(
        CreateAdminInvitationRequest request,
        Guid invitedByUserId,
        string? ipAddress,
        string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return null;

        if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
            return null;

        if (role is not UserRole.Admin and not UserRole.SuperAdmin)
            return null;

        var email = request.Email.Trim().ToLowerInvariant();

        var existingActiveInvitation = await _context.AdminInvitations
            .AnyAsync(x =>
                x.Email == email &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow);

        if (existingActiveInvitation)
            return null;

        var token = GenerateSecureToken();
        var tokenHash = HashToken(token);

        var invitation = new AdminInvitation
        {
            Email = email,
            Role = role,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            IsUsed = false,
            InvitedByUserId = invitedByUserId
        };

        await _context.AdminInvitations.AddAsync(invitation);
        await _context.SaveChangesAsync();

        await _adminActionLogService.LogAsync(
            invitedByUserId,
            "CreateAdminInvitation",
            "AdminInvitation",
            invitation.Id,
            $"Admin invitation created for {email} with role {role}.",
            ipAddress,
            userAgent);

        return new AdminInvitationCreatedResponse
        {
            InvitationId = invitation.Id,
            Email = invitation.Email,
            Role = invitation.Role.ToString(),
            ExpiresAt = invitation.ExpiresAt,
            Token = token,
            InvitationUrl = $"/admin/accept-invitation?token={Uri.EscapeDataString(token)}"
        };
    }

    public async Task<AdminPagedResponse<AdminInvitationDto>> GetInvitationsAsync(
        int page = 1,
        int pageSize = 20)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var query = _context.AdminInvitations
            .AsNoTracking()
            .Include(x => x.InvitedByUser)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.ExpiresAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminInvitationDto
            {
                Id = x.Id,
                Email = x.Email,
                Role = x.Role.ToString(),
                ExpiresAt = x.ExpiresAt,
                IsUsed = x.IsUsed,
                UsedAt = x.UsedAt,
                InvitedByUserId = x.InvitedByUserId,
                InvitedByFullName = x.InvitedByUser.FullName,
                InvitedByEmail = x.InvitedByUser.Email
            })
            .ToListAsync();

        return new AdminPagedResponse<AdminInvitationDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<bool> AcceptAsync(
        AcceptAdminInvitationRequest request,
        string? ipAddress,
        string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(request.Token) ||
            string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return false;
        }

        var tokenHash = HashToken(request.Token);

        var invitation = await _context.AdminInvitations
            .FirstOrDefaultAsync(x =>
                x.TokenHash == tokenHash &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow);

        if (invitation is null)
            return false;

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == invitation.Email);

        if (existingUser is not null)
        {
            existingUser.FullName = string.IsNullOrWhiteSpace(existingUser.FullName)
                ? request.FullName.Trim()
                : existingUser.FullName;

            existingUser.PasswordHash = _passwordHasher.Hash(request.Password);
            existingUser.Role = invitation.Role;
            existingUser.IsActive = true;
            existingUser.IsEmailVerified = true;
        }
        else
        {
            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = invitation.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = invitation.Role,
                IsActive = true,
                IsEmailVerified = true,
                IsPhoneVerified = false
            };

            await _context.Users.AddAsync(user);
        }

        invitation.IsUsed = true;
        invitation.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _adminActionLogService.LogAsync(
            invitation.InvitedByUserId,
            "AcceptAdminInvitation",
            "AdminInvitation",
            invitation.Id,
            $"Admin invitation accepted by {invitation.Email}.",
            ipAddress,
            userAgent);

        return true;
    }

    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private static string HashToken(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}