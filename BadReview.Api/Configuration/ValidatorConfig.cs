using FluentValidation;

using BadReview.Shared.DTOs.Request;
using BadReview.Api.Services;
using BadReview.Api.Services.Validations;

namespace BadReview.Api.Configuration;

public static class ValidatorConfig
{
    public static WebApplicationBuilder AddValidatorConfig(this WebApplicationBuilder builder)
    {
        // validation helpers
        builder.Services.AddScoped<ValidatorRules.ICheckAvailables, CheckAvailableCredentials>();
        // user validation
        builder.Services.AddScoped<IValidator<LoginUserRequest>, LoginUserRequestValidator>();
        builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        //builder.Services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();
        // review validation
        builder.Services.AddScoped<IValidator<CreateReviewRequest>, CreateReviewRequestValidator>();

        return builder;
    }
}