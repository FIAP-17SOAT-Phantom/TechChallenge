using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed record ExcluirPecaCommand(Guid PecaId) : IRequest<Result>;
