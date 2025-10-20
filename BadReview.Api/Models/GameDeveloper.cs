namespace BadReview.Api.Models
{
    public class GameDeveloper
    {
        public int GameId { get; set; }
        public int DeveloperId { get; set; }

        public Game Game { get; set; } = null!;
        public Developer Developer { get; set; } = null!;
    }
}
