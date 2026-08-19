using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed record CriarPecaCommand(string Nome, string Codigo, string Descricao, decimal PrecoUnitario, int QuantidadeEmEstoque, int QuantidadeMinima) : IRequest<Result<Guid>>;
