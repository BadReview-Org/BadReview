using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Request;

public record CreateReviewRequest(
    int? Rating,
    DateTime? StartDate,
    DateTime? EndDate,
    string? ReviewText,
    ReviewState StateEnum,
    bool IsFavorite,
    bool IsReview,
    int GameId
);