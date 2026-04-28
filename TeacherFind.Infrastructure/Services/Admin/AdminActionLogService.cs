using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Services.Admin;

public class AdminActionLogService : IAdminActionLogService
{
    private readonly AppDbContext _context;

    public AdminActionLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        Guid adminUserId,
        string action,
        string entityName,
        Guid? entityId,
        string? description,
        string? ipAddress,
        string? userAgent)
    {
        var log = new AdminActionLog
        {
            AdminUserId = adminUserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Description = description,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        await _context.AdminActionLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<AdminPagedResponse<AdminActionLogDto>> GetLogsAsync(AdminActionLogQuery query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;

        var logsQuery = _context.AdminActionLogs
            .AsNoTracking()
            .Include(x => x.AdminUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            logsQuery = logsQuery.Where(x => x.Action.Contains(query.Action));
        }

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            logsQuery = logsQuery.Where(x => x.EntityName.Contains(query.EntityName));
        }

        if (query.AdminUserId.HasValue)
        {
            logsQuery = logsQuery.Where(x => x.AdminUserId == query.AdminUserId.Value);
        }

        var totalCount = await logsQuery.CountAsync();

        var items = await logsQuery
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminActionLogDto
            {
                Id = x.Id,
                AdminUserId = x.AdminUserId,
                AdminFullName = x.AdminUser.FullName,
                AdminEmail = x.AdminUser.Email,
                Action = x.Action,
                EntityName = x.EntityName,
                EntityId = x.EntityId,
                Description = x.Description,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent
            })
            .ToListAsync();

        return new AdminPagedResponse<AdminActionLogDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }
}