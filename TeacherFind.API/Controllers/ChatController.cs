using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetMyConversations()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _chatService.GetMyConversationsAsync(userId);
        return Ok(result);
    }

    [HttpGet("messages/{conversationId:guid}")]
    public async Task<IActionResult> GetMessages(Guid conversationId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _chatService.GetMessagesAsync(conversationId, userId);
        return Ok(result);
    }

    [HttpPost("read/{conversationId:guid}")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        await _chatService.MarkAsReadAsync(conversationId, userId);
        return Ok(new { message = "Mesajlar okundu olarak işaretlendi." });
    }
}