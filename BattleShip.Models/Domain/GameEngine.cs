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
        if (State is GameState.PlayerWon or GameState.OpponentWon)
            throw new InvalidOperationException("La partie est déjà terminée.");

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

        // Vérifier si tous les navires adverses sont coulés (victoire du joueur)
        if (CheckVictory(OpponentGrid))
        {
            State = GameState.PlayerWon;
        }
        else
        {
            // Règle : un tir touché laisse le tour au joueur actuel
            State = GameState.PlayerTurn;
        }

        return isSunk ? ShotResult.Sunk : ShotResult.Hit;
    }

    public ShotResult TakeOpponentShot(Coordinate target)
    {
        if (State is GameState.PlayerWon or GameState.OpponentWon)
            throw new InvalidOperationException("La partie est déjà terminée.");

        if (State != GameState.OpponentTurn)
            throw new InvalidOperationException("Ce n'est pas le tour de l'adversaire.");

        if (!target.IsValid())
            throw new ArgumentOutOfRangeException(nameof(target), "Coordonnées hors limites.");

        // Empêcher de rejouer sur une case déjà visée
        if (!PlayerGrid.ShotsReceived.Add(target))
            throw new InvalidOperationException("Cette case a déjà été visée.");

        // Vérifier si un navire est touché
        var hitShip = PlayerGrid.Ships.FirstOrDefault(s => s.OccupiedCoordinates.Contains(target));

        if (hitShip == null)
        {
            // Tir manqué : passe le tour au joueur
            State = GameState.PlayerTurn;
            return ShotResult.Miss;
        }

        // Tir touché : vérifier si le navire est coulé
        bool isSunk = hitShip.OccupiedCoordinates.All(c => PlayerGrid.ShotsReceived.Contains(c));

        // Vérifier si tous les navires du joueur sont coulés (victoire de l'IA)
        if (CheckVictory(PlayerGrid))
        {
            State = GameState.OpponentWon;
        }
        else
        {
            // Règle : un tir touché laisse le tour à l'adversaire actuel
            State = GameState.OpponentTurn;
        }

        return isSunk ? ShotResult.Sunk : ShotResult.Hit;
    }

    private static bool CheckVictory(Grid grid)
    {
        return grid.Ships.Count > 0 && grid.Ships.All(s => s.OccupiedCoordinates.All(c => grid.ShotsReceived.Contains(c)));
    }
}