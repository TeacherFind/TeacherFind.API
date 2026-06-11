using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Chat;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Chat;

public class ChatService : IChatService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public ChatService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageDto request)
    {
        // CORRECT
        var sender = await _userRepository.GetByIdAsync(senderId)
            ?? throw new Exception("Gönderen kullanıcı bulunamadı.");

        var receiver = await _userRepository.GetByIdAsync(request.ReceiverId)
            ?? throw new Exception("Alıcı kullanıcı bulunamadı.");

        if (senderId == request.ReceiverId)
            throw new Exception("Kendinize mesaj atamazsınız.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new Exception("Mesaj boş olamaz.");

        var conversation = await _conversationRepository
            .GetBetweenUsersAsync(senderId, request.ReceiverId);

        if (conversation == null)
        {
            conversation = new Conversation { User1Id = senderId, User2Id = request.ReceiverId };
            await _conversationRepository.AddAsync(conversation);
            await _conversationRepository.SaveChangesAsync();
        }

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = senderId,
            ReceiverId = request.ReceiverId,
            Content = request.Content.Trim()
        };

        await _messageRepository.AddAsync(message);
        await _messageRepository.SaveChangesAsync();
        try
        {
            await _notificationService.SendNotificationAsync(
                request.ReceiverId,
                "Yeni mesaj",
                $"{sender.FullName} size yeni bir mesaj gönderdi.",
                "Message",
                senderId,
                sender.FullName,
                $"/messages/{senderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Notification error while sending message: {ex.Message}");
        }

        return Map(message);
    }

    public async Task<List<MessageDto>> GetMessagesAsync(Guid conversationId, Guid currentUserId)
    {
        var messages = await _messageRepository.GetConversationMessagesAsync(conversationId);
        return messages.Select(Map).ToList();
    }

    // Task 8 — GET /api/messages/{userId}
    public async Task<List<MessageDto>> GetMessagesByUserIdAsync(Guid currentUserId, Guid otherUserId)
    {
        var conversation = await _conversationRepository
            .GetBetweenUsersAsync(currentUserId, otherUserId);

        if (conversation is null)
            return new List<MessageDto>();

        var messages = await _messageRepository
            .GetConversationMessagesAsync(conversation.Id);

        // Auto mark as read when messages are fetched
        await _messageRepository.MarkConversationAsReadAsync(conversation.Id, currentUserId);
        await _messageRepository.SaveChangesAsync();

        return messages.Select(Map).ToList();
    }

    public async Task<List<ConversationDto>> GetMyConversationsAsync(Guid currentUserId)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(currentUserId);
        var result = new List<ConversationDto>();

        foreach (var conv in conversations)
        {
            var otherUserId = conv.User1Id == currentUserId ? conv.User2Id : conv.User1Id;
            var otherUser = await _userRepository.GetByIdAsync(otherUserId);
            var messages = conv.Messages.OrderByDescending(x => x.SentAt).ToList();
            var last = messages.FirstOrDefault();
            var unread = messages.Count(x => x.ReceiverId == currentUserId && !x.IsRead);

            result.Add(new ConversationDto
            {
                ConversationId = conv.Id,
                OtherUserId = otherUserId,
                OtherUserName = ResolveDisplayName(otherUser),
                DebugVersion = "chat-name-fix-2026-06-11",
                LastMessage = last?.Content ?? "",
                LastMessageAt = last?.SentAt,
                UnreadCount = unread
            });
        }

        return result.OrderByDescending(x => x.LastMessageAt).ToList();
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid currentUserId)
    {
        await _messageRepository.MarkConversationAsReadAsync(conversationId, currentUserId);
        await _messageRepository.SaveChangesAsync();
    }

    private static MessageDto Map(Message m) => new()
    {
        Id = m.Id,
        ConversationId = m.ConversationId,
        SenderId = m.SenderId,
        ReceiverId = m.ReceiverId,
        Content = m.Content,
        IsRead = m.IsRead,
        SentAt = m.SentAt
    };

    private static string ResolveDisplayName(User? user)
    {
        if (user is null)
            return "Kullanıcı";

        var fullName = NormalizeName(user.FullName);
        if (fullName is not null)
            return fullName;

        var name = GetStringProperty(user, "Name");
        if (name is not null)
            return name;

        var firstName = GetStringProperty(user, "FirstName");
        var lastName = GetStringProperty(user, "LastName");
        var firstAndLastName = NormalizeName($"{firstName} {lastName}");
        if (firstAndLastName is not null)
            return firstAndLastName;

        return GetStringProperty(user, "UserName")
            ?? NormalizeName(user.Email)
            ?? "Kullanıcı";
    }

    private static string? GetStringProperty(User user, string propertyName)
        => NormalizeName(user.GetType().GetProperty(propertyName)?.GetValue(user) as string);

    private static string? NormalizeName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }
}
