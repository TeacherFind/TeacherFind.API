using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Contracts.Auth;

public class RegisterRequest
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Student;
    public string? PhoneNumber { get; set; }
    public Guid? CityId { get; set; }
    public Guid? DistrictId { get; set; }  
    public Guid? NeighborhoodId { get; set; }   

    // Tutor-only fields
    public Guid? UniversityId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? Bio { get; set; }

    public List<RegisterCertificateDto> Certificates { get; set; } = new();
    public List<RegisterSubjectDto> Subjects { get; set; } = new();
}

public class RegisterCertificateDto
{
    public string Name { get; set; } = default!;
    public string? Organization { get; set; }
    public int? Year { get; set; }
    public string? Link { get; set; }
    public string? FileUrl { get; set; }
}

public class RegisterSubjectDto
{
    public int? SubjectId { get; set; }
    public string? Stage { get; set; }
    public string? Category { get; set; }
    public string? Name { get; set; }
    public string? Level { get; set; }
}