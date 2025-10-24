using BadReview.Api.DTOs.Review;
using BadReview.Api.DTOs.Genre;
using BadReview.Api.DTOs.Developer;
using BadReview.Api.DTOs.Platform;

namespace BadReview.Api.DTOs.Game;

public record GameDto(
    int Id,
    string Name,
    string Cover,
    Datetime Date,
    string Summary,
    double RatingIGDB,
    double RatingBadReview,
    string Video,
    List<GenreDto> Genres,
    List<DeveloperDto> Developers,
    List<PlatformDto> Platforms
);

public record GameWithReviewsDto(
    int Id,
    string Name,
    string Cover,
    Datetime Date,
    string Summary,
    double RatingIGDB,
    double RatingBadReview,
    string Video,
    List<ReviewDto> Reviews,
    List<GenreDto> Genres,
    List<DeveloperDto> Developers,
    List<PlatformDto> Platforms
);