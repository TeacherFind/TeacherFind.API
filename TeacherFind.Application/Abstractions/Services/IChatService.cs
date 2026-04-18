using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Chat;

namespace TeacherFind.Application.Abstractions.Services;

public interface IChatService
{
    Task<MessageDto> SendMessageAsync(Guid senderId, SendMessageDto request);
    Task<List<MessageDto>> GetMessagesAsync(Guid conversationId, Guid currentUserId);
    Task<List<ConversationDto>> GetMyConversationsAsync(Guid currentUserId);
    Task MarkAsReadAsync(Guid conversationId, Guid currentUserId);
}
