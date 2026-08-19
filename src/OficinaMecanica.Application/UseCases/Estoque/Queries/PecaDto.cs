namespace OficinaMecanica.Application.UseCases.Estoque.Queries;

public sealed record PecaDto(Guid Id, string Nome, string Codigo, string Descricao, decimal PrecoUnitario, int QuantidadeEmEstoque, int QuantidadeReservada, int QuantidadeDisponivel, int QuantidadeMinima);
