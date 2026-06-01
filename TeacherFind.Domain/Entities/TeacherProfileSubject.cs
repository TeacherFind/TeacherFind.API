using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeacherFind.Domain.Common;

namespace TeacherFind.Domain.Entities;

public class TeacherProfileSubject : BaseEntity
{
    public Guid TeacherProfileId { get; set; }
    public TeacherProfile TeacherProfile { get; set; } = default!;

    public int? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    // Denormalized fields for when Subject seed doesn't have exact match
    public string? Stage { get; set; }
    public string? Category { get; set; }
    public string? Name { get; set; }
    public string? Level { get; set; }
}
