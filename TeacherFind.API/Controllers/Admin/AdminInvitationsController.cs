using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/invitations")]
public class AdminInvitationsController : ControllerBase
{
    private readonly IAdminInvitationService _adminInvitationService;

    public AdminInvitationsController(IAdminInvitationService adminInvitationService)
    {
        _adminInvitationService = adminInvitationService;
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminInvitationRequest request)
    {
        var adminUserId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _adminInvitationService.CreateAsync(
            request,
            adminUserId,
            ipAddress,
            userAgent);

        if (result == null)
            return BadRequest(new { message = "Davet oluşturulamadı. Email veya rol geçersiz olabilir." });

        return Ok(result);
    }

    [Authorize(Policy = "SuperAdminOnly")]
    [HttpGet]
    public async Task<IActionResult> GetInvitations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _adminInvitationService.GetInvitationsAsync(page, pageSize);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("accept")]
    public async Task<IActionResult> Accept([FromBody] AcceptAdminInvitationRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _adminInvitationService.AcceptAsync(
            request,
            ipAddress,
            userAgent);

        if (!result)
            return BadRequest(new { message = "Davet geçersiz, süresi dolmuş veya daha önce kullanılmış." });

        return Ok(new { message = "Admin hesabı başarıyla aktif edildi." });
    }

    private Guid GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            throw new UnauthorizedAccessException("Geçersiz kullanıcı tokenı");

        return userId;
    }
}