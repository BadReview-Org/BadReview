using BadReview.Api.Models;
using BadReview.Shared.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasOne(e => e.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Game)
            .WithMany(g => g.Reviews)
            .HasForeignKey(e => e.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.UserId, e.GameId })
            .IsUnique()
            .HasDatabaseName("UX_Reviews_UserId_GameId");

        builder.OwnsOne(e => e.Date, date =>
        {
            date.Property(d => d.CreatedAt)
                .HasDefaultValueSql("getdate()").ValueGeneratedOnAdd();
            date.Property(d => d.UpdatedAt)
                .HasDefaultValueSql("getdate()").ValueGeneratedOnAddOrUpdate();
        });

        builder.Property(e => e.Rating)
            .HasDefaultValue(0);
        builder.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] >= 0 AND [Rating] <= 5"));

        builder.Property(g => g.ReviewText)
            .HasMaxLength(3000);

        builder.Property(g => g.IsFavorite)
            .HasDefaultValue(false);

        builder.Property(g => g.IsReview)
            .HasDefaultValue(false);

        // Condicion sobre IsReview/IsFavorite: no pueden ser ambos false a la vez
        builder.ToTable(t => t.HasCheckConstraint("CK_Reviews_ReviewFlags",
            "([IsReview] = 1 OR [IsFavorite] = 1)"));

        // Condiciones de Start/EndDate
        // Si ninguno es null, EndDate >= StartDate
        builder.ToTable(t => t.HasCheckConstraint("CK_Reviews_Dates",
            "([StartDate] IS NULL OR [EndDate] IS NULL OR [EndDate] >= [StartDate])"));

        // Condiciones por ReviewState
        // Si el estado es PLAYING (1), no tiene EndDate
        // Si el estado es WISHLIST (2), no tiene ni StartDate ni EndDate
        // En otro caso, no hay restricciones
        builder.ToTable(t => t.HasCheckConstraint("CK_Reviews_StateDates",
            @"([StateEnum] = 1 AND [EndDate] IS NULL) OR
            ([StateEnum] = 2 AND [StartDate] IS NULL AND [EndDate] IS NULL) OR
            ([StateEnum] IN (0,3))"
        ));
    }
}