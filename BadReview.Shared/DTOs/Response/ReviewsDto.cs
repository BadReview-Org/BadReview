using System.Text.Json.Serialization;

using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Response;

public record DetailReviewDto(
    int Id,
    int Rating,
    DateTime? StartDate,
    DateTime? EndDate,
    string? ReviewText,
    ReviewState StateEnum,
    bool IsFavorite,
    bool IsReview,
    [property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    BasicUserDto? User,
    BasicGameDto? Game,
    // could be replaced by a CUDateDto
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record BasicReviewDto(
    int Id,
    int Rating,
    string? ReviewText,
    ReviewState StateEnum,
    bool IsFavorite,
    bool IsReview,
    [property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    BasicUserDto? User,
    BasicGameDto? Game,
    DateTime UpdatedAt
);