using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed class ListarVeiculosPorClienteHandler : IRequestHandler<ListarVeiculosPorClienteQuery, IReadOnlyList<VeiculoDto>>
{
    private readonly IVeiculoRepository _veiculoRepository;

    public ListarVeiculosPorClienteHandler(IVeiculoRepository veiculoRepository)
    {
        _veiculoRepository = veiculoRepository;
    }

    public async Task<IReadOnlyList<VeiculoDto>> Handle(ListarVeiculosPorClienteQuery request, CancellationToken cancellationToken)
    {
        var veiculos = await _veiculoRepository.GetPagedByClienteIdAsync(request.ClienteId, request.Pagina, request.TamanhoPagina, cancellationToken);

        return veiculos
            .Select(veiculo => new VeiculoDto(
                veiculo.Id,
                veiculo.Placa.Valor,
                veiculo.Marca,
                veiculo.Modelo,
                veiculo.Ano,
                veiculo.ClienteId))
            .ToList();
    }
}
