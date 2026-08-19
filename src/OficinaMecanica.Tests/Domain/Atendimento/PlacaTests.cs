using OficinaMecanica.Domain.Atendimento.ValueObjects;

namespace OficinaMecanica.Tests.Domain.Atendimento;

public sealed class PlacaTests
{
    [Theory]
    [InlineData("abc-1234", "ABC1234")]
    [InlineData("abc1d23", "ABC1D23")]
    public void Criar_ComPlacaValida_DeveNormalizar(string valor, string esperado)
    {
        var result = Placa.Criar(valor);

        Assert.True(result.IsSuccess);
        Assert.Equal(esperado, result.Value.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB-1234")]
    [InlineData("ABC123")]
    public void Criar_ComPlacaInvalida_DeveFalhar(string valor)
    {
        var result = Placa.Criar(valor);

        Assert.True(result.IsFailure);
    }
}
