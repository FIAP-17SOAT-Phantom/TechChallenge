using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class CriarVeiculoValidator : AbstractValidator<CriarVeiculoCommand>
{
    public CriarVeiculoValidator()
    {
        RuleFor(x => x.Placa)
            .NotEmpty().WithMessage("Placa e obrigatoria");

        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("Marca e obrigatoria")
            .MaximumLength(50).WithMessage("Marca deve ter no maximo 50 caracteres");

        RuleFor(x => x.Modelo)
            .NotEmpty().WithMessage("Modelo e obrigatorio")
            .MaximumLength(50).WithMessage("Modelo deve ter no maximo 50 caracteres");

        RuleFor(x => x.Ano)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 2).WithMessage("Ano invalido");

        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("Cliente e obrigatorio");
    }
}
