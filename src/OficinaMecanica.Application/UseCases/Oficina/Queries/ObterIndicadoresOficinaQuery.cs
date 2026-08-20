using MediatR;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed record ObterIndicadoresOficinaQuery : IRequest<IndicadoresOficinaDto>;

public sealed record IndicadoresOficinaDto(double? TempoMedioExecucaoEmMinutos, string? TempoMedioExecucaoFormatado);
