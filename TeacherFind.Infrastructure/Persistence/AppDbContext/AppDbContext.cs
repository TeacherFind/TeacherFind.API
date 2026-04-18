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

    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 🔥 Conversation ilişkisi
        modelBuilder.Entity<Conversation>()
            .HasMany(x => x.Messages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // 🔥 Index (performans)
        modelBuilder.Entity<Conversation>()
            .HasIndex(x => new { x.User1Id, x.User2Id });

        modelBuilder.Entity<Message>()
            .HasIndex(x => x.ConversationId);

        modelBuilder.Entity<Message>()
            .HasIndex(x => new { x.ReceiverId, x.IsRead });

        modelBuilder.Entity<Message>()
            .HasIndex(x => x.SentAt);
    }
}