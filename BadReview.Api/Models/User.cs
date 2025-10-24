namespace BadReview.Api.Models;
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateTime? Birthday { get; set; }
    public string? Country { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}