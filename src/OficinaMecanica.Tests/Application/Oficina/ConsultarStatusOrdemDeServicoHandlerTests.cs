using Moq;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Application.UseCases.Oficina.Queries;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Oficina.Entities;

namespace OficinaMecanica.Tests.Application.Oficina;

public sealed class ConsultarStatusOrdemDeServicoHandlerTests
{
    [Fact]
    public async Task Handle_QuandoOrdemExiste_DeveRetornarStatusEDatas()
    {
        var ordemDeServico = new OrdemDeServico("OS-0001", Guid.NewGuid(), Guid.NewGuid());
        var repository = CriarRepository(ordemDeServico);
        var handler = new ConsultarStatusOrdemDeServicoHandler(repository.Object);

        var result = await handler.Handle(new ConsultarStatusOrdemDeServicoQuery(ordemDeServico.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ordemDeServico.Id, result.Value.Id);
        Assert.Equal("OS-0001", result.Value.Numero);
        Assert.Equal("Recebida", result.Value.Status);
        Assert.Equal(ordemDeServico.DataUltimaAtualizacao, result.Value.DataUltimaAtualizacao);
    }

    [Fact]
    public async Task Handle_QuandoOrdemNaoExiste_DeveRetornarNotFound()
    {
        var repository = CriarRepository(null);
        var handler = new ConsultarStatusOrdemDeServicoHandler(repository.Object);

        var result = await handler.Handle(new ConsultarStatusOrdemDeServicoQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public async Task Handle_QuandoOrdemPertenceAOutroCliente_DeveRetornarForbidden()
    {
        var ordemDeServico = new OrdemDeServico("OS-0001", Guid.NewGuid(), Guid.NewGuid());
        var repository = CriarRepository(ordemDeServico);
        var handler = new ConsultarStatusOrdemDeServicoHandler(repository.Object);

        var result = await handler.Handle(new ConsultarStatusOrdemDeServicoQuery(ordemDeServico.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.ErrorType);
    }

    private static Mock<IOrdemDeServicoRepository> CriarRepository(OrdemDeServico? ordemDeServico)
    {
        var repository = new Mock<IOrdemDeServicoRepository>(MockBehavior.Strict);
        repository.Setup(item => item.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(ordemDeServico);
        return repository;
    }
}
