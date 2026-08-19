using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Commands;

public sealed record CriarServicoCommand(string Nome, string Descricao, decimal PrecoBase, int TempoEstimadoMinutos) : IRequest<Result<Guid>>;
