using BadReview.Api.DTOs.User;
using BadReview.Api.DTOs.Game;

namespace BadReview.Api.DTOs.Review;

public record ReviewWithGameDto(
    int Id,
    int Rating,
    DateTime StartDate,
    Datetime EndDate,
    string ReviewText
    string StateEnum,
    bool IsFavorite,
    UserDto User,
    GameDto Game
);

public record ReviewDto(
    int Id,
    int Rating,
    DateTime StartDate,
    Datetime EndDate,
    string ReviewText
    string StateEnum,
    bool IsFavorite,
    UserDto User
)