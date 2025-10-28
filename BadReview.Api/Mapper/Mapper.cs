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
}