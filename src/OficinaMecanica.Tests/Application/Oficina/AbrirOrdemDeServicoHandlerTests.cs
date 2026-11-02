using Moq;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Application.UseCases.Oficina.Commands;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.CatalogoServicos.Entities;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Oficina.Entities;

namespace OficinaMecanica.Tests.Application.Oficina;

public sealed class AbrirOrdemDeServicoHandlerTests
{
    [Fact]
    public async Task Handle_ComClienteEVeiculoNovos_DeveCriarTudoEmUmaTransacaoEUmSaveChanges()
    {
        var scenario = CriarScenario();
        Cliente? clienteAdicionado = null;
        Veiculo? veiculoAdicionado = null;
        OrdemDeServico? ordemAdicionada = null;
        scenario.ClienteRepository.Setup(repository => repository.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>())).Callback<Cliente, CancellationToken>((entity, _) => clienteAdicionado = entity).Returns(Task.CompletedTask);
        scenario.VeiculoRepository.Setup(repository => repository.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>())).Callback<Veiculo, CancellationToken>((entity, _) => veiculoAdicionado = entity).Returns(Task.CompletedTask);
        scenario.OrdemDeServicoRepository.Setup(repository => repository.AddAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>())).Callback<OrdemDeServico, CancellationToken>((entity, _) => ordemAdicionada = entity).Returns(Task.CompletedTask);

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("OS-0001", result.Value.Numero);
        Assert.NotNull(clienteAdicionado);
        Assert.NotNull(veiculoAdicionado);
        Assert.NotNull(ordemAdicionada);
        Assert.Equal(clienteAdicionado.Id, veiculoAdicionado.ClienteId);
        Assert.Equal(clienteAdicionado.Id, ordemAdicionada.ClienteId);
        Assert.Equal(veiculoAdicionado.Id, ordemAdicionada.VeiculoId);
        Assert.Equal(result.Value.Id, ordemAdicionada.Id);
        Assert.Collection(ordemAdicionada.ServicosSolicitados, item => Assert.Equal(scenario.Servico.Id, item.ServicoId));
        Assert.Collection(ordemAdicionada.PecasSolicitadas, item => Assert.Equal(scenario.Peca.Id, item.PecaId));
        Assert.Equal(1, scenario.UnitOfWork.TransactionCount);
        Assert.Equal(1, scenario.UnitOfWork.SaveChangesCount);
        scenario.ClienteRepository.Verify(repository => repository.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        scenario.VeiculoRepository.Verify(repository => repository.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Once);
        scenario.OrdemDeServicoRepository.Verify(repository => repository.AddAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComClienteEVeiculoExistentes_DeveReutilizarCadastros()
    {
        var scenario = CriarScenario();
        var cliente = CriarClienteExistente();
        var veiculo = CriarVeiculoExistente(cliente.Id);
        OrdemDeServico? ordemAdicionada = null;
        scenario.ClienteRepository.Setup(repository => repository.GetByCpfAsync(It.IsAny<Cpf>(), It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        scenario.VeiculoRepository.Setup(repository => repository.GetByPlacaAsync(It.IsAny<Placa>(), It.IsAny<CancellationToken>())).ReturnsAsync(veiculo);
        scenario.OrdemDeServicoRepository.Setup(repository => repository.AddAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>())).Callback<OrdemDeServico, CancellationToken>((entity, _) => ordemAdicionada = entity).Returns(Task.CompletedTask);

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(ordemAdicionada);
        Assert.Equal(cliente.Id, ordemAdicionada.ClienteId);
        Assert.Equal(veiculo.Id, ordemAdicionada.VeiculoId);
        scenario.ClienteRepository.Verify(repository => repository.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
        scenario.VeiculoRepository.Verify(repository => repository.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
        scenario.OrdemDeServicoRepository.Verify(repository => repository.AddAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(1, scenario.UnitOfWork.TransactionCount);
        Assert.Equal(1, scenario.UnitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task Handle_QuandoPlacaPertenceAOutroCliente_DeveRetornarConflitoSemPersistir()
    {
        var scenario = CriarScenario();
        var cliente = CriarClienteExistente();
        var veiculoDeOutroCliente = CriarVeiculoExistente(Guid.NewGuid());
        scenario.ClienteRepository.Setup(repository => repository.GetByCpfAsync(It.IsAny<Cpf>(), It.IsAny<CancellationToken>())).ReturnsAsync(cliente);
        scenario.VeiculoRepository.Setup(repository => repository.GetByPlacaAsync(It.IsAny<Placa>(), It.IsAny<CancellationToken>())).ReturnsAsync(veiculoDeOutroCliente);

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(1, scenario.UnitOfWork.TransactionCount);
        Assert.Equal(0, scenario.UnitOfWork.SaveChangesCount);
        VerificarQueNadaFoiAdicionado(scenario);
        scenario.ServicoRepository.Verify(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        scenario.PecaRepository.Verify(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoDadosDoClienteExistenteDivergem_DeveRetornarConflitoSemPersistir()
    {
        var scenario = CriarScenario();
        var cliente = new Cliente("Outro Nome", Cpf.Criar("52998224725").Value, "11999999999", Email.Criar("cliente@teste.com").Value);
        scenario.ClienteRepository.Setup(repository => repository.GetByCpfAsync(It.IsAny<Cpf>(), It.IsAny<CancellationToken>())).ReturnsAsync(cliente);

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(0, scenario.UnitOfWork.SaveChangesCount);
        VerificarQueNadaFoiAdicionado(scenario);
        scenario.VeiculoRepository.Verify(repository => repository.GetByPlacaAsync(It.IsAny<Placa>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoServicoNaoExiste_DeveRetornarNotFoundSemPersistir()
    {
        var scenario = CriarScenario();
        scenario.ServicoRepository.Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal(1, scenario.UnitOfWork.TransactionCount);
        Assert.Equal(0, scenario.UnitOfWork.SaveChangesCount);
        VerificarQueNadaFoiAdicionado(scenario);
    }

    [Fact]
    public async Task Handle_QuandoServicoEstaInativo_DeveRetornarConflitoSemPersistir()
    {
        var scenario = CriarScenario();
        scenario.Servico.Desativar();

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal(1, scenario.UnitOfWork.TransactionCount);
        Assert.Equal(0, scenario.UnitOfWork.SaveChangesCount);
        VerificarQueNadaFoiAdicionado(scenario);
    }

    [Fact]
    public async Task Handle_QuandoPecaNaoExiste_DeveRetornarNotFoundSemPersistir()
    {
        var scenario = CriarScenario();
        scenario.PecaRepository.Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await scenario.Handler.Handle(scenario.Command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal(1, scenario.UnitOfWork.TransactionCount);
        Assert.Equal(0, scenario.UnitOfWork.SaveChangesCount);
        VerificarQueNadaFoiAdicionado(scenario);
    }

    private static HandlerScenario CriarScenario()
    {
        var ordemDeServicoRepository = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        var clienteRepository = new Mock<IClienteRepository>(MockBehavior.Strict);
        var veiculoRepository = new Mock<IVeiculoRepository>(MockBehavior.Strict);
        var servicoRepository = new Mock<IServicoRepository>(MockBehavior.Strict);
        var pecaRepository = new Mock<IPecaRepository>(MockBehavior.Strict);
        var unitOfWork = new UnitOfWorkSpy();
        var servico = new Servico("Troca de oleo", "Troca de oleo do motor", 150m, 60);
        var peca = new Peca("Filtro de oleo", "FLT-001", "Filtro", 40m, 10, 2);
        var command = new AbrirOrdemDeServicoCommand(new DadosClienteAbertura("Cliente Teste", "529.982.247-25", "11999999999", "cliente@teste.com"), new DadosVeiculoAbertura("ABC1D23", "Honda", "Civic", 2022), [new ServicoSolicitadoRequest(servico.Id, 1, "Verificar motor")], [new PecaSolicitadaRequest(peca.Id, 2, "Usar peca original")]);
        clienteRepository.Setup(repository => repository.GetByCpfAsync(It.IsAny<Cpf>(), It.IsAny<CancellationToken>())).ReturnsAsync((Cliente?)null);
        clienteRepository.Setup(repository => repository.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        veiculoRepository.Setup(repository => repository.GetByPlacaAsync(It.IsAny<Placa>(), It.IsAny<CancellationToken>())).ReturnsAsync((Veiculo?)null);
        veiculoRepository.Setup(repository => repository.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        servicoRepository.Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([servico]);
        pecaRepository.Setup(repository => repository.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([peca]);
        ordemDeServicoRepository.Setup(repository => repository.GerarProximoNumeroAsync(It.IsAny<CancellationToken>())).ReturnsAsync("OS-0001");
        ordemDeServicoRepository.Setup(repository => repository.AddAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new AbrirOrdemDeServicoHandler(ordemDeServicoRepository.Object, clienteRepository.Object, veiculoRepository.Object, servicoRepository.Object, pecaRepository.Object, unitOfWork);
        return new HandlerScenario(handler, command, servico, peca, ordemDeServicoRepository, clienteRepository, veiculoRepository, servicoRepository, pecaRepository, unitOfWork);
    }

    private static Cliente CriarClienteExistente() => new("Cliente Teste", Cpf.Criar("52998224725").Value, "11999999999", Email.Criar("cliente@teste.com").Value);
    private static Veiculo CriarVeiculoExistente(Guid clienteId) => new(Placa.Criar("ABC1D23").Value, "Honda", "Civic", 2022, clienteId);

    private static void VerificarQueNadaFoiAdicionado(HandlerScenario scenario)
    {
        scenario.ClienteRepository.Verify(repository => repository.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Never);
        scenario.VeiculoRepository.Verify(repository => repository.AddAsync(It.IsAny<Veiculo>(), It.IsAny<CancellationToken>()), Times.Never);
        scenario.OrdemDeServicoRepository.Verify(repository => repository.AddAsync(It.IsAny<OrdemDeServico>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed record HandlerScenario(AbrirOrdemDeServicoHandler Handler, AbrirOrdemDeServicoCommand Command, Servico Servico, Peca Peca, Mock<IOrdemDeServicoRepository> OrdemDeServicoRepository, Mock<IClienteRepository> ClienteRepository, Mock<IVeiculoRepository> VeiculoRepository, Mock<IServicoRepository> ServicoRepository, Mock<IPecaRepository> PecaRepository, UnitOfWorkSpy UnitOfWork);

    private sealed class UnitOfWorkSpy : IUnitOfWork
    {
        public int SaveChangesCount { get; private set; }
        public int TransactionCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCount++;
            return Task.FromResult(1);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            TransactionCount++;
            return await operation(cancellationToken);
        }
    }
}
