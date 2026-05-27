using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeacherFind.Mobile.Core.Abstractions;

namespace TeacherFind.Mobile.Core.Services;

public class UserSessionService : IUserSession
{
    public Guid? UserId { get; private set; }
    public string? UserRole { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;

    public void StartSession(Guid userId, string role)
    {
        UserId = userId;
        UserRole = role;
    }

    public void ClearSession()
    {
        UserId = null;
        UserRole = null;
    }
}
