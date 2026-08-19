using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed class CriarPecaValidator : AbstractValidator<CriarPecaCommand>
{
    public CriarPecaValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome e obrigatorio").MaximumLength(200).WithMessage("Nome deve ter no maximo 200 caracteres");
        RuleFor(x => x.Codigo).NotEmpty().WithMessage("Codigo e obrigatorio").MaximumLength(50).WithMessage("Codigo deve ter no maximo 50 caracteres");
        RuleFor(x => x.Descricao).MaximumLength(500).WithMessage("Descricao deve ter no maximo 500 caracteres");
        RuleFor(x => x.PrecoUnitario).GreaterThanOrEqualTo(0).WithMessage("Preco unitario nao pode ser negativo");
        RuleFor(x => x.QuantidadeEmEstoque).GreaterThanOrEqualTo(0).WithMessage("Quantidade em estoque nao pode ser negativa");
        RuleFor(x => x.QuantidadeMinima).GreaterThanOrEqualTo(0).WithMessage("Quantidade minima nao pode ser negativa");
    }
}
