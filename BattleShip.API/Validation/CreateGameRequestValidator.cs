using BattleShip.Models.DTOs;
using FluentValidation;

namespace BattleShip.API.Validation;

public sealed class CreateGameRequestValidator : AbstractValidator<CreateGameRequest>
{
    public CreateGameRequestValidator()
    {
        RuleFor(request => request.Ships)
            .Must(ships => ships is not null && ships.Count == 5)
            .When(request => request.Ships is not null);

        RuleForEach(request => request.Ships)
            .ChildRules(placement =>
            {
                placement.RuleFor(item => item.Row).InclusiveBetween(0, 9);
                placement.RuleFor(item => item.Col).InclusiveBetween(0, 9);
                placement.RuleFor(item => item.ShipType).IsInEnum();
                placement.RuleFor(item => item.Direction).IsInEnum();
            });
    }
}
