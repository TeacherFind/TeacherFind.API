using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class UpdateListingPhotoSortOrderDto
{
    /// <summary>Photo IDs in the desired display order</summary>
    public List<Guid> PhotoIds { get; set; } = new();
}