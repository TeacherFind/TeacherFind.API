using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/listings")]
[Authorize(Policy = "AdminOnly")]
public class AdminListingsController : ControllerBase
{
    private readonly IAdminListingService _adminListingService;

    public AdminListingsController(IAdminListingService adminListingService)
    {
        _adminListingService = adminListingService;
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

    [HttpPut("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var adminUserId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _adminListingService.ApproveAsync(
            id,
            adminUserId,
            ipAddress,
            userAgent);

        if (!result)
            return NotFound(new { message = "İlan bulunamadı" });

        return Ok(new { message = "İlan onaylandı" });
    }

    [HttpPut("{id:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] RejectListingRequest request)
    {
        var adminUserId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _adminListingService.RejectAsync(
            id,
            request.Reason,
            adminUserId,
            ipAddress,
            userAgent);

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