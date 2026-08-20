using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed class ObterIndicadoresOficinaHandler : IRequestHandler<ObterIndicadoresOficinaQuery, IndicadoresOficinaDto>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;

    public ObterIndicadoresOficinaHandler(IOrdemDeServicoRepository ordemDeServicoRepository)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
    }

    public async Task<IndicadoresOficinaDto> Handle(ObterIndicadoresOficinaQuery request, CancellationToken cancellationToken)
    {
        var tempoMedio = await _ordemDeServicoRepository.GetTempoMedioExecucaoAsync(cancellationToken);

        if (!tempoMedio.HasValue)
        {
            return new IndicadoresOficinaDto(null, null);
        }

        return new IndicadoresOficinaDto(tempoMedio.Value.TotalMinutes, $"{(int)tempoMedio.Value.TotalHours:D2}:{tempoMedio.Value.Minutes:D2}:{tempoMedio.Value.Seconds:D2}");
    }
}
