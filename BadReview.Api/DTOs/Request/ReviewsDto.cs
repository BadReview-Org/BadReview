using BadReview.Api.DTOs.User;
using BadReview.Api.DTOs.Game;

namespace BadReview.Api.DTOs.Review;

public record CreateReviewRequest(
    int Rating,
    DateTime StartDate,
    DateTime EndDate,
    string ReviewText,
    string StateEnum,
    bool IsFavorite,
    int GameId
);