namespace TeacherFind.Domain.Entities;

public class University
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public int Code { get; set; }

    public string Name { get; set; } = default!;

    public int CityPlateCode { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Department> Departments { get; set; } = new List<Department>();
}