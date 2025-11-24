using BadReview.Api.Models;
using BadReview.Shared.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();

        builder.OwnsOne(e => e.Date, date =>
        {
            date.Property(d => d.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAdd();
            date.Property(d => d.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP").ValueGeneratedOnAddOrUpdate();
        });

        builder.Property(e => e.Username)
            .HasMaxLength(20);

        builder.Property(e => e.Email)
            .HasMaxLength(40);

        builder.Property(e => e.FullName)
            .HasMaxLength(60);

        builder.Property(e => e.Country)
            .HasDefaultValue(0);

        builder.Property(e => e.Birthday)
            .HasColumnType("timestamp without time zone");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Users_Country", "\"Country\" >= 0");

            // Username: solo a-z, 0-9 y ._-
            t.HasCheckConstraint("CK_Users_Username_ValidChars", 
                "\"Username\" ~ '^[a-zA-Z0-9._-]+$'");

            // Email:
            t.HasCheckConstraint("CK_Users_Email_Format",
                "\"Email\" NOT LIKE '% %' " + // sin espacios
                "AND \"Email\" LIKE '___%@__%._%' " + // secuencia (al menos 3 caracteres, @, al menos 2, ., al menos 1)
                "AND (LENGTH(\"Email\") - LENGTH(REPLACE(\"Email\", '@', ''))) = 1 " + // exactamente un '@'
                "AND \"Email\" ~ '^[a-zA-Z0-9@._-]+$'"); // sin caracteres especiales

            // Full name: solo letras y espacios
            t.HasCheckConstraint("CK_Users_FullName_AlphaSpace",
                "\"FullName\" IS NULL OR \"FullName\" ~ '^[a-zA-Z ]+$'");

            // Birthday: edad >= 12 años
            t.HasCheckConstraint("CK_Users_Birthday_MinAge",
                "\"Birthday\" IS NULL OR EXTRACT(YEAR FROM AGE(CURRENT_TIMESTAMP, \"Birthday\")) >= 12");
        });
    }
}