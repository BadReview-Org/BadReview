using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using BadReview.Client;
using System.Net.Http.Json;
using MudBlazor.Services;
using BadReview.Client.Services;
using BadReview.Client.Utils;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Servicios requeridos por MudBlazor
builder.Services.AddMudServices();

// Crear un HttpClient temporal para leer el appsettings.json
using var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

var apiSettings = await httpClient.GetFromJsonAsync<AppSettings>("appsettings.json")
    ?? throw new Exception("No se pudo cargar appsettings.json");

string apiUri = apiSettings.Api?.URI 
    ?? throw new Exception("No se encontró Api:URI en appsettings.json");

// Configurar el HttpClient para la aplicación con la URI de la API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUri) });

// Scoped services
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthenticationStateProvider, JWTAuthStateProvider>();
builder.Services.AddScoped<IsoCountryMap>();
builder.Services.AddScoped<IconsMap>();

// Fluent Validation
builder.Services.AddScoped<LoginFormValidator>();
builder.Services.AddScoped<RegisterFirstStepValidator>();
builder.Services.AddScoped<RegisterSecondStepValidator>();
builder.Services.AddScoped<RegisterFormValidator>();

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();

public record AppSettings(ApiConfig? Api);
public record ApiConfig(string? URI);