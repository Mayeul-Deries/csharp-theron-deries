using BattleShip.Models.Enums;

namespace BattleShip.Models.DTOs;

public record CellDto(int Row, int Col, CellState State);

public record GameStatusDto(
    Guid GameId,
    GameState State,
    List<CellDto> PlayerGrid,
    List<CellDto> OpponentGrid
);