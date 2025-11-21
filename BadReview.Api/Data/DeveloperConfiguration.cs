using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DeveloperConfiguration : IEntityTypeConfiguration<Developer>
{
    public void Configure(EntityTypeBuilder<Developer> builder)
    {
        // NO usar IDENTITY para mantener IDs de IGDB
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.OwnsOne(e => e.Logo).Property(c => c.ImageId).HasMaxLength(200);         

        builder.Property(e => e.Name)
            .HasMaxLength(400);

        builder.Property(e => e.Description)
            .HasMaxLength(10000);

        builder.Property(e => e.Country)
            .HasDefaultValue(0);

        builder.Property(e => e.StartDate)
            .HasDefaultValue(null);
    }
}