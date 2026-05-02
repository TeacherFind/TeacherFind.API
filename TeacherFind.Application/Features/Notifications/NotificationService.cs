using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Application.Abstractions.Services;
using TeacherFind.Contracts.Notifications;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
        => _notificationRepository = notificationRepository;

    public async Task SendNotificationAsync(Guid userId, string title, string message, string type,
                                            Guid? senderUserId = null, string? senderName = null, string? link = null)
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
    }

    public async Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);

        return notifications.Select(n => new NotificationDto
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

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);

        // Başka kullanıcının bildirimi değiştirilemez
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
}