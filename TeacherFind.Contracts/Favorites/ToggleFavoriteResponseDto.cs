using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Favorites;

public class ToggleFavoriteResponseDto
{
    public bool IsFavorite { get; set; }
    public string Message { get; set; } = default!;
}