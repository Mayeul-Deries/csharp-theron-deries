using BattleShip.Models.Domain;
using BattleShip.Models.Enums;
using BattleShip.Models.Privacy;
using Xunit;

namespace BattleShip.Tests;

public class PrivacyTests
{
    [Fact]
    public void ToStatusDto_ShouldHideUnshotOpponentShips()
    {
        // Arrange
        var engine = new GameEngine();
        engine.SetupOpponentGridWithDefaultShips();
        engine.StartGame();

        // Act
        var dto = GamePrivacyMapper.ToStatusDto(Guid.NewGuid(), engine);

        // Assert
        // Aucun navire adverse ne doit avoir l'état "Ship" dans le DTO
        Assert.DoesNotContain(dto.OpponentGrid, cell => cell.State == CellState.Ship);
        
        // Toutes les cases non visées doivent être marquées "Empty"
        Assert.All(dto.OpponentGrid, cell => Assert.Equal(CellState.Empty, cell.State));
    }
}