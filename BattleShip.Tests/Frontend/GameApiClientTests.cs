using System.Net;
using System.Text.Json;
using BattleShip.App.Services;
using BattleShip.Models.DTOs;
using BattleShip.Models.Enums;

namespace BattleShip.Tests.Frontend;

public sealed class GameApiClientTests
{
    [Fact]
    public async Task CreateGameAsync_ShouldPostEmptyJsonBody_AndReturnGameId()
    {
        var gameId = Guid.NewGuid();
        var handler = new RecordingHandler(
            HttpStatusCode.Created,
            $$"""{"gameId":"{{gameId}}"}""");
        using var client = CreateClient(handler);
        var gameApi = new GameApiClient(client);

        var result = await gameApi.CreateGameAsync();

        Assert.Equal(gameId, result);
        Assert.Equal(HttpMethod.Post, handler.Request?.Method);
        Assert.Equal("https://localhost:7091/games", handler.Request?.RequestUri?.ToString());

        var body = await handler.Request!.Content!.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.Empty(json.RootElement.EnumerateObject());
    }

    [Fact]
    public async Task CreateGameAsync_WithCustomShips_ShouldPostShips()
    {
        var gameId = Guid.NewGuid();
        var handler = new RecordingHandler(
            HttpStatusCode.Created,
            $$"""{"gameId":"{{gameId}}"}""");
        using var client = CreateClient(handler);
        var gameApi = new GameApiClient(client);

        var request = new CreateGameRequest(
        [
            new(ShipType.Carrier, 0, 0, Direction.Horizontal)
        ]);
        var result = await gameApi.CreateGameAsync(request);

        Assert.Equal(gameId, result);
        var body = await handler.Request!.Content!.ReadAsStringAsync();
        Assert.Contains("Carrier", body);
    }

    [Fact]
    public async Task GetGameAsync_ShouldRequestGameStatus_AndDeserializeResponse()
    {
        var gameId = Guid.NewGuid();
        var handler = new RecordingHandler(
            HttpStatusCode.OK,
            $$"""{"gameId":"{{gameId}}","state":"PlayerTurn","playerGrid":[],"opponentGrid":[]}""");
        using var client = CreateClient(handler);
        var gameApi = new GameApiClient(client);

        var result = await gameApi.GetGameAsync(gameId);

        Assert.Equal(gameId, result.GameId);
        Assert.Equal(GameState.PlayerTurn, result.State);
        Assert.Equal(HttpMethod.Get, handler.Request?.Method);
        Assert.Equal(
            $"https://localhost:7091/games/{gameId}",
            handler.Request?.RequestUri?.ToString());
    }

    [Fact]
    public async Task TakeShotAsync_ShouldPostCoordinates_AndReturnShotResult()
    {
        var gameId = Guid.NewGuid();
        var handler = new RecordingHandler(
            HttpStatusCode.OK,
            """{"result":"Miss"}""");
        using var client = CreateClient(handler);
        var gameApi = new GameApiClient(client);

        var result = await gameApi.TakeShotAsync(gameId, 2, 7);

        Assert.Equal(ShotResult.Miss, result);
        Assert.Equal(HttpMethod.Post, handler.Request?.Method);
        Assert.Equal(
            $"https://localhost:7091/games/{gameId}/shots",
            handler.Request?.RequestUri?.ToString());

        var body = await handler.Request!.Content!.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        Assert.Equal(2, json.RootElement.GetProperty("row").GetInt32());
        Assert.Equal(7, json.RootElement.GetProperty("col").GetInt32());
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task ApiErrors_ShouldExposeHttpStatusCode(HttpStatusCode statusCode)
    {
        var handler = new RecordingHandler(statusCode, "{}");
        using var client = CreateClient(handler);
        var gameApi = new GameApiClient(client);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => gameApi.GetGameAsync(Guid.NewGuid()));

        Assert.Equal(statusCode, exception.StatusCode);
    }

    private static HttpClient CreateClient(RecordingHandler handler) =>
        new(handler)
        {
            BaseAddress = new Uri("https://localhost:7091/")
        };

    private sealed class RecordingHandler(
        HttpStatusCode statusCode,
        string responseBody) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseBody)
            });
        }
    }
}
