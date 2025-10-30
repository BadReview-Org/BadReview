using BadReview.Shared.Utils;

namespace BadReview.Api.Models;

public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GameId { get; set; }
    public int? Rating { get; set; } = 0;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? ReviewText { get; set; }
    public ReviewState StateEnum { get; set; } = ReviewState.NONE;
    public bool IsFavorite { get; set; } = false;

    public User User { get; set; } = null!;
    public Game Game { get; set; } = null!;
}