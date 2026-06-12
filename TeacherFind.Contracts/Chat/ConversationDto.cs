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
    public string OtherUserName { get; set; } = "Kullanıcı";
    public bool OtherUserIsOnline { get; set; }
    public DateTime? OtherUserLastSeenAt { get; set; }
    public string DebugVersion { get; set; } = "chat-name-fix-2026-06-11";
    public string LastMessage { get; set; } = "";
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}
