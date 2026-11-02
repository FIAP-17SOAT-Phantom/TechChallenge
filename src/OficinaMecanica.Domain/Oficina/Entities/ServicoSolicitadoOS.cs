using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Oficina.Entities;

public class ServicoSolicitadoOS : ValueObject
{
    public Guid ServicoId { get; }
    public int Quantidade { get; }
    public string? Observacao { get; }

    private ServicoSolicitadoOS() { }

    public ServicoSolicitadoOS(Guid servicoId, int quantidade, string? observacao = null)
    {
        if (servicoId == Guid.Empty)
            throw new ArgumentException("Servico e obrigatorio");

        if (quantidade <= 0)
            throw new ArgumentException("Quantidade deve ser maior que zero");

        if (observacao?.Length > 500)
            throw new ArgumentException("Observacao deve ter no maximo 500 caracteres");

        ServicoId = servicoId;
        Quantidade = quantidade;
        Observacao = string.IsNullOrWhiteSpace(observacao) ? null : observacao.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ServicoId;
        yield return Quantidade;
        yield return Observacao;
    }
}
