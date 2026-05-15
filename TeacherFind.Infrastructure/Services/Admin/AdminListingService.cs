using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Services.Admin;

public class AdminListingService : IAdminListingService
{
    private const string PendingApprovalStatus = "PendingApproval";
    private const string ActiveStatus = "Active";
    private const string RejectedStatus = "Rejected";
    private readonly INotificationService _notificationService;

    private readonly AppDbContext _context;
    private readonly IAdminActionLogService _adminActionLogService;

    public AdminListingService(
        AppDbContext context,
        IAdminActionLogService adminActionLogService,
        INotificationService notificationService)
    {
        _context = context;
        _adminActionLogService = adminActionLogService;
        _notificationService = notificationService;
    }

    public async Task<AdminPagedResponse<AdminListingDto>> GetPendingListingsAsync(
        int page = 1,
        int pageSize = 20)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 20 : pageSize;

        var query = _context.TeacherListings
            .AsNoTracking()
            .Where(x =>
                !x.IsApproved &&
                x.IsActive &&
                x.Status == PendingApprovalStatus)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var items = await query
            .Include(x => x.TeacherProfile)
                .ThenInclude(x => x.User)
            .Include(x => x.Subject)
            .Include(x => x.City)
            .Include(x => x.District)
            .Include(x => x.Neighborhood)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminListingDto
            {
                Id = x.Id,
                TeacherProfileId = x.TeacherProfileId,
                TeacherName = x.TeacherProfile.User.FullName,
                TeacherEmail = x.TeacherProfile.User.Email,
                Title = x.Title,
                Description = x.Description,
                Category = x.Category,
                SubCategory = x.SubCategory,
                SubjectName = x.Subject != null ? x.Subject.Name : null,
                CityName = x.City != null ? x.City.Name : null,
                DistrictName = x.District != null ? x.District.Name : null,
                NeighborhoodName = x.Neighborhood != null ? x.Neighborhood.Name : null,
                ServiceType = x.ServiceType.ToString(),
                LessonDuration = x.LessonDuration,
                Price = x.Price,
                Status = x.Status,
                IsActive = x.IsActive,
                IsApproved = x.IsApproved,
                ViewCount = x.ViewCount,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return new AdminPagedResponse<AdminListingDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<AdminListingDto?> GetByIdAsync(Guid id)
    {
        return await _context.TeacherListings
            .AsNoTracking()
            .Include(x => x.TeacherProfile)
                .ThenInclude(x => x.User)
            .Include(x => x.Subject)
            .Include(x => x.City)
            .Include(x => x.District)
            .Include(x => x.Neighborhood)
            .Where(x => x.Id == id)
            .Select(x => new AdminListingDto
            {
                Id = x.Id,
                TeacherProfileId = x.TeacherProfileId,
                TeacherName = x.TeacherProfile.User.FullName,
                TeacherEmail = x.TeacherProfile.User.Email,
                Title = x.Title,
                Description = x.Description,
                Category = x.Category,
                SubCategory = x.SubCategory,
                SubjectName = x.Subject != null ? x.Subject.Name : null,
                CityName = x.City != null ? x.City.Name : null,
                DistrictName = x.District != null ? x.District.Name : null,
                NeighborhoodName = x.Neighborhood != null ? x.Neighborhood.Name : null,
                ServiceType = x.ServiceType.ToString(),
                LessonDuration = x.LessonDuration,
                Price = x.Price,
                Status = x.Status,
                IsActive = x.IsActive,
                IsApproved = x.IsApproved,
                ViewCount = x.ViewCount,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ApproveAsync(
        Guid listingId,
        Guid adminUserId,
        string? ipAddress,
        string? userAgent)
    {
        var listing = await _context.TeacherListings
            .Include(x => x.TeacherProfile)
            .FirstOrDefaultAsync(x => x.Id == listingId);

        if (listing is null)
            return false;

        listing.IsApproved = true;
        listing.IsActive = true;
        listing.Status = ActiveStatus;
        await _notificationService.SendNotificationAsync(
    listing.TeacherProfile.UserId,
    "İlanınız onaylandı",
    $"{listing.Title} başlıklı ilanınız onaylandı ve yayına alındı.",
    "Listing",
    adminUserId,
    "Admin",
    $"/tutor/listings/{listing.Id}");
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _adminActionLogService.LogAsync(
            adminUserId,
            "ApproveListing",
            "TeacherListing",
            listing.Id,
            "Admin ilanın durumunu Active yaptı.",
            ipAddress,
            userAgent);

        return true;
    }

    public async Task<bool> RejectAsync(
        Guid listingId,
        string? reason,
        Guid adminUserId,
        string? ipAddress,
        string? userAgent)
    {
        var listing = await _context.TeacherListings
            .Include(x => x.TeacherProfile)
            .FirstOrDefaultAsync(x => x.Id == listingId);

        if (listing is null)
            return false;

        listing.IsApproved = false;
        listing.IsActive = false;
        listing.Status = RejectedStatus;
        await _notificationService.SendNotificationAsync(
    listing.TeacherProfile.UserId,
    "İlanınız reddedildi",
    string.IsNullOrWhiteSpace(reason)
        ? $"{listing.Title} başlıklı ilanınız reddedildi."
        : $"{listing.Title} başlıklı ilanınız reddedildi. Sebep: {reason}",
    "Listing",
    adminUserId,
    "Admin",
    $"/tutor/listings/{listing.Id}");
        listing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _adminActionLogService.LogAsync(
            adminUserId,
            "RejectListing",
            "TeacherListing",
            listing.Id,
            string.IsNullOrWhiteSpace(reason)
                ? "Admin ilanın durumunu Rejected yaptı."
                : $"Admin ilanın durumunu Rejected yaptı. Sebep: {reason}",
            ipAddress,
            userAgent);

        return true;
    }
}