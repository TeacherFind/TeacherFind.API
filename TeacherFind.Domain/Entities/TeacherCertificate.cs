using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeacherFind.Domain.Common;

namespace TeacherFind.Domain.Entities;

public class TeacherCertificate : BaseEntity
{
    public Guid TeacherProfileId { get; set; }

    public TeacherProfile TeacherProfile { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string Organization { get; set; } = default!;

    public int Year { get; set; }

    public string? FileUrl { get; set; }

    public string? FileName { get; set; }

    public string? ContentType { get; set; }
}