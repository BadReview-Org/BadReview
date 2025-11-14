using System.Security.Cryptography.X509Certificates;
using BadReview.Shared.DTOs.Request;
using FluentValidation;

namespace BadReview.Client.Utils;

public class LoginForm
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterForm
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string RepeatPassword { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FullName { get; set; } = null;
    public DateTime? Birthday { get; set; } = null;
    public IsoCountry? Country { get; set; } = null;
}

public class LoginFormValidator : AbstractValidator<LoginForm>
{
    public LoginFormValidator()
    {
        // Username
        RuleFor(x => x.Username).Cascade(CascadeMode.Stop).UsernameRule();

        // Password
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop).PasswordRule();
    }
}

public class RegisterFormValidator : AbstractValidator<RegisterForm>
{
    public RegisterFormValidator()
    {
        // Username
        RuleFor(x => x.Username).Cascade(CascadeMode.Stop).UsernameRule();

        // Password
        RuleFor(x => x.Password).Cascade(CascadeMode.Stop).PasswordRule();
        RuleFor(x => x.RepeatPassword).PasswordRule();

        /*RuleFor(x => x)
            .Must(x => x.Password is null || x.RepeatPassword is null ||
                  x.Password == x.RepeatPassword);*/

        // Email
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop)
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
                .WithMessage("Full name is too long: must contain at most 40 characters.")
            .Matches(@"^[a-zA-Z ]*$")
                .WithMessage("Full name must only contain letters and whitespaces.");

        // Birthday (mínimo 12 años)
        RuleFor(x => x.Birthday)
            .Must(CommonValidators.BeAtLeast12YearsOld)
                .When(x => x.Birthday.HasValue)
                .WithMessage("User must be at least 12 years old.");

        // Country (>= 0)
        /*RuleFor(x => x.Country?.Country_code)
            .GreaterThanOrEqualTo(0)
                .When(x => x.Country)
                .WithMessage("Country code can't be negative.");*/
    }
}