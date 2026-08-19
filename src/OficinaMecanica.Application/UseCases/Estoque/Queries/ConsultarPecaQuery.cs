using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Queries;

public sealed record ConsultarPecaQuery(Guid PecaId) : IRequest<Result<PecaDto>>;
