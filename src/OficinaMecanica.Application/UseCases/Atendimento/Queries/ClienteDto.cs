namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed record ClienteDto(Guid Id, string Nome, string Cpf, string Telefone, string Email);
