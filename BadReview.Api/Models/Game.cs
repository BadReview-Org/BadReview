using BadReview.Api.Models.Owned;

namespace BadReview.Api.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public long? Date { get; set; }
    public string? Summary { get; set; }
    public double RatingIGDB { get; set; } = 0d;
    public long Total_RatingBadReview { get; set; } = 0;
    public long Count_RatingBadReview { get; set; } = 0;
    public string? Video { get; set; }
    public Image? Cover { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
    public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
    public ICollection<GameDeveloper> GameDevelopers { get; set; } = new List<GameDeveloper>();
}