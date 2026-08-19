using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Enums;
using OficinaMecanica.Domain.Oficina.Enums;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class CancelarOrdemDeServicoHandler : IRequestHandler<CancelarOrdemDeServicoCommand, Result>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelarOrdemDeServicoHandler(IOrdemDeServicoRepository ordemDeServicoRepository, IOrcamentoRepository orcamentoRepository, IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _orcamentoRepository = orcamentoRepository;
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CancelarOrdemDeServicoCommand request, CancellationToken cancellationToken)
    {
        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.Failure("Ordem de Servico nao encontrada");
        }

        if (ordemDeServico.Status == StatusOS.EmExecucao && ordemDeServico.OrcamentoId.HasValue)
        {
            var orcamento = await _orcamentoRepository.GetByIdAsync(ordemDeServico.OrcamentoId.Value, cancellationToken);

            if (orcamento is null || orcamento.Status != StatusOrcamento.Aprovado)
            {
                return Result.Failure("Orcamento aprovado nao encontrado");
            }

            var pecaIds = orcamento.Itens.Where(item => item.Tipo == TipoItem.Peca && item.PecaId.HasValue).Select(item => item.PecaId!.Value).Distinct();

            foreach (var pecaId in pecaIds)
            {
                var peca = await _pecaRepository.GetByIdAsync(pecaId, cancellationToken);

                if (peca is null)
                {
                    return Result.Failure($"Peca {pecaId} nao encontrada");
                }

                var reservas = peca.Reservas.Where(reserva => reserva.OrdemDeServicoId == ordemDeServico.Id && reserva.Status == StatusReserva.Ativa).ToList();

                foreach (var reserva in reservas)
                {
                    var liberacaoResult = peca.LiberarReserva(reserva.Id);

                    if (liberacaoResult.IsFailure)
                    {
                        return liberacaoResult;
                    }
                }

                _pecaRepository.Update(peca);
            }
        }

        var cancelamentoResult = ordemDeServico.Cancelar();

        if (cancelamentoResult.IsFailure)
        {
            return cancelamentoResult;
        }

        _ordemDeServicoRepository.Update(ordemDeServico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
