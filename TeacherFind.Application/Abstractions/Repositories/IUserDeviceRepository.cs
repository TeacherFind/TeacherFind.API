using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface IUserDeviceRepository
{
    Task AddOrUpdateAsync(Guid userId, string fcmToken, string platform);
    Task<List<string>> GetUserTokensAsync(Guid userId);
    Task DeleteByTokenAsync(string fcmToken);
    Task DeleteUserTokenAsync(Guid userId, string fcmToken);
    Task SaveChangesAsync();
}