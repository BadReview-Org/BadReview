using System.Text.Json.Serialization;
using BadReview.Api.Utils;

namespace BadReview.Api.DTOs.Response;



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


