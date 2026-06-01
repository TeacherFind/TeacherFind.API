using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;
using TeacherFind.Contracts.Notifications;

namespace TeacherFind.Application.Abstractions.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetMyNotificationsAsync(Guid userId);
    Task<List<NotificationDto>> GetMyUnreadNotificationsAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);

    Task CreateAsync(
        Guid userId,
        string title,
        string message,
        string type,
        string? link = null,
        Guid? senderUserId = null,
        string? senderName = null);

    // kept for BookingService compatibility
    Task SendNotificationAsync(
        Guid userId,
        string title,
        string message,
        string type,
        Guid? senderUserId = null,
        string? senderName = null,
        string? link = null);
}