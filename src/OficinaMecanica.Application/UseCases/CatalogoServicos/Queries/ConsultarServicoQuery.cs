using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Queries;

public sealed record ConsultarServicoQuery(Guid ServicoId) : IRequest<Result<ServicoDto>>;
