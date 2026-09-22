namespace BattleShip.Models.Enums;

/// <summary>
/// Définit les différents types de navires.
/// </summary>
public enum ShipType
{
    Carrier = 1,       // Porte-avions (5 cases)
    Battleship = 2,    // Cuirassé (4 cases)
    Destroyer = 3,     // Croiseur (3 cases)
    Submarine = 4,     // Sous-marin (3 cases)
    TorpedoBoat = 5    // Torpilleur (2 cases)
}