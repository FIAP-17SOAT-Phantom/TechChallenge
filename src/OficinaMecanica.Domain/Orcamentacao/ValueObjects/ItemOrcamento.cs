using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Domain.Orcamentacao.ValueObjects;

public class ItemOrcamento : ValueObject
{
 public string Descricao { get; } = null!;
 public TipoItem Tipo { get; }
 public int Quantidade { get; }
 public decimal ValorUnitario { get; }
 public decimal ValorTotal => Quantidade * ValorUnitario;
 public Guid? PecaId { get; }
 public Guid? ServicoId { get; }

 private ItemOrcamento() { }

 public ItemOrcamento(string descricao, TipoItem tipo, int quantidade, decimal valorUnitario, Guid? pecaId = null, Guid? servicoId = null)
 {
 if (string.IsNullOrWhiteSpace(descricao))
 throw new ArgumentException("Descricao do item e obrigatoria");
 if (quantidade <= 0)
 throw new ArgumentException("Quantidade deve ser maior que zero");
 if (valorUnitario < 0)
 throw new ArgumentException("Valor unitario nao pode ser negativo");

 Descricao = descricao;
 Tipo = tipo;
 Quantidade = quantidade;
 ValorUnitario = valorUnitario;
 PecaId = pecaId;
 ServicoId = servicoId;
 }

 protected override IEnumerable<object?> GetEqualityComponents()
 {
 yield return Descricao;
 yield return Tipo;
 yield return Quantidade;
 yield return ValorUnitario;
 yield return PecaId;
 yield return ServicoId;
 }
}
