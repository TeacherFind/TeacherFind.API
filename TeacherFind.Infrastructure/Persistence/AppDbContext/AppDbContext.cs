using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    // =====================================================
    // Core Tables
    // =====================================================

    public DbSet<User> Users => Set<User>();

    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();

    public DbSet<TeacherListing> TeacherListings => Set<TeacherListing>();

    public DbSet<Booking> Bookings => Set<Booking>();

    public DbSet<TeacherCertificate> TeacherCertificates => Set<TeacherCertificate>();

    // =====================================================
    // Interaction Tables
    // =====================================================

    public DbSet<Favorite> Favorites => Set<Favorite>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();

    public DbSet<Report> Reports => Set<Report>();

    // =====================================================
    // Reference Data
    // =====================================================

    public DbSet<Subject> Subjects => Set<Subject>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<District> Districts => Set<District>();

    public DbSet<Neighborhood> Neighborhoods => Set<Neighborhood>();

    public DbSet<University> Universities => Set<University>();

    public DbSet<Department> Departments => Set<Department>();

    // =====================================================
    // Admin
    // =====================================================

    public DbSet<AdminInvitation> AdminInvitations => Set<AdminInvitation>();

    public DbSet<AdminActionLog> AdminActionLogs => Set<AdminActionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureConversation(modelBuilder);
        ConfigureMessage(modelBuilder);
        ConfigureListing(modelBuilder);
        ConfigureBooking(modelBuilder);

        ConfigureUniversity(modelBuilder);
        ConfigureDepartment(modelBuilder);
        ConfigureDistrict(modelBuilder);
        ConfigureNeighborhood(modelBuilder);
        ConfigureTeacherProfileEducation(modelBuilder);

        ConfigureAdminInvitation(modelBuilder);
        ConfigureAdminActionLog(modelBuilder);
        ConfigureReport(modelBuilder);
    }

    private static void ConfigureConversation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasMany(x => x.Messages)
                .WithOne(x => x.Conversation)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.User1Id, x.User2Id });
        });
    }

    private static void ConfigureMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasIndex(x => x.ConversationId);

            entity.HasIndex(x => new { x.ReceiverId, x.IsRead });

            entity.HasIndex(x => x.SentAt);
        });
    }

    private static void ConfigureListing(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeacherListing>(entity =>
        {
            entity.HasOne(x => x.Subject)
                .WithMany()
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.City)
                .WithMany()
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.District)
                .WithMany()
                .HasForeignKey(x => x.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Neighborhood)
                .WithMany()
                .HasForeignKey(x => x.NeighborhoodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.Headline)
                .HasMaxLength(200);

            entity.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(x => x.Category)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.SubCategory)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(x => x.SubjectId);
            entity.HasIndex(x => x.CityId);
            entity.HasIndex(x => x.DistrictId);
            entity.HasIndex(x => x.NeighborhoodId);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.IsApproved);
        });
    }

    private static void ConfigureBooking(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.TeacherListing)
                .WithMany()
                .HasForeignKey(x => x.TeacherListingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.StudentUser)
                .WithMany()
                .HasForeignKey(x => x.StudentUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TutorUser)
                .WithMany()
                .HasForeignKey(x => x.TutorUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Source)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.StudentNote)
                .HasMaxLength(1000);

            entity.Property(x => x.TutorNote)
                .HasMaxLength(1000);

            entity.HasIndex(x => x.TeacherListingId);
            entity.HasIndex(x => x.StudentUserId);
            entity.HasIndex(x => x.TutorUserId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.StartTime);
        });
    }

    private static void ConfigureUniversity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<University>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .IsRequired();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.CityPlateCode)
                .IsRequired();

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.HasIndex(x => x.Name);
        });
    }

    private static void ConfigureDepartment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .IsRequired();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(250);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasOne(x => x.University)
                .WithMany(x => x.Departments)
                .HasForeignKey(x => x.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Code);

            entity.HasIndex(x => new { x.UniversityId, x.Name })
                .IsUnique();
        });
    }

    private static void ConfigureDistrict(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .IsRequired();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasOne(x => x.City)
                .WithMany(x => x.Districts)
                .HasForeignKey(x => x.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Code);

            entity.HasIndex(x => new { x.CityId, x.Name })
                .IsUnique();
        });
    }

    private static void ConfigureNeighborhood(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Neighborhood>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .IsRequired();

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.IsActive)
                .IsRequired();

            entity.HasOne(x => x.District)
                .WithMany(x => x.Neighborhoods)
                .HasForeignKey(x => x.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Code);

            entity.HasIndex(x => new { x.DistrictId, x.Name })
                .IsUnique();
        });
    }

    private static void ConfigureTeacherProfileEducation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeacherProfile>(entity =>
        {
            entity.HasOne(x => x.University)
                .WithMany()
                .HasForeignKey(x => x.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.DepartmentEntity)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAdminInvitation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminInvitation>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            entity.Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(x => x.ExpiresAt)
                .IsRequired();

            entity.Property(x => x.IsUsed)
                .IsRequired();

            entity.HasOne(x => x.InvitedByUser)
                .WithMany()
                .HasForeignKey(x => x.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Email);
            entity.HasIndex(x => x.TokenHash);
            entity.HasIndex(x => x.IsUsed);
            entity.HasIndex(x => x.ExpiresAt);
        });
    }

    private static void ConfigureAdminActionLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminActionLog>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.UserAgent)
                .HasMaxLength(500);

            entity.HasOne(x => x.AdminUser)
                .WithMany()
                .HasForeignKey(x => x.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.AdminUserId);
            entity.HasIndex(x => x.Action);
            entity.HasIndex(x => x.EntityName);
            entity.HasIndex(x => x.CreatedAt);
        });
    }

    private static void ConfigureReport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasOne(x => x.Reporter)
                .WithMany()
                .HasForeignKey(x => x.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.TargetUser)
                .WithMany()
                .HasForeignKey(x => x.TargetUserId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.TargetListing)
                .WithMany()
                .HasForeignKey(x => x.TargetListingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.ReporterId);
        });
    }
}