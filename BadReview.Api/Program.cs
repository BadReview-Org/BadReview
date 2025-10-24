using BadReview.Api.Data;
using BadReview.Api.Endpoints;
using BadReview.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add DbContext
builder.Services.AddDbContext<BadReviewContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add IGDB Client
builder.Services.AddHttpClient<IGDBClient>(client =>
    client.BaseAddress = new Uri("https://api.igdb.com/v4/"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapUserEndpoints();
app.MapGameEndpoints();
app.MapReviewEndpoints();

app.Run();
