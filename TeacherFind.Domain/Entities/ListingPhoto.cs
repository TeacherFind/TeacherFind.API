using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Common;

namespace TeacherFind.Domain.Entities;

public class ListingPhoto : AuditableEntity
{
    public Guid ListingId { get; set; }

    public TeacherListing Listing { get; set; } = default!;

    public string PhotoUrl { get; set; } = default!;

    public bool IsMain { get; set; }

    public int SortOrder { get; set; }

}