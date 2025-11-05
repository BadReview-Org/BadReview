using BadReview.Api.Models.Owned;

namespace BadReview.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    
    public string? FullName { get; set; } = null!;
    public DateTime? Birthday { get; set; }
    public int? Country { get; set; }
    public CUDate Date { get; set; } = null!;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}