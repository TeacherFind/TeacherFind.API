namespace TeacherFind.Domain.Entities;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Kim şikayet etti
    public Guid ReporterId { get; set; }
    public User? Reporter { get; set; }

    // Ne şikayet edildi (biri dolu olmalı)
    public Guid? TargetListingId { get; set; }
    public TeacherListing? TargetListing { get; set; }

    public Guid? TargetUserId { get; set; }
    public User? TargetUser { get; set; }

    public string Reason { get; set; } = default!; // "Uygunsuz içerik", "Sahte ilan", "Hakaret" vb.
    public string? Description { get; set; }

    public string Status { get; set; } = "Pending"; // Pending | Resolved | Dismissed
    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}