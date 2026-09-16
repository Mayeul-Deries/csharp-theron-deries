using BattleShip.Models.DTOs;
using FluentValidation;

namespace BattleShip.API.Validation;

public sealed class ShotRequestValidator : AbstractValidator<ShotRequest>
{
    public ShotRequestValidator()
    {
        RuleFor(request => request.Row).InclusiveBetween(0, 9);
        RuleFor(request => request.Col).InclusiveBetween(0, 9);
    }
}