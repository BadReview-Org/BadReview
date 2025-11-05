namespace BadReview.Shared.DTOs.Response;

public record DetailUserDto(
    int Id,
    string Username,
    string? FullName,
    DateTime? Birthday,
    int? Country,
    List<DetailReviewDto> Reviews,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record BasicUserDto(
    int Id,
    string Username,
    string? FullName
);

public record RegisterUserDto(BasicUserDto UserDto, string Token);

public record LoginUserDto(string Token);