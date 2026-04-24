namespace TeacherFind.Domain.Entities;

public class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Eski SQL'deki bolum_id değeri
    public int Code { get; set; }

    public string Name { get; set; } = default!;

    public Guid UniversityId { get; set; }

    public University University { get; set; } = default!;

    public bool IsActive { get; set; } = true;
}