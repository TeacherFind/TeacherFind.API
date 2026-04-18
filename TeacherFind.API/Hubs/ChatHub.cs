using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Chat;

namespace TeacherFind.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(SendMessageDto request)
    {
        var senderIdValue = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(senderIdValue))
            throw new HubException("Kullanıcı doğrulanamadı.");

        var senderId = Guid.Parse(senderIdValue);

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new HubException("Mesaj boş olamaz.");

        var message = await _chatService.SendMessageAsync(senderId, request);

        await Clients.Group($"user-{message.ReceiverId}")
            .SendAsync("ReceiveMessage", message);

        await Clients.Group($"user-{message.SenderId}")
            .SendAsync("ReceiveMessage", message);
    }

    public async Task MarkConversationAsRead(Guid conversationId)
    {
        var userIdValue = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdValue))
            throw new HubException("Kullanıcı doğrulanamadı.");

        await _chatService.MarkAsReadAsync(conversationId, Guid.Parse(userIdValue));
    }
}