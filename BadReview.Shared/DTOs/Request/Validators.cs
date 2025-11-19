using FluentValidation;

namespace BadReview.Shared.DTOs.Request;

// Common validators rules
public static class ValidatorRules
{
    public static IRuleBuilderOptions<T, string> UsernameLoginRule<T>
    (this IRuleBuilder<T, string> rule) where T : IUsername
    {
        return rule
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 20).WithMessage("Username must have [3-20] characters.")
            .Matches("^[a-zA-Z0-9._-]+$")
                .WithMessage("Username must only contain letters, numbers and characters (- _ .)");
    }

    public static IRuleBuilderOptions<T, string> UsernameRegisterRule<T>
    (this IRuleBuilder<T, string> rule, ICheckAvailables checker) where T : IUsername
    {
        return rule
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 20).WithMessage("Username must have [3-20] characters.")
            .Matches("^[a-zA-Z0-9._-]+$")
                .WithMessage("Username must only contain letters, numbers and characters (- _ .)")
            .MustAsync(async (username, ct) => await checker.UsernameAvailable(username))
                .WithMessage("Username is already in use. Pick another one.")
                .WithErrorCode("USERNAMENOTAVAILABLE")
                .When(x => !string.IsNullOrWhiteSpace(x.Username), ApplyConditionTo.CurrentValidator);
    }

    public static IRuleBuilderOptions<T, string> UsernameUpdateRule<T>
    (this IRuleBuilder<T, string> rule, ICheckAvailables checker, string? originalUsername) where T : IUsername
    {
        return rule
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 20).WithMessage("Username must have [3-20] characters.")
            .Matches("^[a-zA-Z0-9._-]+$")
                .WithMessage("Username must only contain letters, numbers and characters (- _ .)")
            .MustAsync(async (username, ct) => await checker.UsernameAvailable(username))
                .WithMessage("Username is already in use. Pick another one.")
                .WithErrorCode("USERNAMENOTAVAILABLE")
                .When(x => x.Username != originalUsername && !string.IsNullOrWhiteSpace(x.Username),
                      ApplyConditionTo.CurrentValidator);
    }

    public static IRuleBuilderOptions<T, string> PasswordRequiredRule<T>
    (this IRuleBuilder<T, string> rule) where T : IPassword
    {
        return rule
            .NotEmpty().WithMessage("Password is required.")
            .Length(6, 20).WithMessage("Password must have [6-20] characters.")
            // Al menos una minúscula
            .Matches("[a-z]").WithMessage("Password must have at least one LOWER-case letter.")
            // Al menos una mayúscula
            .Matches("[A-Z]").WithMessage("Password must have at least one UPPER-case letter.")
            // Al menos un número
            .Matches("[0-9]").WithMessage("Password must have at least one number (0-9).")
            // Al menos un caracter especial
            .Matches("[!@#$%^&*(),.?\":{}|<>\\[\\]\\\\/'`~;_+=-]")
                .WithMessage("Password must have at least one special character.")
            // No permitir otros caracteres
            .Matches("^[a-zA-Z0-9!@#$%^&*(),.?\":{}|<>\\[\\]\\\\/'`~;_+=-]+$")
                .WithMessage("Password contains forbidden characters.");
    }

    public static IRuleBuilderOptions<T, string?> PasswordOptionalRule<T>
    (this IRuleBuilder<T, string?> rule) where T : IPassword
    {
        return rule
            .Length(6, 20).WithMessage("Password must have [6-20] characters.")
            // Al menos una minúscula
            .Matches("[a-z]").WithMessage("Password must have at least one LOWER-case letter.")
            // Al menos una mayúscula
            .Matches("[A-Z]").WithMessage("Password must have at least one UPPER-case letter.")
            // Al menos un número
            .Matches("[0-9]").WithMessage("Password must have at least one number (0-9).")
            // Al menos un caracter especial
            .Matches("[!@#$%^&*(),.?\":{}|<>\\[\\]\\\\/'`~;_+=-]")
                .WithMessage("Password must have at least one special character.")
            // No permitir otros caracteres
            .Matches("^[a-zA-Z0-9!@#$%^&*(),.?\":{}|<>\\[\\]\\\\/'`~;_+=-]+$")
                .When(x => !string.IsNullOrEmpty(x.Password))
                .WithMessage("Password contains forbidden characters.");
    }

    public static IRuleBuilderOptions<T, string> RepeatPasswordRequiredRule<T>
    (this IRuleBuilder<T, string> rule) where T : IPassword
    {
        return rule
            .Equal(x => x.Password)
                .When(x => !string.IsNullOrWhiteSpace(x.Password) && !string.IsNullOrWhiteSpace(x.RepeatPassword),
                      ApplyConditionTo.CurrentValidator)
                .WithMessage("Passwords must match.")
            .NotEmpty()
                .WithMessage("Password is required.");
    }

    public static IRuleBuilderOptions<T, string?> RepeatPasswordOptionalRule<T>
    (this IRuleBuilder<T, string?> rule) where T : IPassword
    {
        return rule
            .Equal(x => x.Password)
                .WithMessage("Passwords must match.");
    }

    public static IRuleBuilderOptions<T, string> EmailRegisterRule<T>
    (this IRuleBuilder<T, string> rule, ICheckAvailables checker) where T : IRequireds
    {
        return rule
            .NotEmpty()
                .WithMessage("Email is required.")
            .MaximumLength(30)
            .Matches("^[a-zA-Z0-9@._-]+$")
                .WithMessage("Email has forbidden characters.")
            .Matches(@"^[^\s@]{3,}@[a-zA-Z0-9_-]{2,}\.[a-zA-Z0-9_-]+$")
                .WithMessage("Invalid email format (correct example: user@domain.com).")
            .Must(email => email.Count(c => c == '@') == 1)
                .WithMessage("Email must contain exactly one '@' character.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email), ApplyConditionTo.CurrentValidator)
            .MustAsync(async (email, ct) => await checker.EmailAvailable(email))
                .WithMessage("Email is already in use. Pick another one.")
                .WithErrorCode("EMAILNOTAVAILABLE")
                .When(x => !string.IsNullOrWhiteSpace(x.Email), ApplyConditionTo.CurrentValidator);
    }

    public static IRuleBuilderOptions<T, string> EmailUpdateRule<T>
    (this IRuleBuilder<T, string> rule, ICheckAvailables checker, string? originalEmail) where T : IRequireds
    {
        return rule
            .NotEmpty()
                .WithMessage("Email is required.")
            .MaximumLength(30)
            .Matches("^[a-zA-Z0-9@._-]+$")
                .WithMessage("Email has forbidden characters.")
            .Matches(@"^[^\s@]{3,}@[a-zA-Z0-9_-]{2,}\.[a-zA-Z0-9_-]+$")
                .WithMessage("Invalid email format (correct example: user@domain.com).")
            .Must(email => email.Count(c => c == '@') == 1)
                .WithMessage("Email must contain exactly one '@' character.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email), ApplyConditionTo.CurrentValidator)
            .MustAsync(async (email, ct) => await checker.EmailAvailable(email))
                .WithMessage("Email is already in use. Pick another one.")
                .WithErrorCode("EMAILNOTAVAILABLE")
                .When(x => x.Email != originalEmail, ApplyConditionTo.CurrentValidator)
                .When(x => !string.IsNullOrWhiteSpace(x.Email), ApplyConditionTo.CurrentValidator);
    }

    public static IRuleBuilderOptions<T, string?> FullNameRule<T, U>
    (this IRuleBuilder<T, string?> rule) where T : IOptionals<U>
    {
        return rule
            .MaximumLength(40)
            .Matches(@"^[a-zA-Z ]*$")
                .When(x => !string.IsNullOrEmpty(x.FullName))
                .WithMessage("Full name must only contain letters and whitespaces.");
    }

    public static IRuleBuilderOptions<T, DateTime?> BirthdayRule<T, U>
    (this IRuleBuilder<T, DateTime?> rule) where T : IOptionals<U>
    {
        return rule
            .Must(BeAtLeast12YearsOld)
                .When(x => x.Birthday.HasValue)
                .WithMessage("User must be at least 12 years old.");
    }

    public static IRuleBuilderOptions<T, IsoCountry?> CountryRule<T, U>
    (this IRuleBuilder<T, IsoCountry?> rule, ICountriesIso countries) where T : IOptionals<U>
    {
        return rule
            .MustAsync(async (c, ct) => await countries.GetAsync(c!.Country_code) is not null)
                .When(x => x.Country is not null)
                .WithMessage("Country code is invalid.");
    }


    // helpers
    public interface ICheckAvailables
    { 
        Task<bool> UsernameAvailable(string? username);
        Task<bool> EmailAvailable(string? email);
    }

    public static bool BeAtLeast12YearsOld(DateTime? birthday)
    {
        if (!birthday.HasValue) return true;

        var today = DateTime.Today;
        var age = today.Year - birthday.Value.Year;
        if (birthday.Value.Date > today.AddYears(-age)) age--;

        return age >= 12;
    }
}
