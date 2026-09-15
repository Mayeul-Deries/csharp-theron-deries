using BattleShip.Models.Enums;

namespace BattleShip.Models.Domain;

public class Grid
{
    public List<Ship> Ships { get; } = new();
    public HashSet<Coordinate> ShotsReceived { get; } = new();

    public bool TryAddShip(Ship ship)
    {
        // 1. Vérifier si toutes les coordonnées sont dans la grille 10x10
        if (ship.OccupiedCoordinates.Any(c => !c.IsValid()))
            return false;

        // 2. Vérifier s'il n'y a pas de chevauchement avec un navire déjà placé
        bool overlaps = Ships.Any(s => s.OccupiedCoordinates.Intersect(ship.OccupiedCoordinates).Any());
        if (overlaps)
            return false;

        Ships.Add(ship);
        return true;
    }
}