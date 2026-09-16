using Bunit;
using BattleShip.App.Pages;
using BattleShip.App.Services;
using BattleShip.Models.DTOs;
using BattleShip.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace BattleShip.Tests.Frontend;

public sealed class HomeTests : BunitContext
{
    [Fact]
    public async Task StartGame_ShouldRenderBothBoards()
    {
        var gameId = Guid.NewGuid();
        Services.AddSingleton<IGameApiClient>(
            new FakeGameApiClient(gameId, CreateStatus(gameId, GameState.PlayerTurn)));
        Services.AddSingleton<IOpponentGrpcClient>(new FakeOpponentGrpcClient());

        var page = Render<Home>();

        await page.Find("button.btn-primary").ClickAsync();

        Assert.Equal(200, page.FindAll(".battle-cell").Count);
        Assert.Contains("À vous de jouer", page.Markup);
    }

    [Fact]
    public async Task Miss_ShouldCallOpponentGrpc_AndRefreshFinalStatus()
    {
        var gameId = Guid.NewGuid();
        var api = new FakeGameApiClient(
            gameId,
            CreateStatus(gameId, GameState.PlayerTurn),
            CreateStatus(gameId, GameState.OpponentTurn),
            CreateStatus(gameId, GameState.PlayerTurn));
        var grpc = new FakeOpponentGrpcClient();
        Services.AddSingleton<IGameApiClient>(api);
        Services.AddSingleton<IOpponentGrpcClient>(grpc);

        var page = Render<Home>();
        await page.Find("button.btn-primary").ClickAsync();
        page.WaitForAssertion(() => Assert.Equal(200, page.FindAll(".battle-cell").Count));
        await page.FindAll(".battle-cell")[100].ClickAsync();

        page.WaitForAssertion(() =>
        {
            Assert.Equal(gameId, grpc.GameId);
            Assert.Contains("Tir de l'IA : Miss", page.Markup);
            Assert.Contains("À vous de jouer", page.Markup);
            Assert.Equal(3, api.GetGameCallCount);
        });
    }

    [Fact]
    public async Task OpponentHit_ShouldCallGrpcAgain_UntilPlayerTurn()
    {
        var gameId = Guid.NewGuid();
        var api = new FakeGameApiClient(
            gameId,
            CreateStatus(gameId, GameState.PlayerTurn),
            CreateStatus(gameId, GameState.OpponentTurn),
            CreateStatus(gameId, GameState.OpponentTurn),
            CreateStatus(gameId, GameState.PlayerTurn));
        var grpc = new FakeOpponentGrpcClient(ShotResult.Hit);
        Services.AddSingleton<IGameApiClient>(api);
        Services.AddSingleton<IOpponentGrpcClient>(grpc);

        var page = Render<Home>();
        await page.Find("button.btn-primary").ClickAsync();
        await page.FindAll(".battle-cell")[100].ClickAsync();

        page.WaitForAssertion(() =>
        {
            Assert.Equal(2, grpc.CallCount);
            Assert.Contains("Tir de l'IA : Hit", page.Markup);
            Assert.Contains("À vous de jouer", page.Markup);
        });
    }

    private static GameStatusDto CreateStatus(Guid gameId, GameState state) =>
        new(gameId, state, CreateCells(CellState.Ship), CreateCells(CellState.Empty));

    private static List<CellDto> CreateCells(CellState state) =>
        Enumerable.Range(0, 100)
            .Select(index => new CellDto(index / 10, index % 10, state))
            .ToList();

    private sealed class FakeGameApiClient(
        Guid gameId,
        params GameStatusDto[] statuses) : IGameApiClient
    {
        private int statusIndex;

        public int GetGameCallCount { get; private set; }

        public Task<Guid> CreateGameAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(gameId);

        public Task<GameStatusDto> GetGameAsync(
            Guid requestedGameId,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal(gameId, requestedGameId);
            GetGameCallCount++;
            var status = statuses[Math.Min(statusIndex++, statuses.Length - 1)];
            return Task.FromResult(status);
        }

        public Task<ShotResult> TakeShotAsync(
            Guid requestedGameId,
            int row,
            int col,
            CancellationToken cancellationToken = default)
        {
            Assert.Equal(gameId, requestedGameId);
            Assert.Equal(0, row);
            Assert.Equal(0, col);
            return Task.FromResult(ShotResult.Miss);
        }
    }

    private sealed class FakeOpponentGrpcClient : IOpponentGrpcClient
    {
        private readonly ShotResult result;

        public FakeOpponentGrpcClient(ShotResult result = ShotResult.Miss)
        {
            this.result = result;
        }

        public Guid? GameId { get; private set; }
        public int CallCount { get; private set; }

        public Task<OpponentShotResult> TakeOpponentShotAsync(
            Guid gameId,
            CancellationToken cancellationToken = default)
        {
            GameId = gameId;
            CallCount++;
            return Task.FromResult(new OpponentShotResult(result, 4, 5));
        }
    }
}
