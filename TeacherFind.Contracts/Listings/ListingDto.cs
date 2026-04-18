using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Listings;

public class ListingDto
{
    public Guid Id { get; set; }

    public Guid TeacherProfileId { get; set; }

    public string Title { get; set; } = default!;

    public string Description { get; set; } = default!;

    public decimal Price { get; set; }

    public bool IsActive { get; set; }

    public bool IsApproved { get; set; }

    public int ViewCount { get; set; }
}