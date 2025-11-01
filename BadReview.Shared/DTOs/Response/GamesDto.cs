namespace BadReview.Shared.DTOs.Response;

public record DetailGameDto(
    int Id,
    string Name,
    string? Cover,
    long? Date,
    string? Summary,
    double? RatingIGDB,
    long Total_RatingBadReview,
    long Count_RatingBadReview,
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
    double? RatingIGDB,
    long Total_RatingBadReview,
    long Count_RatingBadReview
);