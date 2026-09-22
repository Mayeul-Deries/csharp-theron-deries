using FluentValidation;
using BattleShip.Models.DTOs;

namespace BattleShip.Models.Validators;

public class ShotRequestValidator : AbstractValidator<ShotRequest>
{
    public ShotRequestValidator()
    {
        RuleFor(x => x.Row).InclusiveBetween(0, 9);
        RuleFor(x => x.Col).InclusiveBetween(0, 9);
    }
}
