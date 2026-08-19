using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed record AtualizarPecaCommand(Guid PecaId, string Nome, string Descricao, decimal PrecoUnitario, int QuantidadeMinima) : IRequest<Result>;
