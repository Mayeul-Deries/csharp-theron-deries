namespace BattleShip.Models.Enums;

/// <summary>
/// Définit les différents types de navires et leur taille associée.
/// </summary>
public enum ShipType
{
    Carrier = 5,       // Porte-avions (5 cases)
    Battleship = 4,    // Croiseur (4 cases)
    Destroyer = 3,     // Contre-torpilleur (3 cases)
    Submarine = 3,     // Sous-marin (3 cases)
    TorpedoBoat = 2    // Torpilleur (2 cases)
}