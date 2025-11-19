using FluentValidation;

using BadReview.Shared.Utils;

namespace BadReview.Shared.DTOs.Request;

public record UserCheckAvailable(string? Username, string? Email);


// interfaces
public interface IUsername { string Username { get; } }

public interface IPassword { string? Password { get; } string? RepeatPassword { get; } }

public interface ILogin : IUsername, IPassword;

public interface IRequireds : IUsername, IPassword { string Email { get; } }

public interface IOptionals<T> { string? FullName { get; } DateTime? Birthday { get; } T? Country { get; } }


// implementations
public record LoginUserRequest(string Username, string Password) : ILogin
{
    public string? RepeatPassword => null;
}

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string? FullName,
    DateTime? Birthday,
    int? Country
) : IRequireds, IOptionals<int?>
{
    public string? RepeatPassword => null;
}

public record UpdateUserRequest(
    string Username,
    string Email,
    string? Password,
    string? FullName,
    DateTime? Birthday,
    int? Country
) : IRequireds, IOptionals<int?>
{
    public string? RepeatPassword => null;
}

public class LoginForm : ILogin
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    public string? RepeatPassword => null;
}

public class UserForm : IRequireds, IOptionals<IsoCountry?>
{
    public UserFirstStep First { get; set; } = null!;
    public UserSecondStep Second { get; set; } = null!;

    public string Email => First.Email;
    public string Username => First.Username;
    public string Password => First.Password;
    public string? RepeatPassword => First.RepeatPassword;
    public string? FullName => Second.FullName;
    public DateTime? Birthday => Second.Birthday;
    public IsoCountry? Country => Second.Country;

    public UserForm() {}

    // Copy constructor (deep copy)
    public UserForm(UserForm obj)
    {
        First = new UserFirstStep(obj.First);
        Second = new UserSecondStep(obj.Second);
    }
}

public class UserFirstStep : IRequireds
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string RepeatPassword { get; set; } = null!;
    public string Email { get; set; } = null!;

    public UserFirstStep() {}

    // Copy constructor
    public UserFirstStep(UserFirstStep obj)
    {
        Username = obj.Username;
        Password = obj.Password;
        RepeatPassword = obj.RepeatPassword;
        Email = obj.Email;
    }
}

public class UserSecondStep : IOptionals<IsoCountry?>
{
    public string? FullName { get; set; } = null;
    public DateTime? Birthday { get; set; } = null;
    public IsoCountry? Country { get; set; } = null;

    public UserSecondStep() {}

    // Copy constructor
    public UserSecondStep(UserSecondStep obj)
    {
        FullName = obj.FullName;
        Birthday = obj.Birthday;
        Country = obj.Country is null ?
            null : new IsoCountry(obj.Country.Name, obj.Country.Alpha_3, obj.Country.Country_code);
    }
}


// Fluent Validation

// backend
public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
{
    public LoginUserRequestValidator()
    {
        // Username
        RuleFor(x => x.Username).UsernameLoginRule();

        // Password
        RuleFor(x => x.Password).PasswordRequiredRule();
    }
}

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator(ValidatorRules.ICheckAvailables checker)
    {
        // Username
        RuleFor(x => x.Username).UsernameRegisterRule(checker);

        // Password
        RuleFor(x => x.Password).PasswordRequiredRule();

        // Email
        RuleFor(x => x.Email).EmailRegisterRule(checker);

        // FullName (solo letras y espacios, 40 caracteres máx)
        RuleFor(x => x.FullName).FullNameRule<CreateUserRequest, int?>();

        // Birthday (mínimo 12 años)
        RuleFor(x => x.Birthday).BirthdayRule<CreateUserRequest, int?>(); 
    }
}

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator
    (ValidatorRules.ICheckAvailables checker, string? originalUsername, string? originalEmail)
    {
        // Username
        RuleFor(x => x.Username).UsernameUpdateRule(checker, originalUsername);

        // Password
        RuleFor(x => x.Password).PasswordOptionalRule();

        // Email
        RuleFor(x => x.Email).EmailUpdateRule(checker, originalEmail);

        // FullName (solo letras y espacios, 40 caracteres máx)
        RuleFor(x => x.FullName).FullNameRule<UpdateUserRequest, int?>();

        // Birthday (mínimo 12 años)
        RuleFor(x => x.Birthday).BirthdayRule<UpdateUserRequest, int?>(); 
    }
}

