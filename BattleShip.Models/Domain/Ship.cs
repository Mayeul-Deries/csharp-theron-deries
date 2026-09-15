using BattleShip.Models.Enums;

namespace BattleShip.Models.Domain;

public class Ship
{
    public ShipType Type { get; }
    public Coordinate Origin { get; }
    public Direction Direction { get; }
    public List<Coordinate> OccupiedCoordinates { get; } = new();

    public Ship(ShipType type, Coordinate origin, Direction direction)
    {
        Type = type;
        Origin = origin;
        Direction = direction;
        CalculateOccupiedCoordinates();
    }

    private void CalculateOccupiedCoordinates()
    {
        int length = (int)Type;
        for (int i = 0; i < length; i++)
        {
            int row = Origin.Row + (Direction == Direction.Vertical ? i : 0);
            int col = Origin.Col + (Direction == Direction.Horizontal ? i : 0);
            OccupiedCoordinates.Add(new Coordinate(row, col));
        }
    }
}