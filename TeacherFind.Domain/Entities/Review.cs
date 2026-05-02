using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeacherFind.Domain.Entities;

public class Review
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }     // yorumu yapan öğrenci
    public User? Reviewer { get; set; }
    public Guid ListingId { get; set; }     // = TeacherListingId
    public Guid? TeacherProfileId { get; set; }
    public Guid? BookingId { get; set; }
    public int Rating { get; set; }     // 1-5
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}