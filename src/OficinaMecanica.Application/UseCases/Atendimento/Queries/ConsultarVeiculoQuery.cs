using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed record ConsultarVeiculoQuery(Guid VeiculoId) : IRequest<Result<VeiculoDto>>;
