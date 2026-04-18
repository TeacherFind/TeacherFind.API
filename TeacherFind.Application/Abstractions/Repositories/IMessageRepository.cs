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
    Task<List<Message>> GetConversationMessagesAsync(Guid conversationId);
    Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId);
    Task MarkConversationAsReadAsync(Guid conversationId, Guid userId);
    Task SaveChangesAsync();
}