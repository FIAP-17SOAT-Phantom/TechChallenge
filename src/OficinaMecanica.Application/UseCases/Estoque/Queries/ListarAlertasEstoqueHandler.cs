using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Estoque.Queries;

public sealed class ListarAlertasEstoqueHandler : IRequestHandler<ListarAlertasEstoqueQuery, IReadOnlyList<AlertaEstoqueDto>>
{
    private readonly IAlertaEstoqueRepository _alertaEstoqueRepository;

    public ListarAlertasEstoqueHandler(IAlertaEstoqueRepository alertaEstoqueRepository)
    {
        _alertaEstoqueRepository = alertaEstoqueRepository;
    }

    public async Task<IReadOnlyList<AlertaEstoqueDto>> Handle(ListarAlertasEstoqueQuery request, CancellationToken cancellationToken)
    {
        var alertas = await _alertaEstoqueRepository.GetAllAsync(request.SomenteAtivos, request.Pagina, request.TamanhoPagina, cancellationToken);

        return alertas.Select(alerta => new AlertaEstoqueDto(alerta.Id, alerta.PecaId, alerta.NomePeca, alerta.QuantidadeDisponivel, alerta.QuantidadeMinima, alerta.Visualizado, alerta.Resolvido, alerta.DataCriacao, alerta.DataVisualizacao, alerta.DataResolucao)).ToList();
    }
}
