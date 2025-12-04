using BadReview.Api.Endpoints;
using BadReview.Api.Configuration;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "BadReview API",
        Description = "An API for managing game reviews and user profiles."
    });
});

builder
    .AddCorsConfig()
    .AddAuthenticationConfig()
    .AddAuthorizationConfig()
    .AddServiceConfig()
    .AddValidatorConfig();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app
    .UseCors("AllowBlazorClient")
    .UseAuthentication()
    .UseAuthorization();

app
    .MapUserEndpoints()
    .MapGameEndpoints()
    .MapReviewEndpoints()
    .MapGenreEndpoints()
    .MapDeveloperEndpoints()
    .MapPlatformEndpoints();

app.Run();