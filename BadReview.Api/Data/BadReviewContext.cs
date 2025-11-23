using Microsoft.EntityFrameworkCore;

using BadReview.Api.Models;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace BadReview.Api.Data;

public class BadReviewContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public DbSet<Platform> Platforms { get; set; } = null!;
    public DbSet<Developer> Developers { get; set; } = null!;
    public DbSet<GameGenre> GameGenres { get; set; } = null!;
    public DbSet<GamePlatform> GamePlatforms { get; set; } = null!;
    public DbSet<GameDeveloper> GameDevelopers { get; set; } = null!;

    public BadReviewContext(DbContextOptions<BadReviewContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GameConfiguration());
        modelBuilder.ApplyConfiguration(new DeveloperConfiguration());
        modelBuilder.ApplyConfiguration(new GenreConfiguration());
        modelBuilder.ApplyConfiguration(new PlatformConfiguration());
        modelBuilder.ApplyConfiguration(new ReviewConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new GameDeveloperConfiguration());
        modelBuilder.ApplyConfiguration(new GameGenreConfiguration());
        modelBuilder.ApplyConfiguration(new GamePlatformConfiguration());

        base.OnModelCreating(modelBuilder);
    }

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

                if (ex.InnerException is PostgresException sqlEx) Console.WriteLine($"[SQL Error] {sqlEx.Message}");
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
}