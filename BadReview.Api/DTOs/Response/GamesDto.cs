namespace BadReview.Api.DTOs.Response;

public record DetailGameDto(
    int Id,
    string Name,
    string? Cover,
    DateTime? Date,
    string? Summary,
    double RatingIGDB,
    double RatingBadReview,
    string? Video,
    List<DetailReviewDto> Reviews,
    List<GenreDto> Genres,
    List<DeveloperDto> Developers,
    List<PlatformDto> Platforms
);

public record BasicGameDto(
    int Id,
    string Name,
    string? Cover,
    double RatingIGDB,
    double RatingBadReview
);

