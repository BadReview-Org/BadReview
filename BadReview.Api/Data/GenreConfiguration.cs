using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        // NO usar IDENTITY para mantener IDs de IGDB
        builder.Property(e => e.Id).ValueGeneratedNever();  

        builder.Property(e => e.Name)
            .HasMaxLength(200);
    }
}