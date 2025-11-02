namespace BadReview.Shared.DTOs.Response;

public record UserDto(
    int Id,
    string Username,
    string FullName,
    DateTime? Birthday,
    string? Country,
    List<DetailReviewDto>? Reviews
);

public record BasicUserDto(
    int Id,
    string Username,
    string FullName
);

public class LoginUserDto
{
    public string? Token { get; set; }
}
