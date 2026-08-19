using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed class AtualizarPecaValidator : AbstractValidator<AtualizarPecaCommand>
{
    public AtualizarPecaValidator()
    {
        RuleFor(x => x.PecaId).NotEmpty().WithMessage("Peca e obrigatoria");
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome e obrigatorio").MaximumLength(200).WithMessage("Nome deve ter no maximo 200 caracteres");
        RuleFor(x => x.Descricao).MaximumLength(500).WithMessage("Descricao deve ter no maximo 500 caracteres");
        RuleFor(x => x.PrecoUnitario).GreaterThanOrEqualTo(0).WithMessage("Preco unitario nao pode ser negativo");
        RuleFor(x => x.QuantidadeMinima).GreaterThanOrEqualTo(0).WithMessage("Quantidade minima nao pode ser negativa");
    }
}
