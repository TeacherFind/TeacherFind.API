using TeacherFind.Domain.Common;
using TeacherFind.Domain.Enums;

namespace TeacherFind.Domain.Entities;

public class Booking : AuditableEntity
{
    public Guid TeacherListingId { get; set; }
    public TeacherListing TeacherListing { get; set; } = default!;

    public Guid StudentUserId { get; set; }
    public User StudentUser { get; set; } = default!;

    public Guid TutorUserId { get; set; }
    public User TutorUser { get; set; } = default!;

    public DateTime StartTime { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    public DateTime EndTime { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public BookingSource Source { get; set; } = BookingSource.Site;

    public string? StudentNote { get; set; }

    public string? TutorNote { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? RejectedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}