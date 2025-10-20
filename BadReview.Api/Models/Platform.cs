namespace BadReview.Api.Models
{
    public class Platform
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Abbreviation { get; set; } = null!;
        public string Generation { get; set; } = null!;
        public string Logo { get; set; } = null!;

        public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
    }
}
