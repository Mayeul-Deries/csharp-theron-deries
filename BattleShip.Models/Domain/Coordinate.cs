namespace BattleShip.Models.Domain;

public readonly record struct Coordinate(int Row, int Col)
{
    public bool IsValid() => Row is >= 0 and < 10 && Col is >= 0 and < 10;
}