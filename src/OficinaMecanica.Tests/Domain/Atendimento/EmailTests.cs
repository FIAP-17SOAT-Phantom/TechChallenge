using OficinaMecanica.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.Tests.Domain.Atendimento;

public sealed class EmailTests
{
    [Fact]
    public void Criar_ComEmailValido_DeveNormalizar()
    {
        var result = Email.Criar("  CLIENTE@EXEMPLO.COM  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("cliente@exemplo.com", result.Value.Endereco);
    }

    [Theory]
    [InlineData("")]
    [InlineData("email-invalido")]
    [InlineData("cliente@")]
    public void Criar_ComEmailInvalido_DeveFalhar(string valor)
    {
        var result = Email.Criar(valor);

        Assert.True(result.IsFailure);
    }
}
