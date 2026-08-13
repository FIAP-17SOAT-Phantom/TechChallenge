using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Domain.Orcamentacao.ValueObjects;

public class ItemOrcamento : ValueObject
{
 public string Descricao { get; }
 public TipoItem Tipo { get; }
 public int Quantidade { get; }
 public decimal ValorUnitario { get; }
 public decimal ValorTotal => Quantidade * ValorUnitario;

 private ItemOrcamento() { }

 public ItemOrcamento(string descricao, TipoItem tipo, int quantidade, decimal valorUnitario)
 {
 Descricao = descricao;
 Tipo = tipo;
 Quantidade = quantidade;
 ValorUnitario = valorUnitario;
 }

 protected override IEnumerable<object?> GetEqualityComponents()
 {
 yield return Descricao;
 yield return Tipo;
 yield return Quantidade;
 yield return ValorUnitario;
 }
}
