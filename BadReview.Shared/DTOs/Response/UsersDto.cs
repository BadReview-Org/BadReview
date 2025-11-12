namespace BadReview.Shared.DTOs.Response;

public interface IUserDto
{
    int Id { get; }
    string Username { get; }
    int? Country { get; }
    PagedResult<DetailReviewDto> Reviews { get; }
    PagedResult<DetailReviewDto> Favorites { get; }
    DateTime CreatedAt { get; }
}


public record PrivateUserDto(
    int Id,
    string Username,
    string Email,
    string? FullName,
    DateTime? Birthday,
    int? Country,
    PagedResult<DetailReviewDto> Reviews,
    PagedResult<DetailReviewDto> Favorites,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IUserDto;

public record PublicUserDto(
    int Id,
    string Username,
    int? Country,
    PagedResult<DetailReviewDto> Reviews,
    PagedResult<DetailReviewDto> Favorites,
    DateTime CreatedAt
) : IUserDto;

public record BasicUserDto(
    int Id,
    string Username
) ;
public record RegisterUserDto(BasicUserDto UserDto, UserTokensDto LoginDto);
public record UserTokensDto(string AccessToken, string RefreshToken);