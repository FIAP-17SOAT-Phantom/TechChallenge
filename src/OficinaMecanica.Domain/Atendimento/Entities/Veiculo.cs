using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Atendimento.Entities;

public class Veiculo : Entity
{
 public Placa Placa { get; private set; }
 public string Marca { get; private set; }
 public string Modelo { get; private set; }
 public int Ano { get; private set; }
 public Guid ClienteId { get; private set; }

 private Veiculo() { } // EF Core

 public Veiculo(Placa placa, string marca, string modelo, int ano, Guid clienteId)
 {
 Placa = placa;
 Marca = marca;
 Modelo = modelo;
 Ano = ano;
 ClienteId = clienteId;
 }

 public void Atualizar(string marca, string modelo, int ano)
 {
 Marca = marca;
 Modelo = modelo;
 Ano = ano;
 }
}
