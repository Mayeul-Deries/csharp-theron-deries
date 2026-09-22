using BattleShip.API.Services;
using BattleShip.Models.Domain;
using BattleShip.Models.DTOs;
using BattleShip.Models.Validators;
using BattleShip.Models.Privacy;
using BattleShip.Models.AI;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSingleton<InMemoryGameStore>();
builder.Services.AddSingleton<AiOpponentService>(); // Enregistrement de l'IA
builder.Services.AddGrpc();

// Ajout des politiques CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:7091", "http://localhost:5282")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazor");
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true }); // Activation gRPC-Web

app.MapPost("/games", (InMemoryGameStore store) =>
{
    var gameId = Guid.NewGuid().ToString();
    var game = new GameEngine();
    game.SetupPlayerGridWithDefaultShips();
    game.SetupOpponentGridWithDefaultShips();
    game.StartGame();
    store.SaveGame(gameId, game);
    return Results.Created("/games/" + gameId, new { gameId });
});

app.MapGet("/games/{gameId}", (string gameId, InMemoryGameStore store) =>
{
    var game = store.GetGame(gameId);
    if (game == null) return Results.NotFound();

    var dto = GamePrivacyMapper.ToStatusDto(Guid.Parse(gameId), game);
    return Results.Ok(dto);
});

app.MapPost("/games/{gameId}/shots", (string gameId, ShotRequest request, InMemoryGameStore store) =>
{
    var game = store.GetGame(gameId);
    if (game == null) return Results.NotFound();
    
    var validator = new ShotRequestValidator();
    var validationResult = validator.Validate(request);
    if (!validationResult.IsValid) return Results.BadRequest(validationResult.Errors);
    
    try
    {
        var result = game.TakeShot(new Coordinate(request.Row, request.Col));
        return Results.Ok(new { result = result.ToString() });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGrpcService<BattleShip.API.Services.BattleShipGrpcService>().EnableGrpcWeb().RequireCors("AllowBlazor");
app.Run();