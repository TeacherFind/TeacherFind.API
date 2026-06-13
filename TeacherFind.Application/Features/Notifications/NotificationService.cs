using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Notifications;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly IPushNotificationService _pushNotificationService;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserDeviceRepository userDeviceRepository,
        IPushNotificationService pushNotificationService)
    {
        _notificationRepository = notificationRepository;
        _userDeviceRepository = userDeviceRepository;
        _pushNotificationService = pushNotificationService;
    }

    public async Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
        return Map(notifications);
    }

    public async Task<List<NotificationDto>> GetMyUnreadNotificationsAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
        return Map(notifications.Where(n => !n.IsRead).ToList());
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);

        if (notification == null || notification.UserId != userId)
            return false;

        await _notificationRepository.MarkAsReadAsync(notificationId, userId);
        await _notificationRepository.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task ClearAllNotificationsAsync(Guid userId)
    {
        await _notificationRepository.DeleteAllUserNotificationsAsync(userId);
        await _notificationRepository.SaveChangesAsync();
    }

    public async Task CreateAsync(
        Guid userId,
        string title,
        string message,
        string type,
        string? link = null,
        Guid? senderUserId = null,
        string? senderName = null,
        Dictionary<string, string>? data = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            SenderUserId = senderUserId,
            SenderName = senderName,
            Title = title,
            Message = message,
            Type = type,
            Link = link
        };

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        // Push notification (FCM) — fails silently, never breaks the main flow
        var tokens = await _userDeviceRepository.GetUserTokensAsync(userId);

        if (tokens.Count > 0)
        {
            var notificationData = BuildNotificationData(type, link, senderUserId, senderName, data);
            var invalidTokens = await _pushNotificationService.SendToMultipleAsync(tokens, title, message, notificationData);

            foreach (var invalidToken in invalidTokens)
            {
                await _userDeviceRepository.DeleteByTokenAsync(invalidToken);
            }

            if (invalidTokens.Count > 0)
                await _userDeviceRepository.SaveChangesAsync();
        }
    }

    // kept for BookingService compatibility
    public async Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        string type,
        Guid? senderUserId = null,
        string? senderName = null,
        string? link = null,
        Dictionary<string, string>? data = null)
        => await CreateAsync(userId, title, message, type, link, senderUserId, senderName, data);

    private static Dictionary<string, string> BuildNotificationData(
        string type,
        string? link,
        Guid? senderUserId,
        string? senderName,
        Dictionary<string, string>? data)
    {
        var result = data is null
            ? new Dictionary<string, string>()
            : new Dictionary<string, string>(data);

        result["type"] = NormalizeNotificationType(type);

        if (!string.IsNullOrWhiteSpace(link))
            result["link"] = link;

        if (senderUserId.HasValue)
            result["senderId"] = senderUserId.Value.ToString();

        if (!string.IsNullOrWhiteSpace(senderName))
            result["senderName"] = senderName;

        return result;
    }

    private static string NormalizeNotificationType(string type)
        => string.Equals(type, "Message", StringComparison.OrdinalIgnoreCase)
            ? "chat_message"
            : type;

    private static List<NotificationDto> Map(List<Notification> notifications)
        => notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Title = n.Title,
            Message = n.Message,
            Type = n.Type,
            SenderName = n.SenderName,
            Link = n.Link,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
}
