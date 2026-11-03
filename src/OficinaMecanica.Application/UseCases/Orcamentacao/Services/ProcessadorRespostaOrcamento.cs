using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Oficina.Enums;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Services;

public sealed class ProcessadorRespostaOrcamento : IProcessadorRespostaOrcamento
{
    private readonly IOrcamentoRepository _orcamentoRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IPecaRepository _pecaRepository;

    public ProcessadorRespostaOrcamento(IOrcamentoRepository orcamentoRepository, IOrdemDeServicoRepository ordemDeServicoRepository, IPecaRepository pecaRepository)
    {
        _orcamentoRepository = orcamentoRepository;
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _pecaRepository = pecaRepository;
    }

    public Task<Result> ProcessAsync(Guid orcamentoId, RespostaOrcamentoExterna resposta, CancellationToken cancellationToken = default) => resposta == RespostaOrcamentoExterna.Aprovado ? AprovarAsync(orcamentoId, cancellationToken) : RejeitarAsync(orcamentoId, cancellationToken);

    private async Task<Result> AprovarAsync(Guid orcamentoId, CancellationToken cancellationToken)
    {
        var orcamento = await _orcamentoRepository.GetByIdAsync(orcamentoId, cancellationToken);

        if (orcamento is null)
            return Result.NotFound("Orcamento nao encontrado");

        if (orcamento.Status != StatusOrcamento.Enviado)
            return Result.Failure("Orcamento deve estar Enviado para ser aprovado");

        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(orcamento.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
            return Result.NotFound("Ordem de Servico nao encontrada");

        if (ordemDeServico.Status != StatusOS.AguardandoAprovacao)
            return Result.Failure("OS deve estar Aguardando Aprovacao para iniciar execucao");

        var itensExecucao = orcamento.Itens.Where(item => item.Tipo == TipoItem.Servico && item.ServicoId.HasValue).Select(item => new ItemOS(item.ServicoId!.Value, null, item.Quantidade)).ToList();

        if (itensExecucao.Count == 0)
            return Result.Failure("Orcamento aprovado deve possuir pelo menos um servico");

        var quantidadesPorPeca = orcamento.Itens.Where(item => item.Tipo == TipoItem.Peca && item.PecaId.HasValue).GroupBy(item => item.PecaId!.Value).ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => item.Quantidade));
        var pecasResult = await ObterPecasAsync(quantidadesPorPeca, cancellationToken);

        if (pecasResult.IsFailure)
            return Result.Failure(pecasResult.Error, pecasResult.ErrorType);

        var reservaResult = ReservarPecas(pecasResult.Value, ordemDeServico.Id);

        if (reservaResult.IsFailure)
            return reservaResult;

        var preparacaoResult = ordemDeServico.PrepararItensExecucao(itensExecucao);

        if (preparacaoResult.IsFailure)
            return preparacaoResult;

        var aprovacaoResult = orcamento.Aprovar();

        if (aprovacaoResult.IsFailure)
            return aprovacaoResult;

        var execucaoResult = ordemDeServico.IniciarExecucao();

        if (execucaoResult.IsFailure)
            return execucaoResult;

        // Sem Update() explicito: orcamento e OS foram carregados rastreados pelos repositorios,
        // entao o change tracker persiste as alteracoes de escalares. Um Update() explicito marca
        // tambem as owned collections (ItensOrcamento, ItensOrdemDeServico) como Modified, gerando
        // UPDATEs que afetam 0 linhas entre contextos e lancam DbUpdateConcurrencyException.
        return Result.Success();
    }

    private async Task<Result> RejeitarAsync(Guid orcamentoId, CancellationToken cancellationToken)
    {
        var orcamento = await _orcamentoRepository.GetByIdAsync(orcamentoId, cancellationToken);

        if (orcamento is null)
            return Result.NotFound("Orcamento nao encontrado");

        var result = orcamento.Rejeitar();

        if (result.IsFailure)
            return result;

        _orcamentoRepository.Update(orcamento);
        return Result.Success();
    }

    private async Task<Result<List<(Peca Peca, int Quantidade)>>> ObterPecasAsync(IReadOnlyDictionary<Guid, int> quantidadesPorPeca, CancellationToken cancellationToken)
    {
        var pecas = new List<(Peca Peca, int Quantidade)>();

        foreach (var quantidadePorPeca in quantidadesPorPeca)
        {
            var peca = await _pecaRepository.GetByIdAsync(quantidadePorPeca.Key, cancellationToken);

            if (peca is null)
                return Result.NotFound<List<(Peca, int)>>($"Peca {quantidadePorPeca.Key} nao encontrada");

            if (peca.QuantidadeDisponivel < quantidadePorPeca.Value)
                return Result.Conflict<List<(Peca, int)>>($"Estoque insuficiente para {peca.Nome}. Disponivel: {peca.QuantidadeDisponivel}, Solicitado: {quantidadePorPeca.Value}");

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
                return Result.Failure(reservaResult.Error, reservaResult.ErrorType);

            // Sem Update() explicito: a Peca foi carregada rastreada (GetByIdAsync com Include das
            // Reservas), entao o change tracker marca a nova Reserva como Added (INSERT). Um Update()
            // explicito marcaria a Reserva nova como Modified, gerando um UPDATE que afeta 0 linhas
            // (a reserva ainda nao existe no banco) e lanca DbUpdateConcurrencyException.
        }

        return Result.Success();
    }
}
