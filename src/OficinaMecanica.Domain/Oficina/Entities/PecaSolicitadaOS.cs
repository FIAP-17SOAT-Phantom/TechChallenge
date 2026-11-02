using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Oficina.Entities;

public class PecaSolicitadaOS : ValueObject
{
    public Guid PecaId { get; }
    public int Quantidade { get; }
    public string? Observacao { get; }

    private PecaSolicitadaOS() { }

    public PecaSolicitadaOS(Guid pecaId, int quantidade, string? observacao = null)
    {
        if (pecaId == Guid.Empty)
            throw new ArgumentException("Peca e obrigatoria");

        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");

        if (observacao?.Length > 500)
            throw new ArgumentException("Observacao deve ter no maximo 500 caracteres");

        PecaId = pecaId;
        Quantidade = quantidade;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PecaId;
        yield return Quantidade;
        yield return Observacao;
    }
}
