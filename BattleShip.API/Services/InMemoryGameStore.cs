using System.Collections.Concurrent;
using BattleShip.Models.Domain;

namespace BattleShip.API.Services;

public class InMemoryGameStore
{
    private readonly ConcurrentDictionary<string, GameEngine> _games = new();

    public void SaveGame(string gameId, GameEngine game)
    {
        _games[gameId] = game;
    }

    public GameEngine? GetGame(string gameId)
    {
        _games.TryGetValue(gameId, out var game);
        return game;
    }
}
