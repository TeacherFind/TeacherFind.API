namespace TeacherFind.Contracts.Auth;

public class RequestEmailChangeDto
{
    public string NewEmail { get; set; } = default!;

    // E-posta değişikliği talebinde kullanıcının mevcut şifresi doğrulanır.
    public string Password { get; set; } = default!;
}
