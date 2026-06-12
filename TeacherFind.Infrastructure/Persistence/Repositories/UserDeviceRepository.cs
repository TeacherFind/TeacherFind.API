using Microsoft.EntityFrameworkCore;
using TeacherFind.Application.Abstractions.Repositories;
using TeacherFind.Domain.Entities;
using TeacherFind.Infrastructure.Persistence;

namespace TeacherFind.Infrastructure.Persistence.Repositories;

public class UserDeviceRepository : IUserDeviceRepository
{
    private readonly AppDbContext _context;

    public UserDeviceRepository(AppDbContext context) => _context = context;

    public async Task AddOrUpdateAsync(Guid userId, string fcmToken, string platform)
    {
        var existing = await _context.UserDevices
            .FirstOrDefaultAsync(x => x.FcmToken == fcmToken);

        if (existing != null)
        {
            // Token already exists — refresh ownership and timestamp
            existing.UserId = userId;
            existing.Platform = platform;
            existing.LastUsedAt = DateTime.UtcNow;
            return;
        }

        await _context.UserDevices.AddAsync(new UserDevice
        {
            UserId = userId,
            FcmToken = fcmToken,
            Platform = platform
        });
    }

    public async Task<List<string>> GetUserTokensAsync(Guid userId)
        => await _context.UserDevices
            .Where(x => x.UserId == userId)
            .Select(x => x.FcmToken)
            .ToListAsync();

    public async Task DeleteByTokenAsync(string fcmToken)
    {
        var device = await _context.UserDevices
            .FirstOrDefaultAsync(x => x.FcmToken == fcmToken);

        if (device != null)
            _context.UserDevices.Remove(device);
    }

    public async Task DeleteUserTokenAsync(Guid userId, string fcmToken)
    {
        var device = await _context.UserDevices
            .FirstOrDefaultAsync(x => x.UserId == userId && x.FcmToken == fcmToken);

        if (device != null)
            _context.UserDevices.Remove(device);
    }

    public async Task SaveChangesAsync()
        => await _context.SaveChangesAsync();
}