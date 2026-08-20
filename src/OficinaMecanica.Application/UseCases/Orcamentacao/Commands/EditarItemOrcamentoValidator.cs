using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class EditarItemOrcamentoValidator : AbstractValidator<EditarItemOrcamentoCommand>
{
    public EditarItemOrcamentoValidator()
    {
        RuleFor(x => x.OrcamentoId).NotEmpty().WithMessage("Orcamento e obrigatorio");
        RuleFor(x => x.ReferenciaId).NotEmpty().WithMessage("Referencia do item e obrigatoria");
        RuleFor(x => x.Quantidade).NotNull().GreaterThan(0).When(x => !x.Remover).WithMessage("Quantidade deve ser maior que zero");
    }
}
