namespace BattleShip.App.Services;

/// <summary>
/// Defines the gRPC operation used to execute the opponent turn.
/// </summary>
public interface IOpponentGrpcClient
{
    Task<OpponentShotResult> TakeOpponentShotAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);
}
