using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed record CriarVeiculoCommand(string Placa, string Marca, string Modelo, int Ano, Guid ClienteId) : IRequest<Result<Guid>>;
