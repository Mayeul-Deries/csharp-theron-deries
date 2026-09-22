using BattleShip.Models.Domain;
using BattleShip.Models.Enums;

namespace BattleShip.Models.AI;

public class AiOpponentService
{
    private readonly Random _random = new();

    public Coordinate GenerateShot(Grid playerGrid)
    {
        // 1. Recherche d'une cible prioritaire (case adjacente à un tir touché dont le navire n'est pas coulé)
        var targetFromHit = FindTargetFromPreviousHits(playerGrid);
        if (targetFromHit.HasValue)
        {
            return targetFromHit.Value;
        }

        // 2. Stratégie "Pari" (Checkerboard) pour optimiser la recherche aléatoire
        var availableCoordinates = GetAvailableCoordinates(playerGrid).Where(c => (c.Row + c.Col) % 2 == 0).ToList();
        if (availableCoordinates.Count == 0)
        {
            availableCoordinates = GetAvailableCoordinates(playerGrid);
        }

        if (availableCoordinates.Count == 0)
        {
            throw new InvalidOperationException("Aucune case disponible pour le tir de l'IA.");
        }

        int index = _random.Next(availableCoordinates.Count);
        return availableCoordinates[index];
    }

    private static Coordinate? FindTargetFromPreviousHits(Grid playerGrid)
    {
        // On cherche les navires touchés non coulés
        var targets = new List<Coordinate>();
        
        foreach (var ship in playerGrid.Ships)
        {
            bool isSunk = ship.OccupiedCoordinates.All(c => playerGrid.ShotsReceived.Contains(c));
            if (isSunk) continue;

            var hits = ship.OccupiedCoordinates.Where(c => playerGrid.ShotsReceived.Contains(c)).ToList();
            if (hits.Count > 0)
            {
                foreach (var hit in hits)
                {
                    foreach (var candidate in GetAdjacentCoordinates(hit))
                    {
                        if (candidate.IsValid() && !playerGrid.ShotsReceived.Contains(candidate))
                        {
                            targets.Add(candidate);
                        }
                    }
                }
            }
        }

        return targets.Any() ? targets.First() : null;
    }

    private static List<Coordinate> GetAdjacentCoordinates(Coordinate coord) => new()
    {
        new Coordinate(coord.Row - 1, coord.Col),
        new Coordinate(coord.Row + 1, coord.Col),
        new Coordinate(coord.Row, coord.Col - 1),
        new Coordinate(coord.Row, coord.Col + 1)
    };

    private static List<Coordinate> GetAvailableCoordinates(Grid playerGrid)
    {
        var available = new List<Coordinate>();
        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                var coord = new Coordinate(r, c);
                if (!playerGrid.ShotsReceived.Contains(coord))
                {
                    available.Add(coord);
                }
            }
        }
        return available;
    }
}
