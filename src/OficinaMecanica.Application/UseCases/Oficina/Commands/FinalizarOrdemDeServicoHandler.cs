using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Estoque.Enums;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class FinalizarOrdemDeServicoHandler : IRequestHandler<FinalizarOrdemDeServicoCommand, Result>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FinalizarOrdemDeServicoHandler(IOrdemDeServicoRepository ordemDeServicoRepository, IOrcamentoRepository orcamentoRepository, IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _orcamentoRepository = orcamentoRepository;
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(FinalizarOrdemDeServicoCommand request, CancellationToken cancellationToken)
    {
        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.NotFound("Ordem de Servico nao encontrada");
        }

        if (!ordemDeServico.OrcamentoId.HasValue)
        {
            return Result.Failure("Ordem de Servico nao possui orcamento vinculado");
        }

        var orcamento = await _orcamentoRepository.GetByIdAsync(ordemDeServico.OrcamentoId.Value, cancellationToken);

        if (orcamento is null || orcamento.Status != StatusOrcamento.Aprovado)
        {
            return Result.NotFound("Orcamento aprovado nao encontrado");
        }

        var pecas = new List<Peca>();
        var pecaIds = orcamento.Itens.Where(item => item.Tipo == TipoItem.Peca && item.PecaId.HasValue).Select(item => item.PecaId!.Value).Distinct();

        foreach (var pecaId in pecaIds)
        {
            var peca = await _pecaRepository.GetByIdAsync(pecaId, cancellationToken);

            if (peca is null)
            {
                return Result.NotFound($"Peca {pecaId} nao encontrada");
            }

            if (!peca.Reservas.Any(reserva => reserva.OrdemDeServicoId == ordemDeServico.Id && reserva.Status == StatusReserva.Ativa))
            {
                return Result.Conflict($"Reserva ativa da peca {peca.Nome} nao encontrada");
            }

            pecas.Add(peca);
        }

        foreach (var peca in pecas)
        {
            var reservas = peca.Reservas.Where(reserva => reserva.OrdemDeServicoId == ordemDeServico.Id && reserva.Status == StatusReserva.Ativa).ToList();

            foreach (var reserva in reservas)
            {
                var consumoResult = peca.Consumir(reserva.Id);

                if (consumoResult.IsFailure)
                {
                    return consumoResult;
                }
            }

            _pecaRepository.Update(peca);
        }

        var finalizacaoResult = ordemDeServico.Finalizar();

        if (finalizacaoResult.IsFailure)
        {
            return finalizacaoResult;
        }

        _ordemDeServicoRepository.Update(ordemDeServico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
