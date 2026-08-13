using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.CatalogoServicos.Entities;

public class Servico : AggregateRoot
{
 public string Nome { get; private set; }
 public string Descricao { get; private set; }
 public decimal PrecoBase { get; private set; }
 public int TempoEstimadoMinutos { get; private set; }
 public bool Ativo { get; private set; }

 private Servico() { }

 public Servico(string nome, string descricao, decimal precoBase, int tempoEstimadoMinutos)
 {
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
