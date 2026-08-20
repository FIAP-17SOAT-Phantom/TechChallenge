using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Estoque.Entities;

public sealed class AlertaEstoque : AggregateRoot
{
    public Guid PecaId { get; private set; }
    public string NomePeca { get; private set; } = null!;
    public int QuantidadeDisponivel { get; private set; }
    public int QuantidadeMinima { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataVisualizacao { get; private set; }
    public DateTime? DataResolucao { get; private set; }
    public bool Visualizado => DataVisualizacao.HasValue;
    public bool Resolvido => DataResolucao.HasValue;

    private AlertaEstoque() { }

    public AlertaEstoque(Guid pecaId, string nomePeca, int quantidadeDisponivel, int quantidadeMinima)
    {
        if (pecaId == Guid.Empty)
            throw new ArgumentException("Peca e obrigatoria");

        if (string.IsNullOrWhiteSpace(nomePeca))
            throw new ArgumentException("Nome da peca e obrigatorio");

        PecaId = pecaId;
        NomePeca = nomePeca;
        QuantidadeDisponivel = quantidadeDisponivel;
        QuantidadeMinima = quantidadeMinima;
        DataCriacao = DateTime.UtcNow;
    }

    public void AtualizarQuantidade(int quantidadeDisponivel, int quantidadeMinima)
    {
        QuantidadeDisponivel = quantidadeDisponivel;
        QuantidadeMinima = quantidadeMinima;
    }

    public Result MarcarComoVisualizado()
    {
        if (Resolvido)
            return Result.Failure("Alerta resolvido nao pode ser alterado");

        DataVisualizacao ??= DateTime.UtcNow;
        return Result.Success();
    }

    public Result Resolver()
    {
        if (Resolvido)
            return Result.Failure("Alerta ja foi resolvido");

        DataVisualizacao ??= DateTime.UtcNow;
        DataResolucao = DateTime.UtcNow;
        return Result.Success();
    }
}
