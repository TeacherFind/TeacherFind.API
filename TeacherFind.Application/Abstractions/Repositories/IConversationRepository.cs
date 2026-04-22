using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetBetweenUsersAsync(Guid user1Id, Guid user2Id);
    Task<Conversation?> GetByIdAsync(Guid conversationId);
    Task<List<Conversation>> GetUserConversationsAsync(Guid userId);
    Task AddAsync(Conversation conversation);
    Task SaveChangesAsync();
}
