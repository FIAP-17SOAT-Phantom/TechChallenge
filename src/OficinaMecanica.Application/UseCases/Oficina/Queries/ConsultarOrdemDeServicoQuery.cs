using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed record ConsultarOrdemDeServicoQuery(Guid OrdemDeServicoId) : IRequest<Result<OrdemDeServicoDto>>;

public sealed record OrdemDeServicoDto(Guid Id, string Numero, string Status, Guid ClienteId, Guid VeiculoId, Guid? MecanicoId, DateTime DataAbertura, DateTime? DataInicioExecucao, DateTime? DataFinalizacao, string? Diagnostico, Guid? OrcamentoId, IReadOnlyList<ItemOrdemDeServicoDto> Itens, IReadOnlyList<ServicoSolicitadoDto> ServicosSolicitados, IReadOnlyList<PecaSolicitadaDto> PecasSolicitadas);

public sealed record ItemOrdemDeServicoDto(Guid ServicoId, Guid? PecaId, int Quantidade, string? Observacao, bool Executado, DateTime? DataExecucao);
public sealed record ServicoSolicitadoDto(Guid ServicoId, int Quantidade, string? Observacao);
public sealed record PecaSolicitadaDto(Guid PecaId, int Quantidade, string? Observacao);
