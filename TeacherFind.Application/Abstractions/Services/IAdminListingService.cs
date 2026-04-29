using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Admin;

namespace TeacherFind.Application.Abstractions.Services;

public interface IAdminListingService
{
    Task<AdminPagedResponse<AdminListingDto>> GetPendingListingsAsync(
        int page = 1,
        int pageSize = 20);

    Task<AdminListingDto?> GetByIdAsync(Guid id);

    Task<bool> ApproveAsync(
        Guid listingId,
        Guid adminUserId,
        string? ipAddress,
        string? userAgent);

    Task<bool> RejectAsync(
        Guid listingId,
        string? reason,
        Guid adminUserId,
        string? ipAddress,
        string? userAgent);

}