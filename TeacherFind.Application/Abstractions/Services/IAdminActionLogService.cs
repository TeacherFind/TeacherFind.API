using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Admin;

namespace TeacherFind.Application.Abstractions.Services;

public interface IAdminActionLogService
{
    Task LogAsync(
        Guid adminUserId,
        string action,
        string entityName,
        Guid? entityId,
        string? description,
        string? ipAddress,
        string? userAgent);

    Task<AdminPagedResponse<AdminActionLogDto>> GetLogsAsync(AdminActionLogQuery query);
}
