namespace BadReview.Shared.DTOs.Response;

public record DetailGameDto(
    int Id,
    string Name,
    string? CoverId, int? CoverHeight, int? CoverWidth,
    long? Date,
    string? Summary,
    double? RatingIGDB,
    long Total_RatingBadReview,
    long Count_RatingBadReview,
    string? Video,
    List<DetailReviewDto> Reviews,
    List<GenreDto> Genres,
    List<DetailDeveloperDto> Developers,
    List<PlatformDto> Platforms
);

public record BasicGameDto(
    int Id,
    string Name,
    string? CoverId, int? CoverHeight, int? CoverWidth,
    double? RatingIGDB,
    long Total_RatingBadReview,
    long Count_RatingBadReview
);