using BattleShip.Models.Validators;
using BattleShip.Models.DTOs;
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
}
