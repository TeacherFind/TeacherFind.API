using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TeacherFind.API.Hubs;
using TeacherFind.Application.Abstractions.Services;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IHubContext<ChatHub> _chatHubContext;

    public UsersController(
        IChatService chatService,
        IHubContext<ChatHub> chatHubContext)
    {
        _chatService = chatService;
        _chatHubContext = chatHubContext;
    }

    // POST /api/users/heartbeat
    [HttpPost("heartbeat")]
    public async Task<IActionResult> Heartbeat()
    {
        var userId = GetUserId();
        var lastSeenAt = DateTime.UtcNow;

        await _chatService.UpdateUserPresenceAsync(userId, true, lastSeenAt);

        await _chatHubContext.Clients.All.SendAsync("UserStatusChanged", new
        {
            userId,
            isOnline = true,
            lastSeenAt
        });

        return Ok(new
        {
            userId,
            isOnline = true,
            lastSeenAt
        });
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
