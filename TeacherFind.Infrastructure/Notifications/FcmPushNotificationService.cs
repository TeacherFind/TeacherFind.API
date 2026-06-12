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

    public async Task<string?> SendToDeviceAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("Firebase başlatılmadı, push bildirimi gönderilemedi.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(deviceToken))
        {
            _logger.LogWarning("Boş FCM token ile push bildirimi gönderilemedi.");
            return null;
        }

        try
        {
            var message = new Message
            {
                Token = deviceToken.Trim(),
                Notification = new Notification { Title = title, Body = body },
                Data = NormalizeData(data)
            };

            return await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push bildirimi gönderilemedi. Token: {Token}", deviceToken);
            return null;
        }
    }

    public async Task<List<string>> SendToMultipleAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        var failedTokens = new List<string>();

        if (FirebaseApp.DefaultInstance == null)
        {
            _logger.LogWarning("Firebase başlatılmadı, toplu push bildirimi gönderilemedi.");
            return failedTokens;
        }

        var tokens = deviceTokens
            .Where(token => !string.IsNullOrWhiteSpace(token))
            .Select(token => token.Trim())
            .Distinct()
            .ToList();

        if (tokens.Count == 0)
        {
            _logger.LogWarning("Geçerli FCM token bulunamadığı için toplu push bildirimi gönderilmedi.");
            return failedTokens;
        }

        try
        {
            foreach (var tokenBatch in tokens.Chunk(500))
            {
                var batchTokens = tokenBatch.ToList();
                var message = new MulticastMessage
                {
                    Tokens = batchTokens,
                    Notification = new Notification { Title = title, Body = body },
                    Data = NormalizeData(data)
                };

                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

                for (var i = 0; i < response.Responses.Count; i++)
                {
                    var sendResponse = response.Responses[i];
                    if (sendResponse.IsSuccess)
                        continue;

                    var failedToken = batchTokens[i];
                    if (IsInvalidToken(sendResponse.Exception))
                        failedTokens.Add(failedToken);

                    _logger.LogWarning(
                        sendResponse.Exception,
                        "Push bildirimi token için başarısız oldu. Token: {Token}",
                        failedToken);
                }

                _logger.LogInformation(
                    "Toplu push bildirimi tamamlandı. Başarılı: {SuccessCount}, Başarısız: {FailureCount}",
                    response.SuccessCount,
                    response.FailureCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Toplu push bildirimi gönderilemedi.");
        }

        return failedTokens.Distinct().ToList();
    }

    private static Dictionary<string, string>? NormalizeData(Dictionary<string, string>? data)
    {
        var normalizedData = data?
            .Where(item => !string.IsNullOrWhiteSpace(item.Key) && item.Value is not null)
            .ToDictionary(item => item.Key, item => item.Value);

        return normalizedData?.Count > 0 ? normalizedData : null;
    }

    private static bool IsInvalidToken(Exception? exception)
        => exception is FirebaseMessagingException messagingException &&
           (messagingException.MessagingErrorCode == MessagingErrorCode.Unregistered ||
            messagingException.MessagingErrorCode == MessagingErrorCode.InvalidArgument);
}
