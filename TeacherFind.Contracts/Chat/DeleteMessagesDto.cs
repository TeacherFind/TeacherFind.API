namespace TeacherFind.Contracts.Chat;

public class DeleteMessagesDto
{
    public List<Guid> MessageIds { get; set; } = new();
}
