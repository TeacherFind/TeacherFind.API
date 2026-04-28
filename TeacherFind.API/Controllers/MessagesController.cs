using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Chat;

namespace TeacherFind.API.Controllers;

[ApiController]
[Route("api/messages")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IChatService _chatService;

    public MessagesController(IChatService chatService) => _chatService = chatService;

    // GET /api/messages/conversations
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var result = await _chatService.GetMyConversationsAsync(GetUserId());
        return Ok(result);
    }

    // GET /api/messages/{userId}
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetMessages(Guid userId)
    {
        var messages = await _chatService.GetMessagesByUserIdAsync(GetUserId(), userId);
        return Ok(messages);
    }

    // POST /api/messages  { receiverId, content }
    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        if (dto is null)
            return BadRequest(new { message = "İstek boş olamaz." });

        try
        {
            var message = await _chatService.SendMessageAsync(GetUserId(), dto);
            return Ok(message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private Guid GetUserId()
        => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}