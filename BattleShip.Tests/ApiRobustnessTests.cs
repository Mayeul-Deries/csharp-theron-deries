using System.Net;
using System.Net.Http.Json;
using BattleShip.Models.DTOs;
using BattleShip.Models.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BattleShip.Tests;

public class ApiRobustnessTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiRobustnessTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TakeShot_ShouldReturn404_WhenGameNotFound()
    {
        var response = await _client.PostAsJsonAsync("/games/invalid-id/shots", new { row = 0, col = 0 });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TakeShot_ShouldReturn400_WhenCoordinatesInvalid()
    {
        // 1. Créer une partie
        var createResponse = await _client.PostAsync("/games", null);
        var gameData = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var gameId = gameData!["gameId"];

        // 2. Tirer hors limites
        var response = await _client.PostAsJsonAsync($"/games/{gameId}/shots", new { row = 10, col = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
