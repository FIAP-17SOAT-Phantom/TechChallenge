using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed class ListarVeiculosPorClienteValidator : AbstractValidator<ListarVeiculosPorClienteQuery>
{
    public ListarVeiculosPorClienteValidator()
    {
        RuleFor(x => x.ClienteId).NotEmpty().WithMessage("Cliente e obrigatorio");
        RuleFor(x => x.Pagina).GreaterThan(0).WithMessage("Pagina deve ser maior que zero");
        RuleFor(x => x.TamanhoPagina).InclusiveBetween(1, 100).WithMessage("Tamanho da pagina deve estar entre 1 e 100");
    }
}
