using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Request;

public record CreateUserRequest(
    string Username,
    string Email,
    string FullName,
    DateTime? Birthday,
    string? Country
);

public record LoginUserRequest(
    string Username,
    string Password
);

public record RegisterUserRequest(
    string Username,
    string Email,
    string Password,
    string FullName
);