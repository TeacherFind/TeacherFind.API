using Microsoft.EntityFrameworkCore;
using TeacherFind.Domain.Entities;

namespace TeacherFind.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<TeacherProfile> TeacherProfiles => Set<TeacherProfile>();
    public DbSet<TeacherListing> TeacherListings => Set<TeacherListing>();

    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();

    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<University> Universities => Set<University>();
    public DbSet<Department> Departments => Set<Department>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureConversation(modelBuilder);
        ConfigureMessage(modelBuilder);
        ConfigureListing(modelBuilder);
        ConfigureUniversity(modelBuilder);
        ConfigureDepartment(modelBuilder);
        ConfigureTeacherProfileEducation(modelBuilder);
    }

    private static void ConfigureConversation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Conversation>()
            .HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Conversation>()
            .HasIndex(x => new { x.User1Id, x.User2Id });
    }

    private static void ConfigureMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>()
            .HasIndex(x => x.ConversationId);

        modelBuilder.Entity<Message>()
            .HasIndex(x => new { x.ReceiverId, x.IsRead });

        modelBuilder.Entity<Message>()
            .HasIndex(x => x.SentAt);
    }

    private static void ConfigureListing(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeacherListing>()
            .HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherListing>()
            .HasOne(x => x.City)
            .WithMany()
            .HasForeignKey(x => x.CityId)
            .OnDelete(DeleteBehavior.Restrict);
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

    private static void ConfigureTeacherProfileEducation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TeacherProfile>()
            .HasOne(x => x.University)
            .WithMany()
            .HasForeignKey(x => x.UniversityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherProfile>()
            .HasOne(x => x.DepartmentEntity)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}