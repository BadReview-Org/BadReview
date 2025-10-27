using BadReview.Api.Utils;
namespace BadReview.Api.DTOs.Request;
public record CreateUserRequest(
    string Username,
    string Email,
    string FullName,
    DateTime? Birthday,
    string? Country
);