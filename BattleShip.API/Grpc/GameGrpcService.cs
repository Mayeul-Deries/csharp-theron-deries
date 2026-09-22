using BattleShip.API.Protos;
using BattleShip.API.Services;
using BattleShip.Models.AI;
using BattleShip.Models.Domain;
using Grpc.Core;

namespace BattleShip.API.Grpc;

public sealed class GameGrpcService(GameStore gameStore, AiOpponentService aiOpponentService)
    : GameService.GameServiceBase
{
    public override Task<OpponentShotResponse> TakeOpponentShot(
        OpponentShotRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.GameId, out var gameId)
            || !gameStore.TryGet(gameId, out var engine)
            || engine is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Game not found."));
        }

        try
        {
            var coordinate = aiOpponentService.GenerateShot(engine.PlayerGrid);
            var result = engine.TakeOpponentShot(coordinate);
            return Task.FromResult(new OpponentShotResponse
            {
                Result = result.ToString(),
                Row = coordinate.Row,
                Col = coordinate.Col
            });
        }
        catch (InvalidOperationException exception)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, exception.Message));
        }
    }
}