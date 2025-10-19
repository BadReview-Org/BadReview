using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BadReview.Api.Data
{
    public class BadReviewContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=BadReviewDb;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        public DbSet<User> Users { get; set; } = null!;
    }
}