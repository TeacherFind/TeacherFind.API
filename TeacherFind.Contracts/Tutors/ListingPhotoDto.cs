using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class ListingPhotoDto
{
    public Guid Id { get; set; }

    public string PhotoUrl { get; set; } = default!;

    public bool IsMain { get; set; }

    public int SortOrder { get; set; }
}