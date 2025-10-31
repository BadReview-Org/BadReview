using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BadReview.Client;
using System.Net.Http.Json;
using MudBlazor.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Crear un HttpClient temporal para leer el appsettings.json
using var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

var apiSettings = await httpClient.GetFromJsonAsync<AppSettings>("appsettings.json")
    ?? throw new Exception("No se pudo cargar appsettings.json");

string apiUri = apiSettings.Api?.URI 
    ?? throw new Exception("No se encontró Api:URI en appsettings.json");

// Configurar el HttpClient para la aplicación con la URI de la API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiUri) });
builder.Services.AddMudServices();

await builder.Build().RunAsync();

public record AppSettings(ApiConfig? Api);
public record ApiConfig(string? URI);