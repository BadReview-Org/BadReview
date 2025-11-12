using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using FluentValidation;

using BadReview.Api.Data;
using BadReview.Api.Endpoints;
using BadReview.Api.Services;
using BadReview.Shared.DTOs.Request;


var builder = WebApplication.CreateBuilder(args);
var key = builder.Configuration["Jwt:Key"] ?? "";


// Services

builder.Services.AddOpenApi(); // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:5193", "http://localhost:5193")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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

// Validators
builder.Services.AddScoped<IValidator<LoginUserRequest>, LoginUserRequestValidator>();
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<IValidator<CreateReviewRequest>, CreateReviewRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowBlazorClient");

app.UseAuthentication();
app.UseAuthorization();
//app.UseHttpsRedirection();

// Map endpoints
app.MapUserEndpoints();
app.MapGameEndpoints();
app.MapReviewEndpoints();
app.MapGenreEndpoints();
app.MapDeveloperEndpoints();
app.MapPlatformEndpoints();

app.Run();
