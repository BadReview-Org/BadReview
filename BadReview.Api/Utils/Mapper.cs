using BadReview.Api.DTOs.Request;
using BadReview.Api.DTOs.Response;
using BadReview.Api.DTOs.External;
using BadReview.Api.Models;

namespace BadReview.Api.Utils;

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

    public static ReviewDto CreateReviewDto(Review model, int userId, string userName, string userEmail)
    {
        return new ReviewDto
        (
            model.Id,
            model.Rating,
            model.StartDate,
            model.EndDate,
            model.ReviewText,
            model.StateEnum,
            model.IsFavorite,
            CreateUserDto(userId, userName, userEmail)
        );
    }

    public static UserDto CreateUserDto(int id, string name, string email)
    {
        return new UserDto(id, name, email);
    }
}