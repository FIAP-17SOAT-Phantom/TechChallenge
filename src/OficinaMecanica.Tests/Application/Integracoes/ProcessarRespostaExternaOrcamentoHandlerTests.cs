using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Application.UseCases.Integracoes.Commands;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Entities;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Tests.Application.Integracoes;

public sealed class ProcessarRespostaExternaOrcamentoHandlerTests
{
    [Fact]
    public async Task Handle_ComEventoNovo_DeveProcessarPersistirERetornarNaoRepetido()
    {
        var scenario = CriarScenario();

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.Repetido);
        Assert.Equal(1, scenario.Processador.CallCount);
        Assert.Equal(1, scenario.Repository.TryAddCount);
        Assert.Equal(1, scenario.UnitOfWork.SaveCount);
        Assert.Equal(1, scenario.UnitOfWork.TransactionCount);
    }

    [Fact]
    public async Task Handle_ComRepeticaoExata_DeveSerIdempotenteSemReprocessar()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var orcamentoId = Guid.NewGuid();
        var existingEvent = new EventoRespostaOrcamento("evento-1", orcamentoId, RespostaOrcamentoExterna.Aprovado, timestamp);
        var scenario = CriarScenario(existingEvent, orcamentoId, timestamp);

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Repetido);
        Assert.Equal(0, scenario.Processador.CallCount);
        Assert.Equal(0, scenario.Repository.TryAddCount);
        Assert.Equal(0, scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ComExternalEventIdReutilizadoParaOutroConteudo_DeveRetornarConflict()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var orcamentoId = Guid.NewGuid();
        var existingEvent = new EventoRespostaOrcamento("evento-1", orcamentoId, RespostaOrcamentoExterna.Rejeitado, timestamp);
        var scenario = CriarScenario(existingEvent, orcamentoId, timestamp);

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(0, scenario.Processador.CallCount);
        Assert.Equal(0, scenario.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Handle_ComAssinaturaInvalida_DeveRetornarUnauthorizedSemAbrirTransacao()
    {
        var scenario = CriarScenario(authenticationResult: Result.Unauthorized("Assinatura invalida"));

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Equal(0, scenario.Processador.CallCount);
        Assert.Equal(0, scenario.UnitOfWork.TransactionCount);
    }

    [Fact]
    public async Task Handle_QuandoInsercaoConcorrenteConfirmaMesmoEvento_DeveRetornarRepetidoSemSalvarMutacoesLocais()
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var orcamentoId = Guid.NewGuid();
        var concurrentEvent = new EventoRespostaOrcamento("evento-1", orcamentoId, RespostaOrcamentoExterna.Aprovado, timestamp);
        var scenario = CriarScenario(orcamentoId: orcamentoId, timestamp: timestamp);
        scenario.Repository.TryAddResult = false;
        scenario.Repository.EventAfterTryAdd = concurrentEvent;

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Repetido);
        Assert.Equal(1, scenario.Processador.CallCount);
        Assert.Equal(0, scenario.UnitOfWork.SaveCount);
    }

    private static HandlerScenario CriarScenario(EventoRespostaOrcamento? existingEvent = null, Guid? orcamentoId = null, long? timestamp = null, Result? authenticationResult = null)
    {
        var command = new ProcessarRespostaExternaOrcamentoCommand(orcamentoId ?? Guid.NewGuid(), "evento-1", "Aprovado", timestamp ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds(), "assinatura");
        var authenticator = new AuthenticatorFake(authenticationResult ?? Result.Success());
        var repository = new EventoRepositoryFake(existingEvent);
        var processador = new ProcessadorFake();
        var unitOfWork = new UnitOfWorkFake();
        var handler = new ProcessarRespostaExternaOrcamentoHandler(authenticator, repository, processador, unitOfWork);
        return new HandlerScenario(handler, command, repository, processador, unitOfWork);
    }

    private sealed record HandlerScenario(ProcessarRespostaExternaOrcamentoHandler Handler, ProcessarRespostaExternaOrcamentoCommand Command, EventoRepositoryFake Repository, ProcessadorFake Processador, UnitOfWorkFake UnitOfWork);

    private sealed class AuthenticatorFake : IWebhookOrcamentoAuthenticator
    {
        private readonly Result _result;

        public AuthenticatorFake(Result result)
        {
            _result = result;
        }

        public Result Validate(string externalEventId, long timestampUnix, Guid orcamentoId, RespostaOrcamentoExterna resposta, string? signature) => _result;
    }

    private sealed class EventoRepositoryFake : IEventoRespostaOrcamentoRepository
    {
        private readonly EventoRespostaOrcamento? _existingEvent;

        public EventoRepositoryFake(EventoRespostaOrcamento? existingEvent)
        {
            _existingEvent = existingEvent;
        }

        public bool TryAddResult { get; set; } = true;
        public EventoRespostaOrcamento? EventAfterTryAdd { get; set; }
        public int TryAddCount { get; private set; }

        public Task AcquireOrcamentoLockAsync(Guid orcamentoId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<EventoRespostaOrcamento?> GetByExternalEventIdAsync(string externalEventId, CancellationToken cancellationToken = default) => Task.FromResult(TryAddCount > 0 && EventAfterTryAdd is not null ? EventAfterTryAdd : _existingEvent);

        public Task<bool> TryAddAsync(EventoRespostaOrcamento evento, CancellationToken cancellationToken = default)
        {
            TryAddCount++;
            return Task.FromResult(TryAddResult);
        }
    }

    private sealed class ProcessadorFake : IProcessadorRespostaOrcamento
    {
        public int CallCount { get; private set; }
        public Result Result { get; set; } = global::OficinaMecanica.Domain.Common.Result.Success();

        public Task<Result> ProcessAsync(Guid orcamentoId, RespostaOrcamentoExterna resposta, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(Result);
        }
    }

    private sealed class UnitOfWorkFake : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public int TransactionCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }

        public Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default) => ExecuteInReadCommittedTransactionAsync(operation, cancellationToken);

        public async Task<T> ExecuteInReadCommittedTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            TransactionCount++;
            return await operation(cancellationToken);
        }
    }
}
