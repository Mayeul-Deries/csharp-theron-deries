using Grpc.Core;
using BattleShip.API.Protos;
using BattleShip.API.Services;
using BattleShip.Models.Domain;
using BattleShip.Models.AI;

namespace BattleShip.API.Services;

public class BattleShipGrpcService : GameService.GameServiceBase
{
    private readonly InMemoryGameStore _store;
    private readonly AiOpponentService _aiService;

    public BattleShipGrpcService(InMemoryGameStore store, AiOpponentService aiService)
    {
        _store = store;
        _aiService = aiService;
    }

    public override Task<GameResponse> CreateGame(CreateGameRequest request, ServerCallContext context)
    {
        var gameId = Guid.NewGuid().ToString();
        var game = new GameEngine();
        game.SetupPlayerGridWithDefaultShips();
        game.SetupOpponentGridWithDefaultShips();
        game.StartGame();
        _store.SaveGame(gameId, game);
        return Task.FromResult(new GameResponse { GameId = gameId });
    }

    public override Task<ShotResponse> TakeShot(Protos.ShotRequest request, ServerCallContext context)
    {
        var game = _store.GetGame(request.GameId);
        if (game == null) throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));

        try
        {
            var result = game.TakeShot(new Coordinate(request.Row, request.Col));
            return Task.FromResult(new ShotResponse { Result = result.ToString() });
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }

    public override Task<OpponentShotResponse> TakeOpponentShot(OpponentShotRequest request, ServerCallContext context)
    {
        var game = _store.GetGame(request.GameId);
        if (game == null) throw new RpcException(new Status(StatusCode.NotFound, "Game not found"));

        try
        {
            var target = _aiService.GenerateShot(game.PlayerGrid);
            var result = game.TakeOpponentShot(target);
            return Task.FromResult(new OpponentShotResponse 
            { 
                Result = result.ToString(),
                Row = target.Row,
                Col = target.Col
            });
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
    }
}