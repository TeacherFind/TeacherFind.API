using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
    }

    public async Task<Message?> GetByIdAsync(Guid messageId)
    {
        return await _context.Messages
            .Include(x => x.ReplyToMessage)
            .FirstOrDefaultAsync(x => x.Id == messageId);
    }

    public async Task<List<Message>> GetConversationMessagesAsync(Guid conversationId)
    {
        return await _context.Messages
            .Include(x => x.ReplyToMessage)
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.SentAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetVisibleConversationMessagesAsync(Guid conversationId, Guid userId)
    {
        return await _context.Messages
            .Include(x => x.ReplyToMessage)
            .Where(x => x.ConversationId == conversationId &&
                        ((x.SenderId == userId && !x.IsDeletedBySender) ||
                         (x.ReceiverId == userId && !x.IsDeletedByReceiver)))
            .OrderBy(x => x.SentAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetVisibleMessagesBetweenUsersAsync(Guid currentUserId, Guid otherUserId)
    {
        return await _context.Messages
            .Include(x => x.ReplyToMessage)
            .Where(x =>
                (x.SenderId == currentUserId &&
                 x.ReceiverId == otherUserId &&
                 !x.IsDeletedBySender)
                ||
                (x.ReceiverId == currentUserId &&
                 x.SenderId == otherUserId &&
                 !x.IsDeletedByReceiver))
            .OrderBy(x => x.SentAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetMessagesForUserAsync(Guid userId, List<Guid> messageIds)
    {
        return await _context.Messages
            .Where(x => messageIds.Contains(x.Id) &&
                        (x.SenderId == userId || x.ReceiverId == userId))
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId)
    {
        return await _context.Messages
            .CountAsync(x => x.ConversationId == conversationId
                          && x.ReceiverId == userId
                          && !x.IsDeletedByReceiver
                          && !x.IsRead);
    }

    public async Task MarkConversationAsReadAsync(Guid conversationId, Guid userId)
    {
        var messages = await _context.Messages
            .Where(x => x.ConversationId == conversationId
                     && x.ReceiverId == userId
                     && !x.IsDeletedByReceiver
                     && !x.IsRead)
            .ToListAsync();

        foreach (var msg in messages)
        {
            msg.IsRead = true;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
