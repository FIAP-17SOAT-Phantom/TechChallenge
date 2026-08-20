using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class AdicionarPecaOrcamentoValidator : AbstractValidator<AdicionarPecaOrcamentoCommand>
{
    public AdicionarPecaOrcamentoValidator()
    {
        RuleFor(x => x.OrcamentoId).NotEmpty().WithMessage("Orcamento e obrigatorio");
        RuleFor(x => x.PecaId).NotEmpty().WithMessage("Peca e obrigatoria");
        RuleFor(x => x.Quantidade).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");
    }
}
