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

    public async Task CreateAsync(
        Guid userId,
        string title,
        string message,
        string type,
        string? link = null,
        Guid? senderUserId = null,
        string? senderName = null)
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

    // kept for BookingService compatibility
    public async Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        string type,
        Guid? senderUserId = null,
        string? senderName = null,
        string? link = null)
        => await CreateAsync(userId, title, message, type, link, senderUserId, senderName);

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