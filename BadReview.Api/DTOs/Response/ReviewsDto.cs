namespace BadReview.Api.DTOs.Review;

public class ReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string ReviewText { get; set; } = null!;
    public string StateEnum { get; set; } = null!;
    public bool IsFavorite { get; set; }
    public UserDto User { get; set; } = null!;
}