using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Estoque.Events;

public sealed record EstoqueBaixoEvent(Guid PecaId, string NomePeca, int QuantidadeDisponivel, int QuantidadeMinima) : DomainEvent;
