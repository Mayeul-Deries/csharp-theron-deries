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
    public void SetupPlayerGridWithRandomShips() => PlaceRandomShips(PlayerGrid);
    public void SetupOpponentGridWithRandomShips() => PlaceRandomShips(OpponentGrid);

    public bool TrySetupPlayerShips(IEnumerable<Ship> ships)
    {
        var selectedShips = ships.ToList();
        if (selectedShips.Count != Enum.GetValues<ShipType>().Length)
            return false;

        var expectedTypes = Enum.GetValues<ShipType>().ToHashSet();
        if (selectedShips.Select(ship => ship.Type).ToHashSet().Count != expectedTypes.Count
            || !selectedShips.All(ship => expectedTypes.Contains(ship.Type)))
            return false;

        var grid = new Grid();
        if (!selectedShips.All(grid.TryAddShip))
            return false;

        PlayerGrid.Ships.Clear();
        PlayerGrid.Ships.AddRange(grid.Ships);
        return true;
    }

    private static void PlaceDefaultShips(Grid grid)
    {
        grid.TryAddShip(new Ship(ShipType.Carrier, new Coordinate(0, 0), Direction.Horizontal));
        grid.TryAddShip(new Ship(ShipType.Battleship, new Coordinate(2, 0), Direction.Horizontal));
        grid.TryAddShip(new Ship(ShipType.Destroyer, new Coordinate(4, 0), Direction.Horizontal));
        grid.TryAddShip(new Ship(ShipType.Submarine, new Coordinate(6, 0), Direction.Horizontal));
        grid.TryAddShip(new Ship(ShipType.TorpedoBoat, new Coordinate(8, 0), Direction.Horizontal));
    }

    private static void PlaceRandomShips(Grid grid)
    {
        foreach (var shipType in Enum.GetValues<ShipType>())
        {
            PlaceRandomShip(grid, shipType);
        }
    }

    private static void PlaceRandomShip(Grid grid, ShipType shipType)
    {
        for (var attempt = 0; attempt < 1_000; attempt++)
        {
            var coordinate = new Coordinate(Random.Shared.Next(10), Random.Shared.Next(10));
            var direction = Random.Shared.Next(2) == 0
                ? Direction.Horizontal
                : Direction.Vertical;
            var ship = new Ship(shipType, coordinate, direction);

            if (grid.TryAddShip(ship))
            {
                return;
            }
        }

        throw new InvalidOperationException("Impossible de placer aléatoirement la flotte.");
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
        bool opponentFleetSunk = OpponentGrid.Ships.All(ship => ship.OccupiedCoordinates.All(
            coordinate => OpponentGrid.ShotsReceived.Contains(coordinate)));

        State = opponentFleetSunk ? GameState.PlayerWon : GameState.PlayerTurn;

        return isSunk ? ShotResult.Sunk : ShotResult.Hit;
    }

    public ShotResult TakeOpponentShot(Coordinate target)
    {
        if (State != GameState.OpponentTurn)
            throw new InvalidOperationException("Ce n'est pas le tour de l'adversaire.");

        if (!target.IsValid())
            throw new ArgumentOutOfRangeException(nameof(target), "Coordonnées hors limites.");

        if (!PlayerGrid.ShotsReceived.Add(target))
            throw new InvalidOperationException("Cette case a déjà été visée.");

        var hitShip = PlayerGrid.Ships.FirstOrDefault(
            ship => ship.OccupiedCoordinates.Contains(target));

        if (hitShip is null)
        {
            State = GameState.PlayerTurn;
            return ShotResult.Miss;
        }

        bool isSunk = hitShip.OccupiedCoordinates.All(
            coordinate => PlayerGrid.ShotsReceived.Contains(coordinate));
        bool playerFleetSunk = PlayerGrid.Ships.All(ship => ship.OccupiedCoordinates.All(
            coordinate => PlayerGrid.ShotsReceived.Contains(coordinate)));
        State = playerFleetSunk ? GameState.OpponentWon : GameState.OpponentTurn;

        return isSunk ? ShotResult.Sunk : ShotResult.Hit;
    }
}