using FluentValidation;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Queries;

public sealed class ListarServicosValidator : AbstractValidator<ListarServicosQuery>
{
    public ListarServicosValidator()
    {
        RuleFor(x => x.Pagina).GreaterThan(0).WithMessage("Pagina deve ser maior que zero");
        RuleFor(x => x.TamanhoPagina).InclusiveBetween(1, 100).WithMessage("Tamanho da pagina deve estar entre 1 e 100");
    }
}
