namespace BadReview.Api.Models;

public class GamePlatform
{
    public int GameId { get; set; }
    public int PlatformId { get; set; }

    public Game Game { get; set; } = null!;
    public Platform Platform { get; set; } = null!;
}