using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface IPushNotificationService
{
    Task<string?> SendToDeviceAsync(string deviceToken, string title, string body);
    Task SendToMultipleAsync(List<string> deviceTokens, string title, string body);
}