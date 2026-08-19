using OficinaMecanica.Domain.Estoque.Entities;
using OficinaMecanica.Domain.Estoque.Enums;
using OficinaMecanica.Domain.Estoque.Events;

namespace OficinaMecanica.Tests.Domain.Estoque;

public sealed class PecaTests
{
    [Fact]
    public void Reservar_ComEstoqueDisponivel_DeveCriarReserva()
    {
        var peca = CriarPeca(10, 2);

        var result = peca.Reservar(Guid.NewGuid(), 4);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, peca.QuantidadeReservada);
        Assert.Equal(6, peca.QuantidadeDisponivel);
        Assert.Equal(StatusReserva.Ativa, result.Value.Status);
    }

    [Fact]
    public void Reservar_ComEstoqueInsuficiente_NaoDeveAlterarEstoque()
    {
        var peca = CriarPeca(3, 1);

        var result = peca.Reservar(Guid.NewGuid(), 4);

        Assert.True(result.IsFailure);
        Assert.Equal(0, peca.QuantidadeReservada);
        Assert.Empty(peca.Reservas);
    }

    [Fact]
    public void Reservar_AtingindoEstoqueMinimo_DeveEmitirEvento()
    {
        var peca = CriarPeca(5, 2);

        peca.Reservar(Guid.NewGuid(), 3);

        Assert.Contains(peca.DomainEvents, evento => evento is EstoqueBaixoEvent);
    }

    [Fact]
    public void ConsumirReserva_DeveBaixarEstoqueFisicoEReservado()
    {
        var peca = CriarPeca(10, 2);
        var reserva = peca.Reservar(Guid.NewGuid(), 4).Value;

        var result = peca.Consumir(reserva.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, peca.QuantidadeEmEstoque);
        Assert.Equal(0, peca.QuantidadeReservada);
        Assert.Equal(StatusReserva.Consumida, reserva.Status);
    }

    [Fact]
    public void LiberarReserva_NaoDeveBaixarEstoqueFisico()
    {
        var peca = CriarPeca(10, 2);
        var reserva = peca.Reservar(Guid.NewGuid(), 4).Value;

        var result = peca.LiberarReserva(reserva.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, peca.QuantidadeEmEstoque);
        Assert.Equal(0, peca.QuantidadeReservada);
        Assert.Equal(StatusReserva.Liberada, reserva.Status);
    }

    [Fact]
    public void ConsumirReservaDuasVezes_DeveFalharSemNovaBaixa()
    {
        var peca = CriarPeca(10, 2);
        var reserva = peca.Reservar(Guid.NewGuid(), 4).Value;
        peca.Consumir(reserva.Id);

        var result = peca.Consumir(reserva.Id);

        Assert.True(result.IsFailure);
        Assert.Equal(6, peca.QuantidadeEmEstoque);
    }

    [Fact]
    public void AdicionarEstoque_ComQuantidadeValida_DeveSomarQuantidade()
    {
        var peca = CriarPeca(10, 2);

        var result = peca.AdicionarEstoque(5);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, peca.QuantidadeEmEstoque);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AdicionarEstoque_ComQuantidadeInvalida_DeveFalhar(int quantidade)
    {
        var peca = CriarPeca(10, 2);

        var result = peca.AdicionarEstoque(quantidade);

        Assert.True(result.IsFailure);
        Assert.Equal(10, peca.QuantidadeEmEstoque);
    }

    [Fact]
    public void Reservar_ComQuantidadeInvalida_DeveFalhar()
    {
        var peca = CriarPeca(10, 2);

        var result = peca.Reservar(Guid.NewGuid(), 0);

        Assert.True(result.IsFailure);
        Assert.Empty(peca.Reservas);
    }

    [Fact]
    public void Consumir_ReservaInexistente_DeveFalhar()
    {
        var peca = CriarPeca(10, 2);

        var result = peca.Consumir(Guid.NewGuid());

        Assert.True(result.IsFailure);
        Assert.Equal(10, peca.QuantidadeEmEstoque);
    }

    [Fact]
    public void LiberarReservaDuasVezes_DeveFalharSemAlterarQuantidade()
    {
        var peca = CriarPeca(10, 2);
        var reserva = peca.Reservar(Guid.NewGuid(), 4).Value;
        peca.LiberarReserva(reserva.Id);

        var result = peca.LiberarReserva(reserva.Id);

        Assert.True(result.IsFailure);
        Assert.Equal(0, peca.QuantidadeReservada);
    }

    [Fact]
    public void Atualizar_ComValoresValidos_DeveAlterarDados()
    {
        var peca = CriarPeca(10, 2);

        peca.Atualizar("Filtro premium", "Nova descricao", 75m, 3);

        Assert.Equal("Filtro premium", peca.Nome);
        Assert.Equal("Nova descricao", peca.Descricao);
        Assert.Equal(75m, peca.PrecoUnitario);
        Assert.Equal(3, peca.QuantidadeMinima);
    }

    [Fact]
    public void AtualizarPreco_ComPrecoNegativo_DeveLancarExcecao()
    {
        var peca = CriarPeca(10, 2);

        Assert.Throws<ArgumentException>(() => peca.AtualizarPreco(-1m));
    }

    private static Peca CriarPeca(int quantidadeEmEstoque, int quantidadeMinima) => new("Filtro", "FLT-001", "Filtro de oleo", 50m, quantidadeEmEstoque, quantidadeMinima);
}
