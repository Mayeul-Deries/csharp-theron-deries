using Bunit;
using BattleShip.App.Components;
using BattleShip.Models.DTOs;
using BattleShip.Models.Enums;

namespace BattleShip.Tests.Frontend;

public class BattleGridTests : BunitContext
{
    [Fact]
    public void Render_ShouldDisplayOneButtonPerCell()
    {
        // Arrange
        var cells = Enumerable.Range(0, 100)
            .Select(index => new CellDto(index / 10, index % 10, CellState.Empty))
            .ToList();

        // Act
        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells));

        // Assert
        Assert.Equal(100, renderedGrid.FindAll("button").Count);
    }
}
