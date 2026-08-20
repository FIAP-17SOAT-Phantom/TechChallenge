using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Entities;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Domain.Orcamentacao.ValueObjects;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class GerarOrcamentoHandler : IRequestHandler<GerarOrcamentoCommand, Result<Guid>>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GerarOrcamentoHandler(IOrdemDeServicoRepository ordemDeServicoRepository, IOrcamentoRepository orcamentoRepository, IServicoRepository servicoRepository, IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _orcamentoRepository = orcamentoRepository;
        _servicoRepository = servicoRepository;
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(GerarOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.NotFound<Guid>("Ordem de Servico nao encontrada");
        }

        var orcamentoAtual = await _orcamentoRepository.GetByOrdemDeServicoIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (orcamentoAtual is not null && orcamentoAtual.Status != StatusOrcamento.Rejeitado && orcamentoAtual.Status != StatusOrcamento.Cancelado)
        {
            return Result.Conflict<Guid>("Ja existe um orcamento ativo para esta Ordem de Servico");
        }

        var itens = new List<ItemOrcamento>();

        foreach (var itemOrdemDeServico in ordemDeServico.Itens)
        {
            var servico = await _servicoRepository.GetByIdAsync(itemOrdemDeServico.ServicoId, cancellationToken);

            if (servico is null || !servico.Ativo)
            {
                return Result.NotFound<Guid>($"Servico {itemOrdemDeServico.ServicoId} nao encontrado ou inativo");
            }

            itens.Add(new ItemOrcamento(servico.Nome, TipoItem.Servico, itemOrdemDeServico.Quantidade, servico.PrecoBase, servicoId: servico.Id));

            if (itemOrdemDeServico.PecaId.HasValue)
            {
                var peca = await _pecaRepository.GetByIdAsync(itemOrdemDeServico.PecaId.Value, cancellationToken);

                if (peca is null)
                {
                    return Result.NotFound<Guid>($"Peca {itemOrdemDeServico.PecaId.Value} nao encontrada");
                }

                itens.Add(new ItemOrcamento(peca.Nome, TipoItem.Peca, itemOrdemDeServico.Quantidade, peca.PrecoUnitario, pecaId: peca.Id));
            }
        }

        var versao = await _orcamentoRepository.GetVersaoAtualAsync(request.OrdemDeServicoId, cancellationToken) + 1;
        var orcamento = new Orcamento(request.OrdemDeServicoId, versao, itens, request.Observacao);
        var vinculoResult = ordemDeServico.VincularOrcamento(orcamento.Id);

        if (vinculoResult.IsFailure)
        {
            return Result.Failure<Guid>(vinculoResult.Error, vinculoResult.ErrorType);
        }

        await _orcamentoRepository.AddAsync(orcamento, cancellationToken);
        _ordemDeServicoRepository.Update(ordemDeServico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(orcamento.Id);
    }
}
