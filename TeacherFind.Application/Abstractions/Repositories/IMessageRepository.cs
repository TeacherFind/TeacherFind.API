using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<Message?> GetByIdAsync(Guid messageId);
    Task<List<Message>> GetConversationMessagesAsync(Guid conversationId);
    Task<List<Message>> GetVisibleConversationMessagesAsync(Guid conversationId, Guid userId);
    Task<List<Message>> GetVisibleMessagesBetweenUsersAsync(Guid currentUserId, Guid otherUserId);
    Task<List<Message>> GetMessagesForUserAsync(Guid userId, List<Guid> messageIds);
    Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId);
    Task MarkConversationAsReadAsync(Guid conversationId, Guid userId);
    Task SaveChangesAsync();
}
