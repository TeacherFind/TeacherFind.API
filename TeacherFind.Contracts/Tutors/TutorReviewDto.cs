using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Tutors;

public class TutorReviewDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ReviewerName { get; set; } = default!;
    public int Rating { get; set; }
    public string Comment { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}