using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Contracts.Devices;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private static readonly string[] AllowedPlatforms = { "web", "android", "ios" };

    private readonly IUserDeviceRepository _userDeviceRepository;

    public DevicesController(IUserDeviceRepository userDeviceRepository)
        => _userDeviceRepository = userDeviceRepository;

    // POST /api/devices/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDeviceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FcmToken))
            return BadRequest(new { message = "FCM token boş olamaz." });

        var platform = request.Platform?.Trim().ToLowerInvariant() ?? "";

        if (!AllowedPlatforms.Contains(platform))
            return BadRequest(new { message = "Geçersiz platform. Geçerli değerler: web, android, ios." });

        await _userDeviceRepository.AddOrUpdateAsync(GetUserId(), request.FcmToken, platform);
        await _userDeviceRepository.SaveChangesAsync();

        return Ok(new { message = "Cihaz kaydedildi." });
    }

    // DELETE /api/devices
    [HttpDelete]
    public async Task<IActionResult> Unregister([FromBody] UnregisterDeviceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FcmToken))
            return BadRequest(new { message = "FCM token boş olamaz." });

        await _userDeviceRepository.DeleteUserTokenAsync(GetUserId(), request.FcmToken);
        await _userDeviceRepository.SaveChangesAsync();

        return Ok(new { message = "Cihaz kaydı silindi." });
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}