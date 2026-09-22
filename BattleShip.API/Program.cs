using BattleShip.API.Grpc;
using BattleShip.API.Services;
using BattleShip.API.Validation;
using BattleShip.Models.AI;
using BattleShip.Models.Domain;
using BattleShip.Models.DTOs;
using BattleShip.Models.Privacy;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddGrpc();
builder.Services.AddSingleton<GameStore>();
builder.Services.AddSingleton<AiOpponentService>();
builder.Services.AddSingleton<IValidator<ShotRequest>, ShotRequestValidator>();
builder.Services.AddSingleton<IValidator<CreateGameRequest>, CreateGameRequestValidator>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7091", "http://localhost:5282")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding");
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.MapGrpcService<GameGrpcService>().EnableGrpcWeb();

app.MapPost("/games", async (
    CreateGameRequest? request,
    GameStore store,
    IValidator<CreateGameRequest> validator) =>
{
    if (request?.Ships is not null)
    {
        var validation = await validator.ValidateAsync(request);
        if (!validation.IsValid)
            return Results.ValidationProblem(validation.ToDictionary());
    }

    try
    {
        var (id, _) = store.Create(request?.Ships);
        return Results.Created($"/games/{id}", new { GameId = id });
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { Error = exception.Message });
    }
});

app.MapGet("/games/{gameId:guid}", (Guid gameId, GameStore store) =>
    store.TryGet(gameId, out var engine) && engine is not null
        ? Results.Ok(GamePrivacyMapper.ToStatusDto(gameId, engine))
        : Results.NotFound());

app.MapPost("/games/{gameId:guid}/shots", async (
    Guid gameId,
    ShotRequest request,
    GameStore store,
    IValidator<ShotRequest> validator) =>
{
    var validation = await validator.ValidateAsync(request);
    if (!validation.IsValid)
        return Results.ValidationProblem(validation.ToDictionary());

    if (!store.TryGet(gameId, out var engine) || engine is null)
        return Results.NotFound();

    try
    {
        var result = engine.TakeShot(new Coordinate(request.Row, request.Col));
        return Results.Ok(new { Result = result });
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new { Error = exception.Message });
    }
    catch (ArgumentOutOfRangeException exception)
    {
        return Results.BadRequest(new { Error = exception.Message });
    }
});

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();

public partial class Program;
