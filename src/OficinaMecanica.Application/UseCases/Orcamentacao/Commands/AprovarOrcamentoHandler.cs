using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Oficina.Enums;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class AprovarOrcamentoHandler : IRequestHandler<AprovarOrcamentoCommand, Result>
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AprovarOrcamentoHandler(IOrcamentoRepository orcamentoRepository, IOrdemDeServicoRepository ordemDeServicoRepository, IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _orcamentoRepository = orcamentoRepository;
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AprovarOrcamentoCommand request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(transactionCancellationToken => AprovarAsync(request, transactionCancellationToken), cancellationToken);
    }

    private async Task<Result> AprovarAsync(AprovarOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var orcamento = await _orcamentoRepository.GetByIdAsync(request.OrcamentoId, cancellationToken);

        if (orcamento is null)
        {
            return Result.NotFound("Orcamento nao encontrado");
        }

        if (orcamento.Status != StatusOrcamento.Enviado)
        {
            return Result.Failure("Orcamento deve estar Enviado para ser aprovado");
        }

        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(orcamento.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.NotFound("Ordem de Servico nao encontrada");
        }

        if (ordemDeServico.Status != StatusOS.AguardandoAprovacao)
        {
            return Result.Failure("OS deve estar Aguardando Aprovacao para iniciar execucao");
        }

        var itensExecucao = orcamento.Itens.Where(item => item.Tipo == TipoItem.Servico && item.ServicoId.HasValue).Select(item => new ItemOS(item.ServicoId!.Value, null, item.Quantidade)).ToList();

        if (itensExecucao.Count == 0)
        {
            return Result.Failure("Orcamento aprovado deve possuir pelo menos um servico");
        }

        var quantidadesPorPeca = orcamento.Itens.Where(item => item.Tipo == TipoItem.Peca && item.PecaId.HasValue).GroupBy(item => item.PecaId!.Value).ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => item.Quantidade));
        var pecasResult = await ObterPecasAsync(quantidadesPorPeca, cancellationToken);

        if (pecasResult.IsFailure)
        {
            return Result.Failure(pecasResult.Error, pecasResult.ErrorType);
        }

        var reservaResult = ReservarPecas(pecasResult.Value, ordemDeServico.Id);

        if (reservaResult.IsFailure)
        {
            return reservaResult;
        }

        var preparacaoResult = ordemDeServico.PrepararItensExecucao(itensExecucao);

        if (preparacaoResult.IsFailure)
        {
            return preparacaoResult;
        }

        var aprovacaoResult = orcamento.Aprovar();

        if (aprovacaoResult.IsFailure)
        {
            return aprovacaoResult;
        }

        var execucaoResult = ordemDeServico.IniciarExecucao();

        if (execucaoResult.IsFailure)
        {
            return execucaoResult;
        }

        _orcamentoRepository.Update(orcamento);
        _ordemDeServicoRepository.Update(ordemDeServico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<List<(Peca Peca, int Quantidade)>>> ObterPecasAsync(IReadOnlyDictionary<Guid, int> quantidadesPorPeca, CancellationToken cancellationToken)
    {
        var pecas = new List<(Peca Peca, int Quantidade)>();

        foreach (var quantidadePorPeca in quantidadesPorPeca)
        {
            var peca = await _pecaRepository.GetByIdAsync(quantidadePorPeca.Key, cancellationToken);

            if (peca is null)
            {
                return Result.NotFound<List<(Peca, int)>>($"Peca {quantidadePorPeca.Key} nao encontrada");
            }

            if (peca.QuantidadeDisponivel < quantidadePorPeca.Value)
            {
                return Result.Conflict<List<(Peca, int)>>($"Estoque insuficiente para {peca.Nome}. Disponivel: {peca.QuantidadeDisponivel}, Solicitado: {quantidadePorPeca.Value}");
            }

            pecas.Add((peca, quantidadePorPeca.Value));
        }

        return Result.Success(pecas);
    }

    private Result ReservarPecas(IEnumerable<(Peca Peca, int Quantidade)> pecas, Guid ordemDeServicoId)
    {
        foreach (var item in pecas)
        {
            var reservaResult = item.Peca.Reservar(ordemDeServicoId, item.Quantidade);

            if (reservaResult.IsFailure)
            {
                return Result.Failure(reservaResult.Error, reservaResult.ErrorType);
            }

            _pecaRepository.Update(item.Peca);
        }

        return Result.Success();
    }
}
