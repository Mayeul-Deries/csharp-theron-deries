using System.Text.Json.Serialization;
using BattleShip.Models.Enums;

namespace BattleShip.Models.DTOs;

public record CreateGameRequest(IReadOnlyList<ShipPlacementDto>? Ships = null);

public record ShipPlacementDto(
    [property: JsonConverter(typeof(JsonStringEnumConverter))] ShipType ShipType,
    int Row,
    int Col,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] Direction Direction);
