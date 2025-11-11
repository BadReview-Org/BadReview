using BadReview.Api.Models.Owned;
using BadReview.Shared.Utils;

namespace BadReview.Api.Models;

public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; } // par unico userid - gameid
    public int GameId { get; set; }
    public int Rating { get; set; } = 0; // de 0 a 5
    public DateTime? StartDate { get; set; } // end date > start date (si ambos != null)
    public DateTime? EndDate { get; set; }
    public string? ReviewText { get; set; } // 1000 caracteres max

    // si PLAYING solo start date, si WISHLIST nada, si PLAYED o NONE start y end date
    public ReviewState StateEnum { get; set; } = ReviewState.NONE;
    public bool IsFavorite { get; set; } = false;
    public bool IsReview { get; set; } = false;
    public CUDate Date { get; set; } = null!;

    public User User { get; set; } = null!;
    public Game Game { get; set; } = null!;
}