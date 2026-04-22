using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetBetweenUsersAsync(Guid user1Id, Guid user2Id)
    {
        return await _context.Conversations
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x =>
                (x.User1Id == user1Id && x.User2Id == user2Id) ||
                (x.User1Id == user2Id && x.User2Id == user1Id));
    }

    public async Task<Conversation?> GetByIdAsync(Guid conversationId)
    {
        return await _context.Conversations
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == conversationId);
    }

    public async Task<List<Conversation>> GetUserConversationsAsync(Guid userId)
    {
        return await _context.Conversations
            .Include(x => x.Messages)
            .Where(x => x.User1Id == userId || x.User2Id == userId)
            .ToListAsync();
    }

    public async Task AddAsync(Conversation conversation)
    {
        await _context.Conversations.AddAsync(conversation);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}