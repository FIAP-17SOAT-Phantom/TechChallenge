using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed record AdicionarEstoqueCommand(Guid PecaId, int Quantidade) : IRequest<Result>;
