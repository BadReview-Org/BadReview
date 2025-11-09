using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Request;

public record LoginUserRequest(
    string Username,
    string Password
);

public record CreateUserRequest(
    string Username,
    string Email,
    string? Password,
    string? FullName,
    DateTime? Birthday,
    int? Country
);