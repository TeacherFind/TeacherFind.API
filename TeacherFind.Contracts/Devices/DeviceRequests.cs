using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Devices;

public class RegisterDeviceRequest
{
    public string FcmToken { get; set; } = default!;
    public string Platform { get; set; } = default!; // "web" | "android" | "ios"
}

public class UnregisterDeviceRequest
{
    public string FcmToken { get; set; } = default!;
}