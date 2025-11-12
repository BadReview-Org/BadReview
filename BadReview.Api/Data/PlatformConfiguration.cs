using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PlatformConfiguration : IEntityTypeConfiguration<Platform>
{
    public void Configure(EntityTypeBuilder<Platform> builder)
    {
        // NO usar IDENTITY para mantener IDs de IGDB
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.OwnsOne(e => e.Logo).Property(c => c.ImageId).HasMaxLength(100);         

        builder.Property(e => e.Name)
            .HasMaxLength(200);

        builder.Property(e => e.Abbreviation)
            .HasMaxLength(100);

        builder.Property(e => e.Summary)
            .HasMaxLength(2000);

        builder.Property(e => e.PlatformType)
            .HasDefaultValue(0);
        builder.ToTable(t => t.HasCheckConstraint("CK_Platforms_PlatformType", "[PlatformType] >= 0"));

        builder.Property(e => e.PlatformTypeName)
            .HasMaxLength(200);

        builder.Property(e => e.Generation)
            .HasDefaultValue(0);
        builder.ToTable(t => t.HasCheckConstraint("CK_Platforms_Generation", "[Generation] >= 0"));
    }
}