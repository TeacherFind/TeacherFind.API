using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Chat;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Chat;

public class ChatService : IChatService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;

    public ChatService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
    }

    public async Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageDto request)
    {
        var conversation = await _conversationRepository
            .GetBetweenUsersAsync(senderId, request.ReceiverId);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                User1Id = senderId,
                User2Id = request.ReceiverId
            };

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

        return new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            ReceiverId = message.ReceiverId,
            Content = message.Content,
            IsRead = message.IsRead,
            SentAt = message.SentAt
        };
    }

    public async Task<List<MessageDto>> GetMessagesAsync(Guid conversationId, Guid currentUserId)
    {
        var messages = await _messageRepository.GetConversationMessagesAsync(conversationId);

        return messages.Select(x => new MessageDto
        {
            Id = x.Id,
            ConversationId = x.ConversationId,
            SenderId = x.SenderId,
            ReceiverId = x.ReceiverId,
            Content = x.Content,
            IsRead = x.IsRead,
            SentAt = x.SentAt
        }).ToList();
    }

    public async Task<List<ConversationDto>> GetMyConversationsAsync(Guid currentUserId)
    {
        var conversations = await _conversationRepository.GetUserConversationsAsync(currentUserId);
        var result = new List<ConversationDto>();

        foreach (var conversation in conversations)
        {
            var otherUserId = conversation.User1Id == currentUserId
                ? conversation.User2Id
                : conversation.User1Id;

            var messages = conversation.Messages
                .OrderByDescending(x => x.SentAt)
                .ToList();

            var lastMessage = messages.FirstOrDefault();
            var unreadCount = messages.Count(x => x.ReceiverId == currentUserId && !x.IsRead);

            result.Add(new ConversationDto
            {
                ConversationId = conversation.Id,
                OtherUserId = otherUserId,
                LastMessage = lastMessage?.Content ?? "",
                LastMessageAt = lastMessage?.SentAt,
                UnreadCount = unreadCount
            });
        }

        return result.OrderByDescending(x => x.LastMessageAt).ToList();
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid currentUserId)
    {
        await _messageRepository.MarkConversationAsReadAsync(conversationId, currentUserId);
        await _messageRepository.SaveChangesAsync();
    }
}