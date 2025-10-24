namespace BadReview.Api.DTOs.Response;

public record ReviewWithGameDto(
    int Id,
    int Rating,
    DateTime StartDate,
    DateTime EndDate,
    string ReviewText,
    string StateEnum,
    bool IsFavorite,
    UserDto User,
    GameDto Game
);

public record ReviewDto(
    int Id,
    int Rating,
    DateTime StartDate,
    DateTime EndDate,
    string ReviewText,
    string StateEnum,
    bool IsFavorite,
    UserDto User
);