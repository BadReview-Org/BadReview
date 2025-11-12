using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DeveloperConfiguration : IEntityTypeConfiguration<Developer>
{
    public void Configure(EntityTypeBuilder<Developer> builder)
    {
        // NO usar IDENTITY para mantener IDs de IGDB
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.OwnsOne(e => e.Logo).Property(c => c.ImageId).HasMaxLength(100);         

        builder.Property(e => e.Name)
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Country)
            .HasDefaultValue(0);
        builder.ToTable(t => t.HasCheckConstraint("CK_Developers_Country", "[Country] >= 0"));

        builder.Property(e => e.StartDate)
            .HasDefaultValue(null);
        builder.ToTable(t => t.HasCheckConstraint("CK_Developers_StartDate", "[StartDate] >= 0"));
    }
}