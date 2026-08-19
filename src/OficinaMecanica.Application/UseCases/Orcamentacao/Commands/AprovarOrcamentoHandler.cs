using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Entities;
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
            return Result.Failure("Orcamento nao encontrado");
        }

        if (orcamento.Status != StatusOrcamento.Enviado)
        {
            return Result.Failure("Orcamento deve estar Enviado para ser aprovado");
        }

        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(orcamento.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.Failure("Ordem de Servico nao encontrada");
        }

        if (ordemDeServico.Status != StatusOS.AguardandoAprovacao)
        {
            return Result.Failure("OS deve estar Aguardando Aprovacao para iniciar execucao");
        }

        var quantidadesPorPeca = orcamento.Itens.Where(item => item.Tipo == TipoItem.Peca && item.PecaId.HasValue).GroupBy(item => item.PecaId!.Value).ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => item.Quantidade));
        var pecas = new List<(Peca Peca, int Quantidade)>();

        foreach (var quantidadePorPeca in quantidadesPorPeca)
        {
            var peca = await _pecaRepository.GetByIdAsync(quantidadePorPeca.Key, cancellationToken);

            if (peca is null)
            {
                return Result.Failure($"Peca {quantidadePorPeca.Key} nao encontrada");
            }

            if (peca.QuantidadeDisponivel < quantidadePorPeca.Value)
            {
                return Result.Failure($"Estoque insuficiente para {peca.Nome}. Disponivel: {peca.QuantidadeDisponivel}, Solicitado: {quantidadePorPeca.Value}");
            }

            pecas.Add((peca, quantidadePorPeca.Value));
        }

        foreach (var item in pecas)
        {
            var reservaResult = item.Peca.Reservar(ordemDeServico.Id, item.Quantidade);

            if (reservaResult.IsFailure)
            {
                return Result.Failure(reservaResult.Error);
            }

            _pecaRepository.Update(item.Peca);
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
}
