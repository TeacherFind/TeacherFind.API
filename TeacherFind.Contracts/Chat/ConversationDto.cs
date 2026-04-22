using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Chat;

public class ConversationDto
{
    public Guid ConversationId { get; set; }
    public Guid OtherUserId { get; set; }
    public string LastMessage { get; set; } = "";
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}