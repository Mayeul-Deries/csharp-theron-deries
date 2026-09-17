using BattleShip.Models.Domain;
using BattleShip.Models.DTOs;
using BattleShip.Models.Enums;

namespace BattleShip.Models.Privacy;

public static class GamePrivacyMapper
{
    public static GameStatusDto ToStatusDto(Guid gameId, GameEngine engine)
    {
        return new GameStatusDto(
            gameId,
            engine.State,
            BuildPlayerGridCells(engine.PlayerGrid),
            BuildOpponentGridCells(
                engine.OpponentGrid,
                engine.State == GameState.OpponentWon)
        );
    }

    // Grille Joueur : On affiche tout (Bateaux + Tirs reçus)
    private static List<CellDto> BuildPlayerGridCells(Grid grid)
    {
        var cells = new List<CellDto>();

        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                var coord = new Coordinate(r, c);
                var ship = grid.Ships.FirstOrDefault(s => s.OccupiedCoordinates.Contains(coord));
                bool hasShip = ship is not null;
                bool isShot = grid.ShotsReceived.Contains(coord);

                CellState state = (hasShip, isShot) switch
                {
                    (true, true) => IsShipSunk(grid, coord) ? CellState.Sunk : CellState.Hit,
                    (true, false) => CellState.Ship,
                    (false, true) => CellState.Miss,
                    (false, false) => CellState.Empty
                };

                cells.Add(new CellDto(r, c, state, ship?.Type));
            }
        }
        return cells;
    }

    // Grille Adversaire : MASQUAGE STRICT. Les bateaux non touchés restent "Empty"
    private static List<CellDto> BuildOpponentGridCells(Grid grid, bool revealShips)
    {
        var cells = new List<CellDto>();

        for (int r = 0; r < 10; r++)
        {
            for (int c = 0; c < 10; c++)
            {
                var coord = new Coordinate(r, c);
                var ship = grid.Ships.FirstOrDefault(s => s.OccupiedCoordinates.Contains(coord));
                bool hasShip = ship is not null;
                bool isShot = grid.ShotsReceived.Contains(coord);

                CellState state = (hasShip, isShot) switch
                {
                    (true, true) => IsShipSunk(grid, coord) ? CellState.Sunk : CellState.Hit,
                    (false, true) => CellState.Miss,
                    (true, false) when revealShips => CellState.Ship,
                    _ => CellState.Empty
                };

                ShipType? exposedType = state switch
                {
                    CellState.Sunk => ship?.Type,
                    CellState.Ship when revealShips => ship?.Type,
                    _ => null
                };

                cells.Add(new CellDto(r, c, state, exposedType));
            }
        }
        return cells;
    }

    private static bool IsShipSunk(Grid grid, Coordinate coord)
    {
        var ship = grid.Ships.FirstOrDefault(s => s.OccupiedCoordinates.Contains(coord));
        return ship != null && ship.OccupiedCoordinates.All(c => grid.ShotsReceived.Contains(c));
    }
}