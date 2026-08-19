using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class AtualizarVeiculoValidator : AbstractValidator<AtualizarVeiculoCommand>
{
    public AtualizarVeiculoValidator()
    {
        RuleFor(x => x.VeiculoId)
            .NotEmpty().WithMessage("Veiculo e obrigatorio");

        RuleFor(x => x.Marca)
            .NotEmpty().WithMessage("Marca e obrigatoria")
            .MaximumLength(50).WithMessage("Marca deve ter no maximo 50 caracteres");

        RuleFor(x => x.Modelo)
            .NotEmpty().WithMessage("Modelo e obrigatorio")
            .MaximumLength(50).WithMessage("Modelo deve ter no maximo 50 caracteres");

        RuleFor(x => x.Ano)
            .InclusiveBetween(1900, DateTime.UtcNow.Year + 2).WithMessage("Ano invalido");
    }
}
