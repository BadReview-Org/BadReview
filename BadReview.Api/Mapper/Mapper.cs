using BadReview.Api.Models;

using BadReview.Shared.DTOs.Request;
using BadReview.Shared.DTOs.Response;
using BadReview.Shared.DTOs.External;

namespace BadReview.Api.Mapper;

public static class Mapper
{
    public static Review CreateReviewModel(CreateReviewRequest req, int userId, int gameId)
    {
        return new Review
        {
            Rating = req.Rating,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            ReviewText = req.ReviewText,
            StateEnum = req.StateEnum,
            IsFavorite = req.IsFavorite,
            UserId = userId,
            GameId = gameId
        };
    }

    public static DetailReviewDto CreateReviewDto(Review model, int userId, string userName, string userEmail)
    {
        return new DetailReviewDto
        (
            model.Id,
            model.Rating,
            model.StartDate,
            model.EndDate,
            model.ReviewText,
            model.StateEnum,
            model.IsFavorite,
            CreateUserDto(userId, userName, userEmail),
            null
        );
    }

    public static BasicUserDto CreateUserDto(int id, string name, string email)
    {
        return new BasicUserDto(id, name, email);
    }

    public static GenreDto CreateGenreDto(GenreIgdbDto gen) => new GenreDto(gen.Id, gen.Name);
    public static GenreDto CreateGenreDto(Genre gen) => new GenreDto(gen.Id, gen.Name);

    public static DeveloperDto CreateDeveloperDto(CompanyIgdbDto dev) => new DeveloperDto(dev.Id, dev.Name);
    public static DeveloperDto CreateDeveloperDto(Developer dev) => new DeveloperDto(dev.Id, dev.Name);

    public static PlatformDto CreatePlatformDto(PlatformIgdbDto p) => new PlatformDto(p.Id, p.Name);
    public static PlatformDto CreatePlatformDto(Platform p) => new PlatformDto(p.Id, p.Name);


    public static Game CreateGameEntity(DetailGameIgdbDto g)
    {
        return new Game {
            Id = g.Id,
            Name = g.Name,
            Cover = g.Cover?.Url,
            Date = g.First_release_date,
            Summary = g.Summary,
            RatingIGDB = g.Rating ?? 0d,
            Total_RatingBadReview = 0,
            Count_RatingBadReview = 0,
            Video = g.Videos?.FirstOrDefault()?.Video_Id,
            Reviews = new List<Review>(),
            GameGenres = g.Genres?.Select(gen => new GameGenre { GameId = g.Id, GenreId = gen.Id }).ToList()
                ?? new List<GameGenre>(),
            GameDevelopers = g.Involved_Companies?
                .Where(c => c.Developer)
                .Select(dev => new GameDeveloper { GameId = g.Id, DeveloperId = dev.Id })
                .ToList()
                ?? new List<GameDeveloper>(),
            GamePlatforms = g.Platforms?
                .Select(p => new GamePlatform { GameId = g.Id, PlatformId = p.Id })
                .ToList()
                ?? new List<GamePlatform>()
        };
    }
    
    public static DetailGameDto CreateDetailGameDto(DetailGameIgdbDto g)
    {
        return new DetailGameDto(
            g.Id,
            g.Name,
            g.Cover?.Url,
            g.First_release_date,
            g.Summary,
            g.Rating ?? 0d,
            0,
            0,
            g.Videos?.FirstOrDefault()?.Video_Id,
            new List<DetailReviewDto>(),
            g.Genres is null ?
                new List<GenreDto>() :
                g.Genres.Select(gen => CreateGenreDto(gen)).ToList(),
            g.Involved_Companies is null ?
                new List<DeveloperDto>() :
                g.Involved_Companies.Where(c => c.Developer).Select(dev => CreateDeveloperDto(dev.Company)).ToList(),
            g.Platforms is null ?
                new List<PlatformDto>() :
                g.Platforms.Select(p => CreatePlatformDto(p)).ToList()
        );
    }

    public static IQueryable<DetailGameDto> GameToDetailDto(this IQueryable<Game> query)
    {
        return query.Select(g => new DetailGameDto(
            g.Id,
            g.Name,
            g.Cover,
            g.Date,
            g.Summary,
            g.RatingIGDB,
            g.Total_RatingBadReview,
            g.Count_RatingBadReview,
            g.Video,
            g.Reviews.Select(r => new DetailReviewDto(
                r.Id,
                r.Rating,
                r.StartDate,
                r.EndDate,
                r.ReviewText,
                r.StateEnum,
                r.IsFavorite,
                new BasicUserDto(
                    r.User.Id,
                    r.User.Username,
                    r.User.FullName
                ),
                null
            )).ToList(),
            g.GameGenres.Select(gg => CreateGenreDto(gg.Genre)
            ).ToList(),
            g.GameDevelopers.Select(gd => CreateDeveloperDto(gd.Developer)
            ).ToList(),
            g.GamePlatforms.Select(gp => CreatePlatformDto(gp.Platform)
            ).ToList()
        ));
    }
}