using BattleShip.Models.Domain;
using BattleShip.Models.DTOs;

namespace BattleShip.API.Services;

public sealed class GameStore
{
    private readonly Dictionary<Guid, GameEngine> games = new();
    private readonly object syncRoot = new();

    public (Guid Id, GameEngine Engine) Create(IReadOnlyList<ShipPlacementDto>? placements = null)
    {
        var engine = new GameEngine();
        var playerShips = placements?.Select(placement => new Ship(
            placement.ShipType,
            new Coordinate(placement.Row, placement.Col),
            placement.Direction));

        if (placements is null)
            engine.SetupPlayerGridWithRandomShips();
        else if (playerShips is null || !engine.TrySetupPlayerShips(playerShips))
            throw new ArgumentException("Le placement de la flotte est invalide.", nameof(placements));

        engine.SetupOpponentGridWithRandomShips();
        engine.StartGame();

        var id = Guid.NewGuid();
        lock (syncRoot)
            games.Add(id, engine);

        return (id, engine);
    }

    public bool TryGet(Guid id, out GameEngine? engine)
    {
        lock (syncRoot)
            return games.TryGetValue(id, out engine);
    }
}