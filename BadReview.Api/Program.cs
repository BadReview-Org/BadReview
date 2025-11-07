using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using BadReview.Api.Data;
using BadReview.Api.Endpoints;
using BadReview.Api.Services;

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

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
        
        // JWT Debugger
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication Failed: {context.Exception.Message}");
                Console.WriteLine($"   Exception Type: {context.Exception.GetType().Name}");
                if (context.Exception.InnerException != null)
                    Console.WriteLine($"   Inner Exception: {context.Exception.InnerException.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("JWT Token Validated Successfully");
                var claims = context.Principal?.Claims.Select(c => $"{c.Type}: {c.Value}");
                Console.WriteLine($"   Claims: {string.Join(", ", claims ?? new[] { "No claims" })}");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var token = context.Token;
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine($"Authorization Header: {authHeader ?? "NULL"}");
                Console.WriteLine($"JWT Token Received: {(token?.Length > 20 ? token.Substring(0, 20) + "..." : "null")}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


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

app.Run();
