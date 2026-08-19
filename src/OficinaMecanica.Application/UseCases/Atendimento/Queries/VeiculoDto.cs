namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed record VeiculoDto(Guid Id, string Placa, string Marca, string Modelo, int Ano, Guid ClienteId);