// frontend
public class LoginFormValidator : AbstractValidator<LoginForm>
{
    public LoginFormValidator()
    {
        // Username
        RuleFor(x => x.Username).UsernameLoginRule();

        // Password
        RuleFor(x => x.Password).PasswordRequiredRule();
    }
}

public class UserFirstStepCreateValidator : AbstractValidator<UserFirstStep>
{
    public UserFirstStepCreateValidator(ValidatorRules.ICheckAvailables checker)
    {
        // Username
        RuleFor(x => x.Username).UsernameRegisterRule(checker);

        // Password
        RuleFor(x => x.Password).PasswordRequiredRule();

        // Repeat Password
        RuleFor(x => x.RepeatPassword).RepeatPasswordRequiredRule();

        // Email
        RuleFor(x => x.Email).EmailRegisterRule(checker);
    }
}

public class UserSecondStepCreateValidator : AbstractValidator<UserSecondStep>
{
    public UserSecondStepCreateValidator(ICountriesIso countries)
    {
        // FullName (solo letras y espacios, 40 caracteres máx)
        RuleFor(x => x.FullName).FullNameRule<UserSecondStep, IsoCountry?>();

        // Birthday (mínimo 12 años)
        RuleFor(x => x.Birthday).BirthdayRule<UserSecondStep, IsoCountry?>();

        // Country (debe coincidir con los paises del ISO 3166)
        RuleFor(x => x.Country).CountryRule<UserSecondStep, IsoCountry?>(countries);
    }
}

public class UserFormCreateValidator : AbstractValidator<UserForm>
{
    public UserFormCreateValidator(UserFirstStepCreateValidator validator1,
                                 UserSecondStepCreateValidator validator2)
    {
        RuleFor(x => x.First).SetValidator(validator1);
        RuleFor(x => x.Second).SetValidator(validator2);
    }
}

public class UserFirstStepUpdateValidator : AbstractValidator<UserFirstStep>
{
    public UserFirstStepUpdateValidator
    (ValidatorRules.ICheckAvailables checker, string? originalUsername, string? originalEmail)
    {
        // Username
        RuleFor(x => x.Username).UsernameUpdateRule(checker, originalUsername);

        // Password (optional)
        RuleFor(x => x.Password).PasswordOptionalRule();

        // Repeat Password (optional)
        RuleFor(x => x.RepeatPassword).RepeatPasswordOptionalRule();

        // Email
        RuleFor(x => x.Email).EmailUpdateRule(checker, originalEmail);
    }
}

public class UserSecondStepUpdateValidator : AbstractValidator<UserSecondStep>
{
    public UserSecondStepUpdateValidator(ICountriesIso countries)
    {
        // FullName (solo letras y espacios, 40 caracteres máx)
        RuleFor(x => x.FullName).FullNameRule<UserSecondStep, IsoCountry?>();

        // Birthday (mínimo 12 años)
        RuleFor(x => x.Birthday).BirthdayRule<UserSecondStep, IsoCountry?>();

        // Country (debe coincidir con los paises del ISO 3166)
        RuleFor(x => x.Country).CountryRule<UserSecondStep, IsoCountry?>(countries);
    }
}

public class UserFormUpdateValidator : AbstractValidator<UserForm>
{
    public UserFormUpdateValidator(UserFirstStepUpdateValidator validator1,
                                 UserSecondStepUpdateValidator validator2)
    {
        RuleFor(x => x.First).SetValidator(validator1);
        RuleFor(x => x.Second).SetValidator(validator2);
    }
}
