using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;

using Microsoft.EntityFrameworkCore;

namespace BacklogOptimizer.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ScrapedPage> ScrapedPages { get; set; }
    public DbSet<ScrapingJob> ScrapingJobs { get; set; }
    public DbSet<JiraIssue> JiraIssues { get; set; }
    public DbSet<JiraIssueEmbedding> JiraIssueEmbeddings { get; set; }
    public DbSet<ScrapedPageEmbedding> ScrapedPageEmbeddings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetColumnType("timestamp with time zone");
        }

        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<JiraIssue>()
            .HasIndex(j => j.JiraKey)
            .IsUnique();

        modelBuilder.Entity<JiraIssueEmbedding>(b =>
        {
            b.HasKey(e => e.JiraIssueId);
            b.HasOne(e => e.JiraIssue)
             .WithOne(j => j.Embedding)
             .HasForeignKey<JiraIssueEmbedding>(e => e.JiraIssueId)
             .OnDelete(DeleteBehavior.Cascade);
            b.Property(e => e.Vector).HasColumnType("vector(1536)");
            b.Property(e => e.EmbeddedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<ScrapedPageEmbedding>(b =>
        {
            b.HasKey(e => e.ScrapedPageId);
            b.HasOne(e => e.ScrapedPage)
             .WithOne(p => p.Embedding)
             .HasForeignKey<ScrapedPageEmbedding>(e => e.ScrapedPageId)
             .OnDelete(DeleteBehavior.Cascade);
            b.Property(e => e.Vector).HasColumnType("vector(1536)");
            b.Property(e => e.EmbeddedAt).HasColumnType("timestamp with time zone");
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt = now;

            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
