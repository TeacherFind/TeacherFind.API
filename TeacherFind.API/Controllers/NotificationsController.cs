using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
        => _notificationService = notificationService;

    // GET /api/notifications
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications()
    {
        var result = await _notificationService.GetMyNotificationsAsync(GetUserId());
        return Ok(result);
    }

    // PUT /api/notifications/{id}/read
    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var success = await _notificationService.MarkAsReadAsync(id, GetUserId());

        if (!success)
            return NotFound(new { message = "Bildirim bulunamadı veya bu bildirim size ait değil." });

        return Ok(new { message = "Bildirim okundu olarak işaretlendi." });
    }

    // PUT /api/notifications/read-all
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(GetUserId());
        return Ok(new { message = "Tüm bildirimler okundu olarak işaretlendi." });
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}