using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed record ConsultarClienteQuery(Guid ClienteId) : IRequest<Result<ClienteDto>>;
