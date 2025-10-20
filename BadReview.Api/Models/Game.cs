namespace BadReview.Api.Models
{
    public class Game
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Cover { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Summary { get; set; } = null!;
        public double RatingIGDB { get; set; }
        public double RatingBadReview { get; set; }
        public string Video { get; set; } = null!;

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
        public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
        public ICollection<GameDeveloper> GameDevelopers { get; set; } = new List<GameDeveloper>();
    }
}
