using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Auth;

public class MeResponse
{
    public Guid UserId { get; set; }

    public string FullName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string Role { get; set; } = default!;

    public string? AvatarUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public Guid? CityId { get; set; }

    public string? CityName { get; set; }

}
