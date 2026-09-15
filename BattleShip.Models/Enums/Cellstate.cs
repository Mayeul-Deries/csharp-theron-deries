namespace BattleShip.Models.Enums;

/// <summary>
/// Définit l'état d'une case sur la grille de jeu.
/// </summary>
public enum CellState
{
    Empty,      // Case d'eau non visée
    Ship,       // Présence d'un navire (masqué au joueur adverse)
    Hit,        // Tir ayant touché un navire
    Miss,       // Tir ayant manqué (dans l'eau)
    Sunk        // Case appartenant à un navire complètement coulé
}