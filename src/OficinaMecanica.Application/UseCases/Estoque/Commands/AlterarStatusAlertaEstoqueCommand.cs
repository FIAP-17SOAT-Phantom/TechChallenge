using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed record AlterarStatusAlertaEstoqueCommand(Guid AlertaId, bool Resolver) : IRequest<Result>;
