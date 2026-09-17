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

    [Fact]
    public void Render_WhenDisabled_ShouldDisableAllCells()
    {
        // Arrange
        var cells = new[] { new CellDto(2, 3, CellState.Empty) };

        // Act
        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells)
            .Add(component => component.IsDisabled, true));

        // Assert
        Assert.True(renderedGrid.Find("button").HasAttribute("disabled"));
    }

    [Fact]
    public void Click_WhenNotInteractive_ShouldNotRaiseSelectedCoordinate()
    {
        // Arrange
        var cells = new[] { new CellDto(2, 3, CellState.Empty) };
        Coordinate? selectedCoordinate = null;
        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells)
            .Add(component => component.IsInteractive, false)
            .Add(component => component.CellSelected,
                coordinate => selectedCoordinate = coordinate));

        // Act
        renderedGrid.Find("button").Click();

        // Assert
        Assert.Null(selectedCoordinate);
    }

    [Fact]
    public void Render_ShouldExposeGridAccessibilitySemantics()
    {
        // Arrange
        var cells = new[] { new CellDto(2, 3, CellState.Hit) };

        // Act
        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells));

        // Assert
        Assert.Equal("grid", renderedGrid.Find("[role='grid']").GetAttribute("role"));
        Assert.Equal("gridcell", renderedGrid.Find("[role='gridcell']")
            .GetAttribute("role"));
    }

    [Fact]
    public void Render_ShouldDescribeCellCoordinatesAndState()
    {
        // Arrange
        var cells = new[] { new CellDto(2, 3, CellState.Hit) };

        // Act
        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells));

        // Assert
        var label = renderedGrid.Find("button").GetAttribute("aria-label");
        Assert.Equal("Ligne 3, colonne 4, état Hit", label);
    }

    [Fact]
    public void Render_With100Cells_ShouldDisplayTacticalAxes()
    {
        var cells = Enumerable.Range(0, 100)
            .Select(index => new CellDto(index / 10, index % 10, CellState.Empty))
            .ToList();

        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells));

        Assert.Equal("LOC", renderedGrid.Find(".grid-axis-corner").TextContent.Trim());
        Assert.Equal(10, renderedGrid.FindAll(".grid-axis-col").Count);
        Assert.Equal(10, renderedGrid.FindAll(".grid-axis-row").Count);
    }

    [Fact]
    public void Render_WithSelectedCoordinate_ShouldAddCellSelectedClass()
    {
        var cells = new[] { new CellDto(2, 3, CellState.Empty) };

        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells)
            .Add(component => component.SelectedCoordinate, new Coordinate(2, 3)));

        Assert.Contains("cell-selected", renderedGrid.Find("button").ClassList);
        Assert.Contains("[ ⊙ ]", renderedGrid.Find("button").TextContent);
    }

    [Fact]
    public void Render_PlayerFleet_ShouldDisplayShipBadge()
    {
        var cells = new[] { new CellDto(0, 0, CellState.Ship, ShipType.Carrier) };

        var renderedGrid = Render<BattleGrid>(parameters => parameters
            .Add(component => component.Cells, cells)
            .Add(component => component.IsPlayerFleet, true));

        Assert.Contains("CV", renderedGrid.Find("button").TextContent);
    }
}
