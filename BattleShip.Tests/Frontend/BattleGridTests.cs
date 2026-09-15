using Bunit;
using BattleShip.App.Components;
using BattleShip.Models.Domain;
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

    [Theory]
    [InlineData(CellState.Ship, "cell-ship")]
    [InlineData(CellState.Hit, "cell-hit")]
    [InlineData(CellState.Miss, "cell-miss")]
    [InlineData(CellState.Sunk, "cell-sunk")]
    public void Render_ShouldApplyCellStateCssClass(CellState state, string expectedClass)
    {
        // Arrange
        var cells = new[] { new CellDto(2, 3, state) };

        // Act
        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells));

        // Assert
        Assert.Contains(expectedClass, renderedGrid.Find("button").ClassList);
    }

    [Fact]
    public void Click_ShouldRaiseSelectedCoordinate()
    {
        // Arrange
        var cells = new[] { new CellDto(2, 3, CellState.Empty) };
        Coordinate? selectedCoordinate = null;
        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells)
            .Add(component => component.CellSelected,
                coordinate => selectedCoordinate = coordinate));

        // Act
        renderedGrid.Find("button").Click();

        // Assert
        Assert.Equal(new Coordinate(2, 3), selectedCoordinate);
    }
}
