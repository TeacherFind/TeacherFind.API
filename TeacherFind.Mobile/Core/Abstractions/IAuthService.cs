using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Contracts.Auth;
using TeacherFind.Mobile.Core.Models;

namespace TeacherFind.Mobile.Core.Abstractions;

    public interface IAuthService
    {
    Task<ApiRequestResult<LoginResponse>> LoginAsync(LoginRequest request);

    }

