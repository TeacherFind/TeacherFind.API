using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);

    Task<List<Notification>> GetUserNotificationsAsync(Guid userId);

    Task MarkAsReadAsync(Guid notificationId);

    Task SaveChangesAsync();
}