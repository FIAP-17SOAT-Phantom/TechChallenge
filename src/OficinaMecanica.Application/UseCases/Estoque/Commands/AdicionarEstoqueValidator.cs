using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed class AdicionarEstoqueValidator : AbstractValidator<AdicionarEstoqueCommand>
{
    public AdicionarEstoqueValidator()
    {
        RuleFor(x => x.PecaId).NotEmpty().WithMessage("Peca e obrigatoria");
        RuleFor(x => x.Quantidade).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");
    }
}
