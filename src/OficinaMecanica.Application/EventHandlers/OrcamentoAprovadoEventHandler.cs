using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Domain.Orcamentacao.Events;

namespace OficinaMecanica.Application.EventHandlers;

/// <summary>
/// Reage ao evento OrcamentoAprovado para executar as politicas:
/// P1: Reservar pecas no estoque
/// P2: Mudar status da OS para EmExecucao
/// 
/// Este handler conecta os BCs Orcamentacao, Estoque e Oficina
/// via Domain Events, mantendo baixo acoplamento entre eles.
/// 
/// NOTA: Em um sistema distribuido, cada politica seria um handler separado
/// com consistencia eventual. No monolito atual, ambas as politicas
/// sao executadas na mesma transacao por simplicidade.
/// </summary>
public sealed class OrcamentoAprovadoEventHandler : INotificationHandler<OrcamentoAprovadoEvent>
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IOrdemDeServicoRepository _osRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrcamentoAprovadoEventHandler(
    IOrcamentoRepository orcamentoRepository,
    IOrdemDeServicoRepository osRepository,
    IPecaRepository pecaRepository,
    IUnitOfWork unitOfWork)
    {
        _orcamentoRepository = orcamentoRepository;
        _osRepository = osRepository;
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(OrcamentoAprovadoEvent notification, CancellationToken cancellationToken)
    {
        // P2: Mudar status da OS para EmExecucao
        var os = await _osRepository.GetByIdAsync(notification.OrdemDeServicoId, cancellationToken)
        ?? throw new InvalidOperationException($"OS {notification.OrdemDeServicoId} nao encontrada");

        os.IniciarExecucao();
        _osRepository.Update(os);

        // P1: Reservar pecas no estoque
        var orcamento = await _orcamentoRepository.GetByIdAsync(notification.OrcamentoId, cancellationToken)
        ?? throw new InvalidOperationException($"Orcamento {notification.OrcamentoId} nao encontrado");

        foreach (var item in orcamento.Itens.Where(i => i.Tipo == TipoItem.Peca && i.PecaId.HasValue))
        {
            var peca = await _pecaRepository.GetByIdAsync(item.PecaId!.Value, cancellationToken);
            if (peca is null) continue;

            var reservaResult = peca.Reservar(notification.OrdemDeServicoId, item.Quantidade);
            if (reservaResult.IsFailure)
                throw new InvalidOperationException($"Falha ao reservar peca {peca.Nome}: {reservaResult.Error}");

            _pecaRepository.Update(peca);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
