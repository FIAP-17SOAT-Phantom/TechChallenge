using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Application.UseCases.Orcamentacao.Commands;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Oficina.Entities;
using OficinaMecanica.Domain.Oficina.Enums;
using OficinaMecanica.Domain.Orcamentacao.Entities;
using OficinaMecanica.Domain.Orcamentacao.Enums;
using OficinaMecanica.Domain.Orcamentacao.ValueObjects;

namespace OficinaMecanica.Tests.Application.Orcamentacao;

public sealed class AprovarOrcamentoHandlerTests
{
    [Fact]
    public async Task Handle_ComEstoqueDisponivel_DeveReservarAprovarEIniciarExecucao()
    {
        var scenario = CriarScenario(10, 4);

        var result = await scenario.Handler.Handle(new AprovarOrcamentoCommand(scenario.Orcamento.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusOrcamento.Aprovado, scenario.Orcamento.Status);
        Assert.Equal(StatusOS.EmExecucao, scenario.OrdemDeServico.Status);
        Assert.Equal(4, scenario.Peca.QuantidadeReservada);
        Assert.Equal(1, scenario.UnitOfWork.SaveCount);
        Assert.Equal(1, scenario.UnitOfWork.TransactionCount);
    }

    [Fact]
    public async Task Handle_ComEstoqueInsuficiente_NaoDeveAprovarNemSalvar()
    {
        var scenario = CriarScenario(3, 4);

        var result = await scenario.Handler.Handle(new AprovarOrcamentoCommand(scenario.Orcamento.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(StatusOrcamento.Enviado, scenario.Orcamento.Status);
        Assert.Equal(StatusOS.AguardandoAprovacao, scenario.OrdemDeServico.Status);
        Assert.Equal(0, scenario.Peca.QuantidadeReservada);
        Assert.Equal(0, scenario.UnitOfWork.SaveCount);
        Assert.Equal(1, scenario.UnitOfWork.TransactionCount);
    }

    [Fact]
    public async Task Handle_ComItensRepetidos_DeveValidarQuantidadeAgrupada()
    {
        var scenario = CriarScenario(6, 4, 4);

        var result = await scenario.Handler.Handle(new AprovarOrcamentoCommand(scenario.Orcamento.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(0, scenario.Peca.QuantidadeReservada);
        Assert.Equal(StatusOrcamento.Enviado, scenario.Orcamento.Status);
    }

    private static ApprovalScenario CriarScenario(int estoque, params int[] quantidades)
    {
        var peca = new Peca("Filtro", "FLT-001", "Filtro", 50m, estoque, 1);
        var ordemDeServico = new OrdemDeServico("OS-0001", Guid.NewGuid(), Guid.NewGuid());
        ordemDeServico.IniciarDiagnostico(Guid.NewGuid());
        ordemDeServico.RegistrarDiagnostico("Diagnostico", [new ItemOS(Guid.NewGuid(), peca.Id, 1)]);
        var itens = new List<ItemOrcamento> { new("Troca de filtro", TipoItem.Servico, 1, 100m, servicoId: Guid.NewGuid()) };
        itens.AddRange(quantidades.Select(quantidade => new ItemOrcamento(peca.Nome, TipoItem.Peca, quantidade, peca.PrecoUnitario, pecaId: peca.Id)));
        var orcamento = new Orcamento(ordemDeServico.Id, 1, itens);
        ordemDeServico.VincularOrcamento(orcamento.Id);
        orcamento.Enviar();
        var orcamentoRepository = new OrcamentoRepositoryFake(orcamento);
        var ordemDeServicoRepository = new OrdemDeServicoRepositoryFake(ordemDeServico);
        var pecaRepository = new PecaRepositoryFake(peca);
        var unitOfWork = new UnitOfWorkFake();
        var handler = new AprovarOrcamentoHandler(orcamentoRepository, ordemDeServicoRepository, pecaRepository, unitOfWork);
        return new ApprovalScenario(handler, orcamento, ordemDeServico, peca, unitOfWork);
    }

    private sealed record ApprovalScenario(AprovarOrcamentoHandler Handler, Orcamento Orcamento, OrdemDeServico OrdemDeServico, Peca Peca, UnitOfWorkFake UnitOfWork);

    private sealed class UnitOfWorkFake : IUnitOfWork
    {
        public int SaveCount { get; private set; }
        public int TransactionCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            TransactionCount++;
            return await operation(cancellationToken);
        }
    }

    private sealed class OrcamentoRepositoryFake : IOrcamentoRepository
    {
        private readonly Orcamento _orcamento;

        public OrcamentoRepositoryFake(Orcamento orcamento)
        {
            _orcamento = orcamento;
        }

        public Task<Orcamento?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Orcamento?>(_orcamento.Id == id ? _orcamento : null);
        public Task<Orcamento?> GetByOrdemDeServicoIdAsync(Guid ordemDeServicoId, CancellationToken cancellationToken = default) => Task.FromResult<Orcamento?>(_orcamento.OrdemDeServicoId == ordemDeServicoId ? _orcamento : null);
        public Task<int> GetVersaoAtualAsync(Guid ordemDeServicoId, CancellationToken cancellationToken = default) => Task.FromResult(_orcamento.Versao);
        public Task<IReadOnlyList<Orcamento>> GetPagedByClienteIdAsync(Guid clienteId, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Orcamento>>([]);
        public Task AddAsync(Orcamento entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(Orcamento entity) { }
        public void Remove(Orcamento entity) { }
    }

    private sealed class OrdemDeServicoRepositoryFake : IOrdemDeServicoRepository
    {
        private readonly OrdemDeServico _ordemDeServico;

        public OrdemDeServicoRepositoryFake(OrdemDeServico ordemDeServico)
        {
            _ordemDeServico = ordemDeServico;
        }

        public Task<OrdemDeServico?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<OrdemDeServico?>(_ordemDeServico.Id == id ? _ordemDeServico : null);
        public Task<OrdemDeServico?> GetByNumeroAsync(string numero, CancellationToken cancellationToken = default) => Task.FromResult<OrdemDeServico?>(_ordemDeServico.Numero == numero ? _ordemDeServico : null);
        public Task<IReadOnlyList<OrdemDeServico>> GetByClienteIdAsync(Guid clienteId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrdemDeServico>>([]);
        public Task<IReadOnlyList<OrdemDeServico>> GetPagedByClienteIdAsync(Guid clienteId, StatusOS? status, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrdemDeServico>>([]);
        public Task<IReadOnlyList<OrdemDeServico>> GetByStatusAsync(StatusOS status, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrdemDeServico>>([]);
        public Task<IReadOnlyList<OrdemDeServico>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<OrdemDeServico>>([]);
        public Task<bool> ExistsByVeiculoIdAsync(Guid veiculoId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<TimeSpan?> GetTempoMedioExecucaoAsync(CancellationToken cancellationToken = default) => Task.FromResult<TimeSpan?>(null);
        public Task<string> GerarProximoNumeroAsync(CancellationToken cancellationToken = default) => Task.FromResult("OS-0002");
        public Task AddAsync(OrdemDeServico entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(OrdemDeServico entity) { }
        public void Remove(OrdemDeServico entity) { }
    }

    private sealed class PecaRepositoryFake : IPecaRepository
    {
        private readonly Peca _peca;

        public PecaRepositoryFake(Peca peca)
        {
            _peca = peca;
        }

        public Task<Peca?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Peca?>(_peca.Id == id ? _peca : null);
        public Task<Peca?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default) => Task.FromResult<Peca?>(_peca.Codigo == codigo ? _peca : null);
        public Task<IReadOnlyList<Peca>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Peca>>([_peca]);
        public Task<IReadOnlyList<Peca>> GetComEstoqueBaixoAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Peca>>([]);
        public Task<IReadOnlyList<Peca>> GetPagedAsync(bool somenteEstoqueBaixo, int page, int pageSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Peca>>([_peca]);
        public Task<bool> HasReferencesAsync(Guid pecaId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task AddAsync(Peca entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(Peca entity) { }
        public void Remove(Peca entity) { }
    }
}
