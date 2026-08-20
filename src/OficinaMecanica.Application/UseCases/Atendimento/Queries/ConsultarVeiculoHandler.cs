using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed class ConsultarVeiculoHandler : IRequestHandler<ConsultarVeiculoQuery, Result<VeiculoDto>>
{
    private readonly IVeiculoRepository _veiculoRepository;

    public ConsultarVeiculoHandler(IVeiculoRepository veiculoRepository)
    {
        _veiculoRepository = veiculoRepository;
    }

    public async Task<Result<VeiculoDto>> Handle(ConsultarVeiculoQuery request, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.GetByIdAsync(request.VeiculoId, cancellationToken);

        if (veiculo is null)
        {
            return Result.NotFound<VeiculoDto>("Veiculo nao encontrado");
        }

        var dto = new VeiculoDto(
            veiculo.Id,
            veiculo.Placa.Valor,
            veiculo.Marca,
            veiculo.Modelo,
            veiculo.Ano,
            veiculo.ClienteId);

        return Result.Success(dto);
    }
}
