using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Auth;

public class SocialLoginRequest
{
    /// <summary>
    /// "google" veya "apple"
    /// </summary>
    public string Provider { get; set; } = default!;

    /// <summary>
    /// Firebase'den dönen idToken (Google veya Apple tarafından imzalanmış)
    /// </summary>
    public string IdToken { get; set; } = default!;
}