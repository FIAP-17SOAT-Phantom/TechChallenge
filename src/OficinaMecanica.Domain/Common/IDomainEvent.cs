using MediatR;

namespace OficinaMecanica.Domain.Common;

public interface IDomainEvent : INotification
{
 DateTime OccurredOn { get; }
}
