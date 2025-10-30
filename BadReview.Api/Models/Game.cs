namespace BadReview.Api.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Cover { get; set; }
    public long? Date { get; set; }
    public string? Summary { get; set; }
    public double RatingIGDB { get; set; } = 0d;
    public double RatingBadReview { get; set; } = 0d;
    public string? Video { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
    public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
    public ICollection<GameDeveloper> GameDevelopers { get; set; } = new List<GameDeveloper>();
}