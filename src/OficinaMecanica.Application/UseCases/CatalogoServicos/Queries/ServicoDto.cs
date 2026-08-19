namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Queries;

public sealed record ServicoDto(Guid Id, string Nome, string Descricao, decimal PrecoBase, int TempoEstimadoMinutos, bool Ativo);
