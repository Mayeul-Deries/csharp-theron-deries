using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BattleShip.App;
using BattleShip.App.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5282/")
});
builder.Services.AddScoped<GameApiClient>();
builder.Services.AddScoped<IGameApiClient>(sp => sp.GetRequiredService<GameApiClient>());
builder.Services.AddScoped<OpponentGrpcClient>();
builder.Services.AddScoped<IOpponentGrpcClient>(sp =>
    sp.GetRequiredService<OpponentGrpcClient>());

await builder.Build().RunAsync();
