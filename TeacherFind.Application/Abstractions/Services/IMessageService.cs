using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Services
{
    public interface IMessageService
    {
        Task SendMessageAsync(Guid senderId, Guid receiverId, string content);

        Task<List<Message>> GetConversationAsync(Guid user1, Guid user2);
    }
}
