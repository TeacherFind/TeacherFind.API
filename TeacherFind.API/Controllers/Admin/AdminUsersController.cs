using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Admin;

namespace TeacherFind.API.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] AdminUserQuery query)
    {
        var result = await _adminUserService.GetUsersAsync(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _adminUserService.GetByIdAsync(id);

        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı" });

        return Ok(user);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateUserStatusRequest request)
    {
        var adminUserId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _adminUserService.UpdateStatusAsync(
            id,
            request.IsActive,
            adminUserId,
            ipAddress,
            userAgent);

        if (!result)
            return NotFound(new { message = "Kullanıcı bulunamadı" });

        return Ok(new { message = "Kullanıcı durumu güncellendi" });
    }

    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        [FromBody] UpdateUserRoleRequest request)
    {
        var adminUserId = GetCurrentUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();

        var result = await _adminUserService.UpdateRoleAsync(
            id,
            request.Role,
            adminUserId,
            ipAddress,
            userAgent);

        if (!result)
            return BadRequest(new { message = "Kullanıcı bulunamadı veya rol geçersiz" });

        return Ok(new { message = "Kullanıcı rolü güncellendi" });
    }

    private Guid GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            throw new UnauthorizedAccessException("Geçersiz kullanıcı tokenı");

        return userId;
    }
}