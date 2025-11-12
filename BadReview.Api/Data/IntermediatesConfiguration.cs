using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GameDeveloperConfiguration : IEntityTypeConfiguration<GameDeveloper>
{
    public void Configure(EntityTypeBuilder<GameDeveloper> builder)
    {
        builder.HasKey(gd => new { gd.GameId, gd.DeveloperId });

        builder.HasOne(gd => gd.Game)
            .WithMany(g => g.GameDevelopers)
            .HasForeignKey(gd => gd.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gd => gd.Developer)
            .WithMany(d => d.GameDevelopers)
            .HasForeignKey(gd => gd.DeveloperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GameGenreConfiguration : IEntityTypeConfiguration<GameGenre>
{
    public void Configure(EntityTypeBuilder<GameGenre> builder)
    {
        builder.HasKey(gg => new { gg.GameId, gg.GenreId });

        builder.HasOne(gg => gg.Game)
            .WithMany(g => g.GameGenres)
            .HasForeignKey(gg => gg.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gg => gg.Genre)
            .WithMany(g => g.GameGenres)
            .HasForeignKey(gg => gg.GenreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GamePlatformConfiguration : IEntityTypeConfiguration<GamePlatform>
{
    public void Configure(EntityTypeBuilder<GamePlatform> builder)
    {
        builder.HasKey(gp => new { gp.GameId, gp.PlatformId });

        builder.HasOne(gp => gp.Game)
            .WithMany(g => g.GamePlatforms)
            .HasForeignKey(gp => gp.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gp => gp.Platform)
            .WithMany(p => p.GamePlatforms)
            .HasForeignKey(gp => gp.PlatformId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}