using FluentValidation;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Commands;

public sealed class AtualizarServicoValidator : AbstractValidator<AtualizarServicoCommand>
{
    public AtualizarServicoValidator()
    {
        RuleFor(x => x.ServicoId)
            .NotEmpty().WithMessage("Servico e obrigatorio");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome e obrigatorio")
            .MaximumLength(200).WithMessage("Nome deve ter no maximo 200 caracteres");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("Descricao deve ter no maximo 500 caracteres");

        RuleFor(x => x.PrecoBase)
            .GreaterThanOrEqualTo(0).WithMessage("Preco base nao pode ser negativo");

        RuleFor(x => x.TempoEstimadoMinutos)
            .GreaterThan(0).WithMessage("Tempo estimado deve ser maior que zero");
    }
}
