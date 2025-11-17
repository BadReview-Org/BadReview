using FluentValidation;

using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Request;

public record CreateReviewRequest(
    int Rating,
    DateTime? StartDate,
    DateTime? EndDate,
    string? ReviewText,
    ReviewState StateEnum,
    bool IsFavorite,
    bool IsReview,
    int GameId
);

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        // Rating: entre 0 y 5
        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 5)
            .WithMessage("Rating must be a int between [0, 5]");

        // ReviewText: máximo 3000 caracteres
        RuleFor(x => x.ReviewText)
            .MaximumLength(3000)
            .WithMessage("The text review must not exceed 3000 characters.");

        // GameId: debe ser >= 0
        RuleFor(x => x.GameId)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Game Id can't be negative");

        // StartDate <= EndDate (si ambos no son null)
        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("Start Date must be before or equal than End Date.");

        // Condicionales según ReviewState
        When(x => x.StateEnum == ReviewState.WISHLIST, () =>
        {
            RuleFor(x => x.StartDate)
                .Must(x => x == null)
                .WithMessage("Start Date must be null when review state is WISHLIST.");

            RuleFor(x => x.EndDate)
                .Must(x => x == null)
                .WithMessage("End Date must be null when review state is WISHLIST.");
        });

        When(x => x.StateEnum == ReviewState.PLAYING, () =>
        {
            RuleFor(x => x.EndDate)
                .Must(x => x == null)
                .WithMessage("End Date must be null when review state is PLAYING.");
        });

        // No permitir ambos IsFavorite e IsReview en false
        RuleFor(x => x)
            .Must(x => x.IsFavorite || x.IsReview)
            .WithMessage("At least one of IsFavorite or IsReview must be true.");
    }
}