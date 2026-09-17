using BattleShip.Models.DTOs;
using BattleShip.Models.Enums;

namespace BattleShip.App.Services;

/// <summary>
/// Defines the REST operations used by the game screen.
/// </summary>
public interface IGameApiClient
{
    Task<Guid> CreateGameAsync(
        CreateGameRequest? request = null,
        CancellationToken cancellationToken = default);

    Task<GameStatusDto> GetGameAsync(
        Guid gameId,
        CancellationToken cancellationToken = default);

    Task<ShotResult> TakeShotAsync(
        Guid gameId,
        int row,
        int col,
        CancellationToken cancellationToken = default);
}
