using TeacherFind.Domain.Enums;

namespace TeacherFind.Contracts.Listings;

public class ListingFilterRequestDto
{
    public string? Search { get; set; }

    // İstersen şimdilik Category kalsın, sonra SubjectId'ye geçeriz
    public string? Category { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public ServiceType? ServiceType { get; set; }

    public bool? OnlyApproved { get; set; } = true;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
}