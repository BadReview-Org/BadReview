namespace BadReview.Api.Configuration;
// CORS Configuration file
// This file sets up Cross-Origin Resource Sharing (CORS) policies for the API.
public static class CorsConfig
{
    public static WebApplicationBuilder AddCorsConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazorClient", policy =>
            {
                // Adjust the origins as necessary for your client application
                policy.WithOrigins("https://localhost:5193", "http://localhost:5193")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return builder;
    }
}

