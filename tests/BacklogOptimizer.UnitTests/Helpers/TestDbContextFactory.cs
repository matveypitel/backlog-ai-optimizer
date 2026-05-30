using BacklogOptimizer.Core.Common;
using BacklogOptimizer.Core.Entities;
using BacklogOptimizer.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BacklogOptimizer.UnitTests.Helpers;

// Standalone test context that avoids Npgsql/pgvector specifics incompatible with InMemory.
internal sealed class TestApplicationDbContext : ApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Do NOT call base — it calls HasPostgresExtension and HasColumnType("vector(...)"),
        // both of which are incompatible with the InMemory provider.

        // Exclude embedding entities whose Vector property (Pgvector type) cannot be mapped by InMemory.
        modelBuilder.Ignore<JiraIssueEmbedding>();
        modelBuilder.Ignore<CompetitorFeatureEmbedding>();
        modelBuilder.Entity<JiraIssue>().Ignore(j => j.Embedding);
        modelBuilder.Entity<CompetitorFeature>().Ignore(f => f.Embedding);

        modelBuilder.Entity<JiraIssue>()
            .HasIndex(j => j.JiraKey)
            .IsUnique();

        modelBuilder.Entity<CompetitorFeature>(b =>
        {
            b.HasOne(f => f.ScrapedPage)
             .WithMany(p => p.Features)
             .HasForeignKey(f => f.ScrapedPageId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReprioritizationSuggestion>()
            .HasIndex(r => r.JiraKey)
            .IsUnique();

        modelBuilder.Entity<User>(b =>
        {
            b.HasIndex(u => u.Email).IsUnique();
            b.Property(u => u.Email).IsRequired();
            b.Property(u => u.PasswordHash).IsRequired();
            b.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<PromptTemplate>(b =>
        {
            b.HasIndex(t => t.Type).IsUnique();
            b.Property(t => t.Type).HasConversion<int>();
        });

        modelBuilder.Entity<ScrapingSource>(b =>
        {
            b.HasIndex(s => s.Url).IsUnique();
            b.Property(s => s.Url).IsRequired();
        });
    }
}

internal static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new TestApplicationDbContext(options);
    }
}
