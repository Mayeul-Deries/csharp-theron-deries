using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BattleShip.Models.DTOs;
using BattleShip.Models.Enums;

namespace BattleShip.App.Services;

/// <summary>
/// Provides the HTTP operations required by the game screen.
/// </summary>
public sealed class GameApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient httpClient;

    public GameApiClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<Guid> CreateGameAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "games",
            new { },
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<CreateGameResponse>(
            JsonOptions,
            cancellationToken);

        return payload?.GameId ?? throw new InvalidOperationException(
            "The create game response did not contain a gameId.");
    }

    public async Task<GameStatusDto> GetGameAsync(
        Guid gameId,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"games/{gameId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GameStatusDto>(
                   JsonOptions,
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "The game status response was empty.");
    }

    public async Task<ShotResult> TakeShotAsync(
        Guid gameId,
        int row,
        int col,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            $"games/{gameId}/shots",
            new ShotRequest(row, col),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ShotResponse>(
            JsonOptions,
            cancellationToken);

        return payload?.Result ?? throw new InvalidOperationException(
            "The shot response did not contain a result.");
    }

    private sealed record CreateGameResponse(Guid GameId);

    private sealed record ShotResponse(ShotResult Result);
}
