using FluentValidation;
using BadReview.Shared.DTOs.Request;

namespace BadReview.Api.Configuration;

public static class ValidatorConfig
{
    public static WebApplicationBuilder AddValidatorConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IValidator<LoginUserRequest>, LoginUserRequestValidator>();
        builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
        builder.Services.AddScoped<IValidator<CreateReviewRequest>, CreateReviewRequestValidator>();

        return builder;
    }
}