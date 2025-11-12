using FluentValidation;

namespace BadReview.Shared.DTOs.Request;

// Common validators rules

public static class CommonValidators
{
    public static IRuleBuilderOptions<T, string> UsernameRule<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 20).WithMessage("Username must have [3-20] characters.")
            .Matches("^[a-zA-Z0-9._-]+$")
                .WithMessage("Username must only contain letters, numbers and characters (- _ .)");
    }

    public static IRuleBuilderOptions<T, string> PasswordRule<T>(this IRuleBuilder<T, string> rule)
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

    public static bool BeAtLeast12YearsOld(DateTime? birthday)
    {
        if (!birthday.HasValue) return true;

        var today = DateTime.Today;
        var age = today.Year - birthday.Value.Year;
        if (birthday.Value.Date > today.AddYears(-age)) age--;

        return age >= 12;
    }
}
