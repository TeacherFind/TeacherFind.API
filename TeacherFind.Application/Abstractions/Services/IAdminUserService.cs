using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Admin;

namespace TeacherFind.Application.Abstractions.Services;

public interface IAdminUserService
{
    Task<AdminPagedResponse<AdminUserDto>> GetUsersAsync(AdminUserQuery query);

    Task<AdminUserDto?> GetByIdAsync(Guid id);

    Task<bool> UpdateStatusAsync(
        Guid userId,
        bool isActive,
        Guid adminUserId,
        string? ipAddress,
        string? userAgent);

    Task<bool> UpdateRoleAsync(
        Guid userId,
        string role,
        Guid adminUserId,
        string? ipAddress,
        string? userAgent);

}