using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Services;

public interface INotificationService
{
    Task SendNotificationAsync(Guid userId, string title, string message, string type);

    Task<List<Notification>> GetMyNotificationsAsync(Guid userId);

    Task MarkAsReadAsync(Guid notificationId);
}