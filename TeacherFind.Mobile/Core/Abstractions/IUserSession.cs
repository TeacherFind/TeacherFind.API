using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Mobile.Core.Abstractions
{
     public interface IUserSession
    {
        Guid? UserId { get; }
        string? UserRole { get; }
        bool IsAuthenticated { get; }
        void StartSession(Guid userId, string role);
        void ClearSession();
    }
}
