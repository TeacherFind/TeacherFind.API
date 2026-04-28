using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Admin;

namespace TeacherFind.Application.Abstractions.Services;

public interface IAdminInvitationService
{
    Task<AdminInvitationCreatedResponse?> CreateAsync(
        CreateAdminInvitationRequest request,
        Guid invitedByUserId,
        string? ipAddress,
        string? userAgent);

    Task<AdminPagedResponse<AdminInvitationDto>> GetInvitationsAsync(
        int page = 1,
        int pageSize = 20);

    Task<bool> AcceptAsync(
        AcceptAdminInvitationRequest request,
        string? ipAddress,
        string? userAgent);
}
