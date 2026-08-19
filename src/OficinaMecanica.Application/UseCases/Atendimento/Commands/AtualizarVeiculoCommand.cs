using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed record AtualizarVeiculoCommand(Guid VeiculoId, string Marca, string Modelo, int Ano) : IRequest<Result>;
