namespace BadReview.Shared.DTOs.Response;

public record PrivateUserDto(
    int Id,
    string Username,
    string? FullName,
    DateTime? Birthday,
    int? Country,
    PagedResult<DetailReviewDto> Reviews,
    PagedResult<DetailReviewDto> Favorites,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record PublicUserDto(
    int Id,
    string Username,
    int? Country,
    PagedResult<BasicReviewDto> Reviews,
    PagedResult<BasicReviewDto> Favorites,
    DateTime CreatedAt
);

public record BasicUserDto(
    int Id,
    string Username
);

public record RegisterUserDto(BasicUserDto UserDto, UserTokensDto LoginDto);

public record UserTokensDto(string AccessToken, string RefreshToken);