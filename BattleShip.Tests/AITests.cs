using BattleShip.Models.AI;
using BattleShip.Models.Domain;
using BattleShip.Models.Enums;
using Xunit;

namespace BattleShip.Tests;

public class AiTests
{
    private readonly AiOpponentService _aiService = new();

    [Fact]
    public void GenerateShot_ShouldReturnValidCoordinate_NotAlreadyShot()
    {
        // Arrange
        var grid = new Grid();
        grid.ShotsReceived.Add(new Coordinate(0, 0));

        // Act
        var shot = _aiService.GenerateShot(grid);

        // Assert
        Assert.True(shot.IsValid());
        Assert.NotEqual(new Coordinate(0, 0), shot);
    }

    [Fact]
    public void GenerateShot_ShouldTargetAdjacentCoordinate_WhenShipIsHitButNotSunk()
    {
        // Arrange
        var grid = new Grid();
        // Torpilleur en (2,2) et (2,3)
        var ship = new Ship(ShipType.TorpedoBoat, new Coordinate(2, 2), Direction.Horizontal);
        grid.TryAddShip(ship);

        // Simulation d'un premier tir réussi en (2,2)
        grid.ShotsReceived.Add(new Coordinate(2, 2));

        // Act
        var shot = _aiService.GenerateShot(grid);

        // Assert : l'IA doit viser une case adjacente à (2,2)
        var expectedAdjacent = new List<Coordinate>
        {
            new Coordinate(1, 2),
            new Coordinate(3, 2),
            new Coordinate(2, 1),
            new Coordinate(2, 3)
        };

        Assert.Contains(shot, expectedAdjacent);
    }
}