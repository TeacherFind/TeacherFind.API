using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using TeacherFind.Application.Abstractions.Repositories;

namespace TeacherFind.Infrastructure.Notifications;

public class FcmPushNotificationService : IPushNotificationService
{
    private readonly ILogger<FcmPushNotificationService> _logger;

    public FcmPushNotificationService(ILogger<FcmPushNotificationService> logger)
        => _logger = logger;

    public async Task<string?> SendToDeviceAsync(string deviceToken, string title, string body)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("Firebase başlatılmadı, push bildirimi gönderilemedi.");
            return null;
        }

        try
        {
            var message = new Message
            {
                Token = deviceToken,
                Notification = new Notification { Title = title, Body = body }
            };

            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push bildirimi gönderilemedi. Token: {Token}", deviceToken);
            return null;
        }
    }

    public async Task SendToMultipleAsync(List<string> deviceTokens, string title, string body)
    {
        if (FirebaseApp.DefaultInstance == null || deviceTokens.Count == 0)
            return;

        try
        {
            var message = new MulticastMessage
            {
                Tokens = deviceTokens,
                Notification = new Notification { Title = title, Body = body }
            };

            await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu push bildirimi gönderilemedi.");
        }
    }
}
