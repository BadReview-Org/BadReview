using BadReview.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BadReview.Api.Data
{
    public class BadReviewContext : DbContext
    {
        public BadReviewContext(DbContextOptions<BadReviewContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
    }
}