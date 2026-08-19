using OficinaMecanica.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.Tests.Domain.Atendimento;

public sealed class CpfTests
{
    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    public void Criar_ComCpfValido_DeveNormalizar(string valor, string esperado)
    {
        var result = Cpf.Criar(valor);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value.Numero);
    }

    [Theory]
    [InlineData("")]
    [InlineData("11111111111")]
    [InlineData("52998224724")]
    public void Criar_ComCpfInvalido_DeveFalhar(string valor)
    {
        var result = Cpf.Criar(valor);

        Assert.True(result.IsFailure);
    }
}
