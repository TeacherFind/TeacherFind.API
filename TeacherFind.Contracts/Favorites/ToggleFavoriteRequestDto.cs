using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Favorites;

public class ToggleFavoriteRequestDto
{
    public Guid TutorId { get; set; }   // maps to TeacherListing.Id
}