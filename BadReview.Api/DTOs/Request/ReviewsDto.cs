using BadReview.Api.Utils;
namespace BadReview.Api.DTOs.Request;

public record CreateReviewRequest(
    int? Rating,
    DateTime? StartDate,
    DateTime? EndDate,
    string? ReviewText,
    ReviewState StateEnum,
    bool IsFavorite,
    int GameId
);