using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.Tests.Domain.Atendimento;

public sealed class ClienteTests
{
    private static Cpf CpfValido() => Cpf.Criar("529.982.247-25").Value;
    private static Email EmailValido() => Email.Criar("teste@oficina.com").Value;

    [Fact]
    public void Criar_ComDadosValidos_DeveCriarCliente()
    {
        var cliente = new Cliente("Joao Silva", CpfValido(), "11999998888", EmailValido());

        Assert.Equal("Joao Silva", cliente.Nome);
        Assert.Equal("52998224725", cliente.Cpf.Numero);
        Assert.Equal("11999998888", cliente.Telefone);
        Assert.Equal("teste@oficina.com", cliente.Email.Endereco);
        Assert.NotEqual(Guid.Empty, cliente.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Criar_ComNomeInvalido_DeveLancarExcecao(string? nome)
    {
        Assert.Throws<ArgumentException>(() =>
            new Cliente(nome!, CpfValido(), "11999998888", EmailValido()));
    }

    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarCampos()
    {
        var cliente = new Cliente("Joao Silva", CpfValido(), "11999998888", EmailValido());
        var novoEmail = Email.Criar("novo@oficina.com").Value;

        cliente.Atualizar("Joao Souza", "11888887777", novoEmail);

        Assert.Equal("Joao Souza", cliente.Nome);
        Assert.Equal("11888887777", cliente.Telefone);
        Assert.Equal("novo@oficina.com", cliente.Email.Endereco);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_ComNomeInvalido_DeveLancarExcecao(string nome)
    {
        var cliente = new Cliente("Joao Silva", CpfValido(), "11999998888", EmailValido());

        Assert.Throws<ArgumentException>(() =>
            cliente.Atualizar(nome, "11888887777", EmailValido()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Atualizar_ComTelefoneInvalido_DeveLancarExcecao(string telefone)
    {
        var cliente = new Cliente("Joao Silva", CpfValido(), "11999998888", EmailValido());

        Assert.Throws<ArgumentException>(() =>
            cliente.Atualizar("Joao Souza", telefone, EmailValido()));
    }
}
