using System.Text.Json.Serialization;

using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Response;


public record DetailReviewDto(
    int Id,
    int? Rating,
    DateTime? StartDate,
    DateTime? EndDate,
    string? ReviewText,
    ReviewState StateEnum,
    bool IsFavorite,
    [property:JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    BasicUserDto? User,
    BasicGameDto? Game
);


