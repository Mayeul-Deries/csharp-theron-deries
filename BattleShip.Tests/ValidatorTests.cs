using BattleShip.API.Validation;
using BattleShip.Models.DTOs;
using BattleShip.Models.Enums;
using Xunit;

namespace BattleShip.Tests;

public class ValidatorTests
{
    private readonly ShotRequestValidator _validator = new();

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, 10, false)]
    public void ShotRequestValidator_ShouldValidateCoordinates(int row, int col, bool expectedValid)
    {
        var request = new ShotRequest(row, col);
        var result = _validator.Validate(request);
        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void CreateGameRequestValidator_ShouldValidatePlacementsCount()
    {
        var validator = new CreateGameRequestValidator();
        var valid = new CreateGameRequest(
        [
            new(ShipType.Carrier, 0, 0, Direction.Horizontal),
            new(ShipType.Battleship, 1, 0, Direction.Horizontal),
            new(ShipType.Destroyer, 2, 0, Direction.Horizontal),
            new(ShipType.Submarine, 3, 0, Direction.Horizontal),
            new(ShipType.TorpedoBoat, 4, 0, Direction.Horizontal)
        ]);
        var invalid = new CreateGameRequest(
        [
            new(ShipType.Carrier, 0, 0, Direction.Horizontal)
        ]);

        Assert.True(validator.Validate(valid).IsValid);
        Assert.False(validator.Validate(invalid).IsValid);
    }
}

