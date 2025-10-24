namespace BadReview.Api.DTOs.Response;

public record GameDto(
    int Id,
    string Name,
    string? Cover,
    DateTime? Date,
    string? Summary,
    double RatingIGDB,
    double RatingBadReview,
    string? Video,
    List<GenreDto> Genres,
    List<DeveloperDto> Developers,
    List<PlatformDto> Platforms
);

public record GameWithReviewsDto(
    int Id,
    string Name,
    string? Cover,
    DateTime? Date,
    string? Summary,
    double RatingIGDB,
    double RatingBadReview,
    string? Video,
    List<ReviewDto> Reviews,
    List<GenreDto> Genres,
    List<DeveloperDto> Developers,
    List<PlatformDto> Platforms
);