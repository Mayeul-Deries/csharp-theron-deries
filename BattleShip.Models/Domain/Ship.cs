using BattleShip.Models.Enums;

namespace BattleShip.Models.Domain;

public class Ship
{
    public ShipType Type { get; }
    public Coordinate Origin { get; }
    public Direction Direction { get; }
    public List<Coordinate> OccupiedCoordinates { get; } = new();

    public int Length => GetLength(Type);

    public Ship(ShipType type, Coordinate origin, Direction direction)
    {
        Type = type;
        Origin = origin;
        Direction = direction;
        CalculateOccupiedCoordinates();
    }

    public static int GetLength(ShipType type) => type switch
    {
        ShipType.Carrier => 5,
        ShipType.Battleship => 4,
        ShipType.Destroyer => 3,
        ShipType.Submarine => 3,
        ShipType.TorpedoBoat => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };

    private void CalculateOccupiedCoordinates()
    {
        int length = GetLength(Type);
        for (int i = 0; i < length; i++)
        {
            int row = Origin.Row + (Direction == Direction.Vertical ? i : 0);
            int col = Origin.Col + (Direction == Direction.Horizontal ? i : 0);
            OccupiedCoordinates.Add(new Coordinate(row, col));
        }
    }
}