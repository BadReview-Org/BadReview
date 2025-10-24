namespace BadReview.Api.DTOs.Game;

public class GameWithReviewsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Cover { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Summary { get; set; } = null!;
    public double RatingIGDB { get; set; }
    public double RatingBadReview { get; set; }
    public string Video { get; set; } = null!;
    public List<ReviewDto> Reviews { get; set; } = new();
    public List<GenreDto> Genres { get; set; } = new();
    public List<DeveloperDto> Developers { get; set; } = new();
    public List<PlatformDto> Platforms { get; set; } = new();
}