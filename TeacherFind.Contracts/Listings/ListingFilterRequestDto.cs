using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Contracts.Listings;

public class ListingFilterRequestDto
{
    public string? Search { get; set; }

    public string? Category { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public ServiceType? ServiceType { get; set; }

    public bool? OnlyApproved { get; set; }
}