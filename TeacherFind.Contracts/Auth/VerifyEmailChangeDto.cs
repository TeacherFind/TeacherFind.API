namespace TeacherFind.Contracts.Auth;

public class VerifyEmailChangeDto
{
    public string NewEmail { get; set; } = default!;

    public string Code { get; set; } = default!;
}
