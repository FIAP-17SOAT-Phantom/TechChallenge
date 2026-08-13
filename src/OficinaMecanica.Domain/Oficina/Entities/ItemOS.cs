using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Oficina.Entities;

public class ItemOS : ValueObject
{
 public Guid ServicoId { get; }
 public Guid? PecaId { get; }
 public int Quantidade { get; }
 public string? Observacao { get; }

 private ItemOS() { } // EF Core

 public ItemOS(Guid servicoId, Guid? pecaId, int quantidade, string? observacao = null)
 {
 ServicoId = servicoId;
 PecaId = pecaId;
 Quantidade = quantidade;
 Observacao = observacao;
 }

 protected override IEnumerable<object?> GetEqualityComponents()
 {
 yield return ServicoId;
 yield return PecaId;
 yield return Quantidade;
 }
}
