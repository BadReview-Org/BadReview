using BadReview.Api.Endpoints;
using BadReview.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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