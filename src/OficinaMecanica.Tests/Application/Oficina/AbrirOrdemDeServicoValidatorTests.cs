using OficinaMecanica.Application.UseCases.Oficina.Commands;

namespace OficinaMecanica.Tests.Application.Oficina;

public sealed class AbrirOrdemDeServicoValidatorTests
{
    private readonly AbrirOrdemDeServicoValidator _validator = new();

    [Fact]
    public void Validate_ComPayloadValido_DeveSerValido()
    {
        var result = _validator.Validate(CriarCommandValido());

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ComListaDeServicosVazia_DeveSerInvalido()
    {
        var command = CriarCommandValido() with { Servicos = [] };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AbrirOrdemDeServicoCommand.Servicos) && error.ErrorMessage == "Informe pelo menos um servico");
    }

    [Fact]
    public void Validate_ComIdsDuplicados_DeveSerInvalido()
    {
        var servicoId = Guid.NewGuid();
        var pecaId = Guid.NewGuid();
        var command = CriarCommandValido() with
        {
            Servicos = [new ServicoSolicitadoRequest(servicoId, 1, null), new ServicoSolicitadoRequest(servicoId, 2, null)],
            Pecas = [new PecaSolicitadaRequest(pecaId, 1, null), new PecaSolicitadaRequest(pecaId, 2, null)]
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AbrirOrdemDeServicoCommand.Servicos) && error.ErrorMessage == "Nao e permitido repetir servicos");
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AbrirOrdemDeServicoCommand.Pecas) && error.ErrorMessage == "Nao e permitido repetir pecas");
    }

    [Fact]
    public void Validate_ComQuantidadesInvalidas_DeveSerInvalido()
    {
        var command = CriarCommandValido() with
        {
            Servicos = [new ServicoSolicitadoRequest(Guid.NewGuid(), 0, null)],
            Pecas = [new PecaSolicitadaRequest(Guid.NewGuid(), -1, null)]
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Servicos[0].Quantidade" && error.ErrorMessage == "Quantidade do servico deve ser maior que zero");
        Assert.Contains(result.Errors, error => error.PropertyName == "Pecas[0].Quantidade" && error.ErrorMessage == "Quantidade da peca deve ser maior que zero");
    }

    [Fact]
    public void Validate_ComObservacoesAcimaDoLimite_DeveSerInvalido()
    {
        var observacao = new string('a', 501);
        var command = CriarCommandValido() with
        {
            Servicos = [new ServicoSolicitadoRequest(Guid.NewGuid(), 1, observacao)],
            Pecas = [new PecaSolicitadaRequest(Guid.NewGuid(), 1, observacao)]
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Servicos[0].Observacao" && error.ErrorMessage == "Observacao deve ter no maximo 500 caracteres");
        Assert.Contains(result.Errors, error => error.PropertyName == "Pecas[0].Observacao" && error.ErrorMessage == "Observacao deve ter no maximo 500 caracteres");
    }

    [Fact]
    public void Validate_ComListasNulas_DeveSerInvalidoSemLancarExcecao()
    {
        var command = CriarCommandValido() with { Servicos = null!, Pecas = null! };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AbrirOrdemDeServicoCommand.Servicos) && error.ErrorMessage == "Servicos sao obrigatorios");
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(AbrirOrdemDeServicoCommand.Pecas) && error.ErrorMessage == "A lista de pecas e obrigatoria");
    }

    [Fact]
    public void Validate_ComElementosNulos_DeveSerInvalidoSemLancarExcecao()
    {
        var command = CriarCommandValido() with { Servicos = [null!], Pecas = [null!] };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Servicos[0]");
        Assert.Contains(result.Errors, error => error.PropertyName == "Pecas[0]");
    }

    private static AbrirOrdemDeServicoCommand CriarCommandValido() => new(new DadosClienteAbertura("Cliente Teste", "52998224725", "11999999999", "cliente@teste.com"), new DadosVeiculoAbertura("ABC1D23", "Honda", "Civic", 2022), [new ServicoSolicitadoRequest(Guid.NewGuid(), 1, "Servico solicitado")], [new PecaSolicitadaRequest(Guid.NewGuid(), 1, "Peca solicitada")]);
}
