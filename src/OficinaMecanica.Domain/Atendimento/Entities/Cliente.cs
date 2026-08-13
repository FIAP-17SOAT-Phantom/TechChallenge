using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Atendimento.Entities;

public class Cliente : AggregateRoot
{
 public string Nome { get; private set; }
 public Cpf Cpf { get; private set; }
 public string Telefone { get; private set; }
 public string Email { get; private set; }

 private readonly List<Veiculo> _veiculos = new();
 public IReadOnlyCollection<Veiculo> Veiculos => _veiculos.AsReadOnly();

 private Cliente() { } // EF Core

 public Cliente(string nome, Cpf cpf, string telefone, string email)
 {
 Nome = nome;
 Cpf = cpf;
 Telefone = telefone;
 Email = email;
 }

 public void Atualizar(string nome, string telefone, string email)
 {
 Nome = nome;
 Telefone = telefone;
 Email = email;
 }

 public Result<Veiculo> AdicionarVeiculo(Placa placa, string marca, string modelo, int ano)
 {
 if (_veiculos.Any(v => v.Placa == placa))
 return Result.Failure<Veiculo>("Veiculo com esta placa ja esta cadastrado para este cliente");

 var veiculo = new Veiculo(placa, marca, modelo, ano, Id);
 _veiculos.Add(veiculo);
 return Result.Success(veiculo);
 }
}
