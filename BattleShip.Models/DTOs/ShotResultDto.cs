using BattleShip.Models.Enums;

namespace BattleShip.Models.DTOs;

public record ShotResultDto(
    ShotResult Result,
    GameState NextState,
    int Row,
    int Col
);