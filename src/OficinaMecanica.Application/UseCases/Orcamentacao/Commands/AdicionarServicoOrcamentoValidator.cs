using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class AdicionarServicoOrcamentoValidator : AbstractValidator<AdicionarServicoOrcamentoCommand>
{
    public AdicionarServicoOrcamentoValidator()
    {
        RuleFor(x => x.OrcamentoId).NotEmpty().WithMessage("Orcamento e obrigatorio");
        RuleFor(x => x.ServicoId).NotEmpty().WithMessage("Servico e obrigatorio");
        RuleFor(x => x.Quantidade).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");
    }
}
