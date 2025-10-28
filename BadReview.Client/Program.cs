using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BadReview.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

using var response = await new HttpClient().GetAsync("appsettings.json");
using var stream = await response.Content.ReadAsStreamAsync();
builder.Configuration.AddJsonStream(stream);

string api = builder.Configuration["Api:URI"] ?? throw new Exception("Can't determine APIs address");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(api) });

await builder.Build().RunAsync();
