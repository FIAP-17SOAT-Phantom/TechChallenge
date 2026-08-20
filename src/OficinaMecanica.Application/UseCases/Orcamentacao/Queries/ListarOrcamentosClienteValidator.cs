using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Queries;

public sealed class ListarOrcamentosClienteValidator : AbstractValidator<ListarOrcamentosClienteQuery>
{
    public ListarOrcamentosClienteValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty().WithMessage("Cliente e obrigatorio");
        RuleFor(x => x.Pagina).GreaterThan(0).WithMessage("Pagina deve ser maior que zero");
        RuleFor(x => x.TamanhoPagina).InclusiveBetween(1, 100).WithMessage("Tamanho da pagina deve estar entre 1 e 100");
    }
}
