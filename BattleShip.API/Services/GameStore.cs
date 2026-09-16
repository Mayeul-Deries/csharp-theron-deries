using BattleShip.Models.Domain;

namespace BattleShip.API.Services;

public sealed class GameStore
{
    private readonly Dictionary<Guid, GameEngine> games = new();
    private readonly object syncRoot = new();

    public (Guid Id, GameEngine Engine) Create()
    {
        var engine = new GameEngine();
        engine.SetupPlayerGridWithRandomShips();
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