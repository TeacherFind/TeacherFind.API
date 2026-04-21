using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(x => x.Id == notificationId);

        if (notification != null)
        {
            notification.IsRead = true;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}