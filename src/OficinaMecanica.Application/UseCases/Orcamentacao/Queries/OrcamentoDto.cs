namespace OficinaMecanica.Application.UseCases.Orcamentacao.Queries;

public sealed record OrcamentoDto(Guid Id, Guid OrdemDeServicoId, int Versao, string Status, decimal ValorTotal, DateTime DataCriacao, DateTime? DataAprovacao, string? Observacao, IReadOnlyList<ItemOrcamentoDto> Itens);

public sealed record ItemOrcamentoDto(string Descricao, string Tipo, int Quantidade, decimal ValorUnitario, decimal ValorTotal, Guid? PecaId, Guid? ServicoId);
