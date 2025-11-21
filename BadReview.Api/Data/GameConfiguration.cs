using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        // NO usar IDENTITY para mantener IDs de IGDB
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.OwnsOne(e => e.Cover).Property(c => c.ImageId).HasMaxLength(200);         

        builder.Property(g => g.Name)
            .HasMaxLength(400);

        builder.Property(g => g.Summary)
            .HasMaxLength(10000);

        builder.Property(g => g.RatingIGDB)
            .HasDefaultValue(0)
            .HasPrecision(5, 2); // permite hasta 999.99 (suficiente para rating 0–100)

        builder.Property(g => g.Total_RatingBadReview)
            .HasDefaultValue(0);
        builder.ToTable(t => t.HasCheckConstraint("CK_Games_Total_RatingBadReview", "[Total_RatingBadReview] >= 0"));

        builder.Property(g => g.Count_RatingBadReview)
            .HasDefaultValue(0);
        builder.ToTable(t => t.HasCheckConstraint("CK_Games_Count_RatingBadReview", "[Count_RatingBadReview] >= 0"));

        builder.Property(g => g.Video)
            .HasMaxLength(200);
    }
}