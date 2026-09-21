using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Data;

public class UsDbContext : DbContext
{
    public UsDbContext(DbContextOptions<UsDbContext> options) : base(options)
    {
    }

    public DbSet<Url> Urls { get; set; }
    public DbSet<Manager> Managers { get; set; }
    public DbSet<RefreshTokenEntry> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<Url>()
            .HasIndex(x => x.ShortUrl)
            .IsUnique()
            .IncludeProperties(nameof(Url.LongUrl));
    }
}