using FluentValidation;

using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Request;

public record LoginUserRequest(
    string Username,
    string Password
);

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string? FullName,
    DateTime? Birthday,
    int? Country
);

public record UserCheckAvailable(string? Username, string? Email);

public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        // Username
        RuleFor(x => x.Username).UsernameRule();

        // Password
        RuleFor(x => x.Password).PasswordRule();
    }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        // Username
        RuleFor(x => x.Username).UsernameRule();

        // Password
        RuleFor(x => x.Password).PasswordRule();

        // Email
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(30)
            .Matches("^[a-zA-Z0-9@._-]+$")
                .WithMessage("Email has forbidden characters.")
            .Matches(@"^[^\s@]{3,}@[a-zA-Z0-9_-]{2,}\.[a-zA-Z0-9_-]+$")
                .WithMessage("Invalid email format (correct example: user@domain.com).")
            .Must(email => email.Count(c => c == '@') == 1)
                .WithMessage("Email must contain exactly one '@' character.");

        // FullName (solo letras y espacios, 40 caracteres máx)
        RuleFor(x => x.FullName)
            .MaximumLength(40)
            .Matches(@"^[a-zA-Z ]*$")
                .When(x => !string.IsNullOrEmpty(x.FullName))
                .WithMessage("Full name must only contain letters and whitespaces.");

        // Birthday (mínimo 12 años)
        RuleFor(x => x.Birthday)
            .Must(CommonValidators.BeAtLeast12YearsOld)
                .When(x => x.Birthday.HasValue)
                .WithMessage("User must be at least 12 years old.");

        // Country (>= 0)
        RuleFor(x => x.Country)
            .GreaterThanOrEqualTo(0)
                .When(x => x.Country.HasValue)
                .WithMessage("Country code can't be negative.");
    }
}