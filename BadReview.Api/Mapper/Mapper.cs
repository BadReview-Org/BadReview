using BadReview.Api.Models;
using BadReview.Api.Models.Owned;

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
            null,
            DateTime.Now,
            DateTime.Now
        );
    }

    public static BasicUserDto CreateUserDto(int id, string name, string email)
    {
        return new BasicUserDto(id, name, email);
    }

    public static GenreDto CreateGenreDto(GenreIgdbDto gen) => new GenreDto(gen.Id, gen.Name);
    public static GenreDto CreateGenreDto(Genre gen) => new GenreDto(gen.Id, gen.Name);
    public static Genre CreateGenreEntity(GenreIgdbDto gen) => new Genre { Id = gen.Id, Name = gen.Name };

    public static BasicDeveloperDto CreateDeveloperDto(BasicCompanyIgdbDto dev) =>
        new BasicDeveloperDto(dev.Id, dev.Name, dev.Country, dev.Logo?.Image_Id, dev.Logo?.Height, dev.Logo?.Width);
    public static DetailDeveloperDto CreateDeveloperDto(Developer dev) =>
        new DetailDeveloperDto(
            dev.Id, dev.Name, dev.Country, dev.Description, dev.StartDate,
            dev.Logo?.ImageId, dev.Logo?.ImageHeight, dev.Logo?.ImageWidth, null);

    public static DetailDeveloperDto CreateDeveloperDto(DetailCompanyIgdbDto dev) =>
        new DetailDeveloperDto(
            dev.Id, dev.Name, dev.Country, dev.Description, dev.Start_date,
            dev.Logo?.Image_Id, dev.Logo?.Height, dev.Logo?.Width, null);

    public static Developer CreateDeveloperEntity(DetailCompanyIgdbDto c) =>
        new Developer
        {
            Id = c.Id,
            Name = c.Name,
            Country = c.Country,
            Description = c.Description,
            StartDate = c.Start_date,
            Logo = c.Logo?.Image_Id is not null ?
                new Image(c.Logo.Image_Id, c.Logo.Height, c.Logo.Width) : null
        };

    public static BasicPlatformDto CreatePlatformDto(BasicPlatformIgdbDto p) =>
        new BasicPlatformDto(
            p.Id, p.Name, p.Abbreviation,
            p.Platform_logo?.Image_Id, p.Platform_logo?.Height, p.Platform_logo?.Width);
    public static DetailPlatformDto CreatePlatformDto(Platform p)
        => new DetailPlatformDto(p.Id, p.Name, p.Abbreviation, p.Generation, p.Summary,
            p.Logo?.ImageId, p.Logo?.ImageHeight, p.Logo?.ImageWidth, null);
    public static DetailPlatformDto CreatePlatformDto(DetailPlatformIgdbDto p) =>
        new DetailPlatformDto(
            p.Id, p.Name, p.Abbreviation, p.Generation, p.Summary,
            p.Platform_logo?.Image_Id, p.Platform_logo?.Height, p.Platform_logo?.Width, null);

    public static Platform CreatePlatformEntity(DetailPlatformIgdbDto p) =>
        new Platform {
            Id = p.Id,
            Name = p.Name,
            Abbreviation = p.Abbreviation,
            Generation = p.Generation,
            Summary = p.Summary,
            Logo = p.Platform_logo?.Image_Id is not null ?
                new Image(p.Platform_logo.Image_Id, p.Platform_logo.Height, p.Platform_logo.Width) : null
        };

    public static Game CreateGameEntity(DetailGameIgdbDto g)
    {
        return new Game {
            Id = g.Id,
            Name = g.Name,
            Cover = g.Cover?.Image_Id is not null ?
                new Image(g.Cover.Image_Id, g.Cover.Height, g.Cover.Width) : null,
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
                .Select(dev => new GameDeveloper { GameId = g.Id, DeveloperId = dev.Company.Id })
                .ToList()
                ?? new List<GameDeveloper>(),
            GamePlatforms = g.Platforms?
                .Select(p => new GamePlatform { GameId = g.Id, PlatformId = p.Id })
                .ToList()
                ?? new List<GamePlatform>()
        };
    }
    public static BasicGameDto CreateBasicGameDto(BasicGameIgdbDto g)
    {
        return new BasicGameDto(
            g.Id,
            g.Name,
            g.Cover?.Image_Id, g.Cover?.Height, g.Cover?.Width,
            g.Rating,
            0,
            0
        );
    }
    public static DetailGameDto CreateDetailGameDto(DetailGameIgdbDto g)
    {
        return new DetailGameDto(
            g.Id,
            g.Name,
            g.Cover?.Image_Id, g.Cover?.Height, g.Cover?.Width,
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
                new List<DetailDeveloperDto>() :
                g.Involved_Companies.Where(c => c.Developer).Select(dev => CreateDeveloperDto(dev.Company)).ToList(),
            g.Platforms is null ?
                new List<DetailPlatformDto>() :
                g.Platforms.Select(p => CreatePlatformDto(p)).ToList()
        );
    }

    public static IQueryable<DetailGameDto> GameToDetailDto(this IQueryable<Game> query)
    {
        return query.Select(g => new DetailGameDto(
            g.Id,
            g.Name,
            g.Cover != null ? g.Cover.ImageId : null,
            g.Cover != null ? g.Cover.ImageHeight : null,
            g.Cover != null ? g.Cover.ImageWidth : null,
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
                null,
                r.Date.CreatedAt, r.Date.UpdatedAt
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