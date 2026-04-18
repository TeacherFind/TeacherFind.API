using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Application.Abstractions.Repositories;

public interface IFavoriteRepository
{
    Task AddAsync(Favorite favorite);
    Task RemoveAsync(Favorite favorite);

    Task<Favorite?> GetAsync(Guid userId, Guid listingId);

    Task<List<Favorite>> GetUserFavoritesAsync(Guid userId);

    Task SaveChangesAsync();
}
