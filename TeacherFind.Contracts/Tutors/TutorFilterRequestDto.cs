using TeacherFind.Domain.Enums;

namespace TeacherFind.Contracts.Tutors;

public class TutorFilterRequestDto
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public int? SubjectId { get; set; }
    public Guid? CityId { get; set; }
    public Guid? DistrictId { get; set; }
    public Guid? NeighborhoodId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ServiceType? ServiceType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;

    /// <summary>newest (default) | price_asc | price_desc | rating</summary>
    public string? Sort { get; set; }
}