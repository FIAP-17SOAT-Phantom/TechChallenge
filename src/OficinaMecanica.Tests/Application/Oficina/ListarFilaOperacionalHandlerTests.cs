using Moq;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Application.UseCases.Oficina.Queries;
using OficinaMecanica.Domain.Oficina.Entities;

namespace OficinaMecanica.Tests.Application.Oficina;

public sealed class ListarFilaOperacionalHandlerTests
{
    [Fact]
    public async Task Handle_DevePreservarOrdenacaoDoRepositorioEMapearCampos()
    {
        var repositorio = new Mock<IOrdemDeServicoRepository>();
        var emExecucao = CriarOrdem("OS-0002");
        var recebida = CriarOrdem("OS-0001");
        repositorio.Setup(repository => repository.GetFilaOperacionalAsync(2, 10, It.IsAny<CancellationToken>())).ReturnsAsync([emExecucao, recebida]);
        var handler = new ListarFilaOperacionalHandler(repositorio.Object);

        var resultado = await handler.Handle(new ListarFilaOperacionalQuery(2, 10), CancellationToken.None);

        Assert.Collection(resultado, primeira => Assert.Equal("OS-0002", primeira.Numero), segunda => Assert.Equal("OS-0001", segunda.Numero));
        Assert.Equal(emExecucao.Id, resultado[0].Id);
        Assert.Equal(emExecucao.Status.ToString(), resultado[0].Status);
        repositorio.Verify(repository => repository.GetFilaOperacionalAsync(2, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_DeveRejeitarPaginacaoInvalida()
    {
        var validator = new ListarFilaOperacionalValidator();

        var resultado = validator.Validate(new ListarFilaOperacionalQuery(0, 101));

        Assert.False(resultado.IsValid);
        Assert.Equal(2, resultado.Errors.Count);
    }

    private static OrdemDeServico CriarOrdem(string numero) => new(numero, Guid.NewGuid(), Guid.NewGuid());
}
