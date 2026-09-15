using BattleShip.Models.Enums;

namespace BattleShip.Models.Domain;

public class GameEngine
{
    public Grid PlayerGrid { get; } = new();
    public Grid OpponentGrid { get; } = new();
    public GameState State { get; private set; } = GameState.Setup;

    public void StartGame()
    {
        State = GameState.PlayerTurn;
    }

    public void SetupPlayerGridWithDefaultShips() => PlaceDefaultShips(PlayerGrid);
    public void SetupOpponentGridWithDefaultShips() => PlaceDefaultShips(OpponentGrid);

    private static void PlaceDefaultShips(Grid grid)
    {
        grid.TryAddShip(new Ship(ShipType.Carrier, new Coordinate(0, 0), Direction.Horizontal));
        grid.TryAddShip(new Ship(ShipType.Battleship, new Coordinate(2, 0), Direction.Horizontal));
        grid.TryAddShip(new Ship(ShipType.Destroyer, new Coordinate(4, 0), Direction.Horizontal));
        grid.TryAddShip(new Ship(ShipType.Submarine, new Coordinate(6, 0), Direction.Horizontal));
        grid.TryAddShip(new Ship(ShipType.TorpedoBoat, new Coordinate(8, 0), Direction.Horizontal));
    }

    public ShotResult TakeShot(Coordinate target)
    {
        if (State is not (GameState.PlayerTurn or GameState.Setup))
            throw new InvalidOperationException("Ce n'est pas le tour du joueur.");

        if (!target.IsValid())
            throw new ArgumentOutOfRangeException(nameof(target), "Coordonnées hors limites.");

        // Empêcher de rejouer sur une case déjà visée
        if (!OpponentGrid.ShotsReceived.Add(target))
            throw new InvalidOperationException("Cette case a déjà été visée.");

        // Vérifier si un navire est touché
        var hitShip = OpponentGrid.Ships.FirstOrDefault(s => s.OccupiedCoordinates.Contains(target));

        if (hitShip == null)
        {
            // Tir manqué : passe le tour à l'adversaire
            State = GameState.OpponentTurn;
            return ShotResult.Miss;
        }

        // Tir touché : vérifier si le navire est coulé
        bool isSunk = hitShip.OccupiedCoordinates.All(c => OpponentGrid.ShotsReceived.Contains(c));

        // Règle : un tir touché laisse le tour au joueur actuel
        State = GameState.PlayerTurn;

        return isSunk ? ShotResult.Sunk : ShotResult.Hit;
    }
}