using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/listings")]
[Authorize(Policy = "AdminOnly")]
public class AdminListingsController : ControllerBase
{
    private readonly IAdminListingService _adminListingService;
    private readonly AppDbContext _context;

    public AdminListingsController(
        IAdminListingService adminListingService,
        AppDbContext context)
    {
        _adminListingService = adminListingService;
        _context = context;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingListings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminListingService.GetPendingListingsAsync(page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var listing = await _adminListingService.GetByIdAsync(id);

        if (listing == null)
            return NotFound(new { message = "İlan bulunamadı" });

        return Ok(listing);
    }

    // GET /api/admin/listings — all listings with optional status filter
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.TeacherListings
            .Include(x => x.TeacherProfile).ThenInclude(p => p.User)
            .Include(x => x.Subject)
            .Include(x => x.City)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status == status);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AdminListingDto
            {
                Id = x.Id,
                Title = x.Title,
                TeacherName = x.TeacherProfile.User.FullName,
                TeacherEmail = x.TeacherProfile.User.Email,
                Category = x.Category,
                SubCategory = x.SubCategory,
                Price = x.Price,
                Status = x.Status,
                IsActive = x.IsActive,
                IsApproved = x.IsApproved,
                SubjectName = x.Subject != null ? x.Subject.Name : null,
                CityName = x.City != null ? x.City.Name : null,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            items,
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminUserId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _adminListingService.ApproveAsync(id, adminUserId, ipAddress, userAgent);

        if (!result)
            return NotFound(new { message = "İlan bulunamadı" });

        return Ok(new { message = "İlan onaylandı" });
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectListingRequest request)
    {
        var adminUserId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _adminListingService.RejectAsync(
            id, request.Reason, adminUserId, ipAddress, userAgent);

        if (!result)
            return NotFound(new { message = "İlan bulunamadı" });

        return Ok(new { message = "İlan reddedildi" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            throw new UnauthorizedAccessException("Geçersiz kullanıcı tokenı");

        return userId;
    }
}