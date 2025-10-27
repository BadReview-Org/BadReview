using BadReview.Api.Utils;
namespace BadReview.Api.DTOs.Request;
public record CreateUserRequest(
    string Username,
    string Email,
    string FullName,
    DateTime? Birthday,
    string? Country
);

public record LoginUserRequest(
    string Username,
    string Email,
    string Password,
    string FullName
);

public record RegisterUserRequest(
    string Username,
    string Email,
    string Password,
    string FullName
);