using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Chat;

namespace TeacherFind.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly ConcurrentDictionary<Guid, int> UserConnectionCounts = new();

    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userId, out var parsedUserId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{parsedUserId}");

            var lastSeenAt = DateTime.UtcNow;
            UserConnectionCounts.AddOrUpdate(parsedUserId, 1, (_, count) => count + 1);
            await _chatService.UpdateUserPresenceAsync(parsedUserId, true, lastSeenAt);

            await Clients.All.SendAsync("UserStatusChanged", new
            {
                userId = parsedUserId,
                isOnline = true,
                lastSeenAt
            });
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userId, out var parsedUserId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{parsedUserId}");

            var remainingConnections = UserConnectionCounts.AddOrUpdate(
                parsedUserId,
                0,
                (_, count) => Math.Max(0, count - 1));

            if (remainingConnections == 0)
            {
                UserConnectionCounts.TryRemove(parsedUserId, out _);

                var lastSeenAt = DateTime.UtcNow;
                await _chatService.UpdateUserPresenceAsync(parsedUserId, false, lastSeenAt);

                await Clients.All.SendAsync("UserStatusChanged", new
                {
                    userId = parsedUserId,
                    isOnline = false,
                    lastSeenAt
                });
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(SendMessageDto request)
    {
        var senderIdValue = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(senderIdValue))
            throw new HubException("Kullanıcı doğrulanamadı.");

        var senderId = Guid.Parse(senderIdValue);

        //  mesajı kaydet
        var message = await _chatService.SendMessageAsync(senderId, request);

        //  karşı tarafa mesaj gönder
        await Clients.Group($"user-{message.ReceiverId}")
            .SendAsync("ReceiveMessage", message);

        //  karşı tarafa notification gönder
        await Clients.Group($"user-{message.ReceiverId}")
            .SendAsync("ReceiveNotification", new
            {
                title = "Yeni mesaj",
                message = message.Content
            });

        //  kendine de gönder
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
