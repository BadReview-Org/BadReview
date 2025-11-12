namespace BadReview.Api.Configuration;

public static class AuthorizationConfig
{
    public static WebApplicationBuilder AddAuthorizationConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization(opts =>
        {
            opts.AddPolicy("AccessTokenPolicy", policy =>
            {
                policy.AddAuthenticationSchemes("AccessToken").RequireAuthenticatedUser();
            });

            opts.AddPolicy("RefreshTokenPolicy", policy =>
            {
                policy.AddAuthenticationSchemes("RefreshToken").RequireAuthenticatedUser();
            });
        });

        return builder;
    }
}