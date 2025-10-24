namespace BadReview.Api.Models;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<GameGenre> GameGenres { get; set; } = new List<GameGenre>();
}