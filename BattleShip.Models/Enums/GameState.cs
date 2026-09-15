namespace BattleShip.Models.Enums;

/// <summary>
/// Définit les différentes phases d'une partie de bataille navale.
/// </summary>
public enum GameState
{
    Setup,         // Placement des navires
    PlayerTurn,    // Tour du joueur humain
    OpponentTurn,  // Tour de l'IA / adversaire
    PlayerWon,     // Partie terminée, victoire du joueur
    OpponentWon    // Partie terminée, victoire de l'adversaire
}