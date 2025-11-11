#if false
using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        // Configure Game entity - NO usar IDENTITY para mantener IDs de IGDB
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.OwnsOne(e => e.Cover);

        builder.Property(g => g.Name)
            .HasMaxLength(200);

        builder.Property(g => g.Summary)
            .HasMaxLength(4000);

        builder.Property(g => g.RatingIGDB)
            .HasDefaultValue(0)
            .HasPrecision(5, 2); // permite hasta 999.99 (suficiente para rating 0–100)

        builder.HasCheckConstraint();

        builder.Property(g => g.Total_RatingBadReview)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(g => g.Count_RatingBadReview)
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(g => g.Video)
            .HasMaxLength(500);

        // Relación 1-1 opcional con Image
        builder.HasOne(g => g.Cover)
            .WithOne()
            .HasForeignKey<Game>(g => g.CoverId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
#endif