using Microsoft.EntityFrameworkCore;

using BadReview.Api.Data;
using BadReview.Api.Services;

namespace BadReview.Api.Configuration;

public static class ServiceConfig
{
    public static WebApplicationBuilder AddServiceConfig(this WebApplicationBuilder builder)
    {
        // DbContext
        builder.Services.AddDbContext<BadReviewContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        // IGDB Client
        builder.Services.AddHttpClient<IIGDBService, IGDBClient>(client =>
            client.BaseAddress = new Uri("https://api.igdb.com/v4/"));
        // AuthService
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Specific endpoints services
        builder.Services.AddScoped<IGameService, GameService>();
        builder.Services.AddScoped<IGenreService, GenreService>();
        builder.Services.AddScoped<IReviewService, ReviewService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IDeveloperService, DeveloperService>();
        builder.Services.AddScoped<IPlatformService, PlatformService>();

        return builder;
    }
}