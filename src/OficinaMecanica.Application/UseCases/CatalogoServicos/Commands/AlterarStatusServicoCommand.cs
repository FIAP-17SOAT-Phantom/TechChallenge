using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Commands;

public sealed record AlterarStatusServicoCommand(Guid ServicoId, bool Ativo) : IRequest<Result>;
