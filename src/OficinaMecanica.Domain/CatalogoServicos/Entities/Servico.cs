using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.CatalogoServicos.Entities;

public class Servico : AggregateRoot
{
 public string Nome { get; private set; } = null!;
 public string Descricao { get; private set; } = null!;
 public decimal PrecoBase { get; private set; }
 public int TempoEstimadoMinutos { get; private set; }
 public bool Ativo { get; private set; }

 private Servico() { }

 public Servico(string nome, string descricao, decimal precoBase, int tempoEstimadoMinutos)
 {
 if (string.IsNullOrWhiteSpace(nome))
 throw new ArgumentException("Nome do servico e obrigatorio");
 if (precoBase < 0)
 throw new ArgumentException("Preco base nao pode ser negativo");
 if (tempoEstimadoMinutos <= 0)
 throw new ArgumentException("Tempo estimado deve ser maior que zero");

 Nome = nome;
 Descricao = descricao;
 PrecoBase = precoBase;
 TempoEstimadoMinutos = tempoEstimadoMinutos;
 Ativo = true;
 }

 public void Atualizar(string nome, string descricao, decimal precoBase, int tempoEstimadoMinutos)
 {
 Nome = nome;
 Descricao = descricao;
 PrecoBase = precoBase;
 TempoEstimadoMinutos = tempoEstimadoMinutos;
 }

 public void Desativar() => Ativo = false;
 public void Ativar() => Ativo = true;
}
