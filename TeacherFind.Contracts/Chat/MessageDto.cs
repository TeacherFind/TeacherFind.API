using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Chat;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string Content { get; set; } = "";
    public Guid? ReplyToMessageId { get; set; }
    public string? ReplyToMessageContent { get; set; }
    public Guid? ReplyToMessageSenderId { get; set; }
    public bool IsDeletedBySender { get; set; }
    public bool IsDeletedByReceiver { get; set; }
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
}
