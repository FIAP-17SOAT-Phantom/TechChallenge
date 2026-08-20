using MediatR;

namespace OficinaMecanica.Application.UseCases.Estoque.Queries;

public sealed record ListarAlertasEstoqueQuery(bool SomenteAtivos, int Pagina = 1, int TamanhoPagina = 20) : IRequest<IReadOnlyList<AlertaEstoqueDto>>;

public sealed record AlertaEstoqueDto(Guid Id, Guid PecaId, string NomePeca, int QuantidadeDisponivel, int QuantidadeMinima, bool Visualizado, bool Resolvido, DateTime DataCriacao, DateTime? DataVisualizacao, DateTime? DataResolucao);
