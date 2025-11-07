using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using BadReview.Api.Data;
using BadReview.Shared.DTOs.External;
using BadReview.Shared.DTOs.Request;
using BadReview.Shared.Utils;
using BadReview.Shared.DTOs.Response;

using static BadReview.Api.Mapper.Mapper;
using BadReview.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BadReview.Api.Services;

public class ReviewService : IReviewService
{
    private readonly IIGDBService _igdb;
    private readonly BadReviewContext _db;

    public ReviewService(IIGDBService igdb, BadReviewContext db)
    {
        _igdb = igdb;
        _db = db;
    }

    public async Task<PagedResult<DetailReviewDto>> GetReviewsAsync(PaginationRequest pag)
    {
        var page = pag.Page ?? CONSTANTS.DEF_PAGE;
        var pageSize = pag.PageSize ?? CONSTANTS.DEF_PAGESIZE;

        var count = await _db.Reviews.CountAsync();

        var reviews = await _db.Reviews
            .OrderBy(r => r.Id)
            .Skip(page * pageSize)
            .Take(pageSize)
            .Include(r => r.User)
            .ToListAsync();

        if (reviews is null)
            return new PagedResult<DetailReviewDto>(new(), count, page, pageSize);


        List<DetailReviewDto> reviewList = reviews
            .Select(r => new DetailReviewDto(
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
                ))
            .ToList();

        return new PagedResult<DetailReviewDto>(reviewList, count, page, pageSize);
    }

    public async Task<DetailReviewDto?> GetReviewByIdAsync(int id)
    {
        var review = await _db.Reviews
            .Include(r => r.User)
            .Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review is null) return null;


        var reviewDto = new DetailReviewDto(
            review.Id,
            review.Rating,
            review.StartDate,
            review.EndDate,
            review.ReviewText,
            review.StateEnum,
            review.IsFavorite,
            new BasicUserDto(
                review.User.Id,
                review.User.Username,
                review.User.FullName
            ),
            new BasicGameDto(
                review.Game.Id,
                review.Game.Name,
                review.Game.Cover?.ImageId,
                review.Game.Cover?.ImageHeight,
                review.Game.Cover?.ImageWidth,
                review.Game.RatingIGDB,
                review.Game.Total_RatingBadReview,
                review.Game.Count_RatingBadReview
            ),
            review.Date.CreatedAt, review.Date.UpdatedAt
        );

        return reviewDto;
    }

    public async Task<(ReviewCode, DetailReviewDto?)> UpdateReviewAsync(int reviewId, int userId, CreateReviewRequest updatedReview)
    {
        var review = await _db.Reviews.Include(r => r.User)
            .Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review is null) return (ReviewCode.REVIEWNOTFOUND, null);
        if (userId != review.UserId) return (ReviewCode.USERNOTMATCH, null);


        review.Game.Total_RatingBadReview -= review.Rating ?? 0;
        review.Game.Total_RatingBadReview += updatedReview.Rating ?? 0;

        review.Rating = updatedReview.Rating;
        review.StartDate = updatedReview.StartDate;
        review.EndDate = updatedReview.EndDate;
        review.ReviewText = updatedReview.ReviewText;
        review.StateEnum = updatedReview.StateEnum;
        review.IsFavorite = updatedReview.IsFavorite;

        await _db.SaveChangesAsync();

        var reviewDto = new DetailReviewDto
        (
            review.Id,
            review.Rating,
            review.StartDate,
            review.EndDate,
            review.ReviewText,
            review.StateEnum,
            review.IsFavorite,
            new BasicUserDto(
                review.User.Id,
                review.User.Username,
                review.User.FullName
            ),
            new BasicGameDto(
                review.Game.Id,
                review.Game.Name,
                review.Game.Cover?.ImageId,
                review.Game.Cover?.ImageHeight,
                review.Game.Cover?.ImageWidth,
                review.Game.RatingIGDB,
                review.Game.Total_RatingBadReview,
                review.Game.Count_RatingBadReview
            ),
            review.Date.CreatedAt, review.Date.UpdatedAt
        );

        return (ReviewCode.OK, reviewDto);
    }

    public async Task<ReviewCode> DeleteReviewAsync(int reviewId, int userId)
    {
        var review = await _db.Reviews.Include(r => r.Game)
            .FirstOrDefaultAsync(r => r.Id == reviewId);

        if (review is null) return ReviewCode.REVIEWNOTFOUND;
        if (userId != review.UserId) return ReviewCode.USERNOTMATCH;


        review.Game.Total_RatingBadReview -= review.Rating ?? 0;
        review.Game.Count_RatingBadReview--;
        
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        return ReviewCode.OK;
    }

    public async Task<(ReviewCode, DetailReviewDto?)> CreateReviewAsync(CreateReviewRequest newReview, User user)
    {
        var game = await _db.Games.FindAsync(newReview.GameId);

        if (game is null) return (ReviewCode.GAMENOTFOUND, null);

        if (user.Reviews.Select(r => r.GameId).Contains(game.Id))
            return (ReviewCode.USERALREADYHASREVIEW, null);


        game.Total_RatingBadReview += newReview.Rating ?? 0;
        game.Count_RatingBadReview++;

        var reviewDb = new Review
        {
            Rating = newReview.Rating,
            StartDate = newReview.StartDate,
            EndDate = newReview.EndDate,
            ReviewText = newReview.ReviewText,
            StateEnum = newReview.StateEnum,
            IsFavorite = newReview.IsFavorite,
            UserId = user.Id,
            GameId = game.Id
        };

        _db.Reviews.Add(reviewDb);
        await _db.SaveChangesAsync();

        /*reviewdb = await db.Reviews
            .AsNoTracking()
            .FirstAsync(r => r.Id == reviewdb.Id);*/
        reviewDb.Date = await _db.Reviews
            .Where(r => r.Id == reviewDb.Id)
            .Select(r => r.Date)
            .AsNoTracking()
            .FirstAsync();

        var reviewDto = new DetailReviewDto
        (
            reviewDb.Id,
            reviewDb.Rating,
            reviewDb.StartDate,
            reviewDb.EndDate,
            reviewDb.ReviewText,
            reviewDb.StateEnum,
            reviewDb.IsFavorite,
            new BasicUserDto(
                user.Id,
                user.Username,
                user.FullName
            ),
            new BasicGameDto(
                game.Id,
                game.Name,
                game.Cover?.ImageId,
                game.Cover?.ImageHeight,
                game.Cover?.ImageWidth,
                game.RatingIGDB,
                game.Total_RatingBadReview,
                game.Count_RatingBadReview
            ),
            reviewDb.Date.CreatedAt, reviewDb.Date.UpdatedAt
        );

        return (ReviewCode.OK, reviewDto);
    }
}