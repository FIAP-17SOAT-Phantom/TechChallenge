using MediatR;

namespace OficinaMecanica.Application.UseCases.Oficina.Queries;

public sealed record ListarFilaOperacionalQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<IReadOnlyList<OrdemDeServicoOperacionalDto>>;
public sealed record OrdemDeServicoOperacionalDto(Guid Id, string Numero, string Status, Guid ClienteId, Guid VeiculoId, Guid? MecanicoId, DateTime DataAbertura);
