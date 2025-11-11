using Microsoft.EntityFrameworkCore;

using BadReview.Api.Models;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace BadReview.Api.Data;

public class BadReviewContext : DbContext
{
    public BadReviewContext(DbContextOptions<BadReviewContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public DbSet<Platform> Platforms { get; set; } = null!;
    public DbSet<Developer> Developers { get; set; } = null!;
    public DbSet<GameGenre> GameGenres { get; set; } = null!;
    public DbSet<GamePlatform> GamePlatforms { get; set; } = null!;
    public DbSet<GameDeveloper> GameDevelopers { get; set; } = null!;

    // Wrapper of SaveChangesAsync method, to capture exceptions automatically. We could return an error code,
    // string or just raise another exception.
    public async Task<bool> SafeSaveChangesAsync(bool logging = true, CancellationToken cancellationToken = default)
    {
        bool success = false;
        try
        {
            await SaveChangesAsync(cancellationToken);
            success = true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            if (logging) Console.WriteLine($"[Concurrency Error] {ex.Message}");
        }
        catch (DbUpdateException ex)
        {
            if (logging)
            {
                Console.WriteLine($"[DB Update Error] {ex.Message}");

                if (ex.InnerException is SqlException sqlEx) Console.WriteLine($"[SQL Error] {sqlEx.Message}");
            }
        }
        catch (ValidationException ex)
        {
            if (logging) Console.WriteLine($"[Validation Error] {ex.Message}");
        }
        catch (Exception ex)
        {
            if (logging) Console.WriteLine($"[Unexpected Error] {ex}");
        }

        return success;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.OwnsOne(e => e.Date, date =>
            {
                date.Property(d => d.CreatedAt).HasDefaultValueSql("getdate()").ValueGeneratedOnAdd();
                date.Property(d => d.UpdatedAt).HasDefaultValueSql("getdate()").ValueGeneratedOnAddOrUpdate();
            });
        });

        // Configure Game entity - NO usar IDENTITY para mantener IDs de IGDB
        modelBuilder.Entity<Game>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.OwnsOne(e => e.Cover);
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.OwnsOne(e => e.Logo);
        });

        modelBuilder.Entity<Developer>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.OwnsOne(e => e.Logo);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            //entity.HasKey(e => e.Id);
            //entity.Property(e => e.ReviewText).IsRequired();
            //entity.Property(e => e.StateEnum).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Game)
                .WithMany(g => g.Reviews)
                .HasForeignKey(e => e.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.OwnsOne(e => e.Date, date =>
            {
                date.Property(d => d.CreatedAt).HasDefaultValueSql("getdate()").ValueGeneratedOnAdd();
                date.Property(d => d.UpdatedAt).HasDefaultValueSql("getdate()").ValueGeneratedOnAddOrUpdate();
            });
        });

        modelBuilder.Entity<GameGenre>(entity =>
        {
            entity.HasKey(gg => new { gg.GameId, gg.GenreId });

            entity.HasOne(gg => gg.Game)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gg => gg.Genre)
                .WithMany(g => g.GameGenres)
                .HasForeignKey(gg => gg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GamePlatform>(entity =>
        {
            entity.HasKey(gp => new { gp.GameId, gp.PlatformId });

            entity.HasOne(gp => gp.Game)
                .WithMany(g => g.GamePlatforms)
                .HasForeignKey(gp => gp.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gp => gp.Platform)
                .WithMany(p => p.GamePlatforms)
                .HasForeignKey(gp => gp.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GameDeveloper>(entity =>
        {
            entity.HasKey(gd => new { gd.GameId, gd.DeveloperId });

            entity.HasOne(gd => gd.Game)
                .WithMany(g => g.GameDevelopers)
                .HasForeignKey(gd => gd.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gd => gd.Developer)
                .WithMany(d => d.GameDevelopers)
                .HasForeignKey(gd => gd.DeveloperId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}