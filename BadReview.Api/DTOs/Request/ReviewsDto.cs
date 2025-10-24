namespace BadReview.Api.DTOs.Request;

public record CreateReviewRequest(
    int Rating,
    DateTime StartDate,
    DateTime EndDate,
    string ReviewText,
    string StateEnum,
    bool IsFavorite,
    int GameId
);