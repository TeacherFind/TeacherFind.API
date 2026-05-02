using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Contracts.Reviews;

public class CreateReviewRequestDto
{
    public Guid BookingId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}