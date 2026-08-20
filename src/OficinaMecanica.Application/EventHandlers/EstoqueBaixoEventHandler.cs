using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Estoque.Events;

namespace OficinaMecanica.Application.EventHandlers;

public sealed class EstoqueBaixoEventHandler : INotificationHandler<EstoqueBaixoEvent>
{
    private readonly IAlertaEstoqueRepository _alertaEstoqueRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EstoqueBaixoEventHandler(IAlertaEstoqueRepository alertaEstoqueRepository, IUnitOfWork unitOfWork)
    {
        _alertaEstoqueRepository = alertaEstoqueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(EstoqueBaixoEvent notification, CancellationToken cancellationToken)
    {
        var alerta = await _alertaEstoqueRepository.GetAtivoByPecaIdAsync(notification.PecaId, cancellationToken);

        if (alerta is null)
        {
            alerta = new AlertaEstoque(notification.PecaId, notification.NomePeca, notification.QuantidadeDisponivel, notification.QuantidadeMinima);
            await _alertaEstoqueRepository.AddAsync(alerta, cancellationToken);
        }
        else
        {
            alerta.AtualizarQuantidade(notification.QuantidadeDisponivel, notification.QuantidadeMinima);
            _alertaEstoqueRepository.Update(alerta);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
