using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BadReview.Api.Configuration;

public static class AuthenticationConfig
{
    public static WebApplicationBuilder AddAuthenticationConfig(this WebApplicationBuilder builder)
    {
        var key = builder.Configuration["Jwt:Key"] ?? "";

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };

        builder.Services
            .AddAuthentication(opts => opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("AccessToken", opts =>
            {
                opts.TokenValidationParameters = validationParameters;

                opts.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claim = context.Principal?.FindFirst("token_type")?.Value;
                        if (claim != "access_token") context.Fail("Invalid token type, expected an Access token.");

                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer("RefreshToken", opts =>
            {
                opts.TokenValidationParameters = validationParameters;

                opts.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        var claim = context.Principal?.FindFirst("token_type")?.Value;
                        if (claim != "refresh_token") context.Fail("Invalid token type, expected a Refresh Token.");

                        return Task.CompletedTask;
                    }
                };
            });

        return builder;
    }
}