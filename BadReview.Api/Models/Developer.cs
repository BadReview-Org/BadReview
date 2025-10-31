namespace BadReview.Api.Models;

public class Developer
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int? Country { get; set; }
    public string? Logo { get; set; }

    public ICollection<GameDeveloper> GameDevelopers { get; set; } = new List<GameDeveloper>();
}