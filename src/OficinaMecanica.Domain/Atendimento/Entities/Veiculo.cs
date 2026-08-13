using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Atendimento.Entities;

public class Veiculo : AggregateRoot
{
 public Placa Placa { get; private set; } = null!;
 public string Marca { get; private set; } = null!;
 public string Modelo { get; private set; } = null!;
 public int Ano { get; private set; }
 public Guid ClienteId { get; private set; }

 private Veiculo() { } // EF Core

 public Veiculo(Placa placa, string marca, string modelo, int ano, Guid clienteId)
 {
 if (string.IsNullOrWhiteSpace(marca))
 throw new ArgumentException("Marca e obrigatoria");
 if (string.IsNullOrWhiteSpace(modelo))
 throw new ArgumentException("Modelo e obrigatorio");
 if (ano < 1900 || ano > DateTime.UtcNow.Year + 2)
 throw new ArgumentException("Ano invalido");

 Placa = placa;
 Marca = marca;
 Modelo = modelo;
 Ano = ano;
 ClienteId = clienteId;
 }

 /// <summary>
 /// Regra de dominio: verifica se o veiculo pertence ao cliente informado.
 /// </summary>
 public bool PertenceAoCliente(Guid clienteId) => ClienteId == clienteId;

 public void Atualizar(string marca, string modelo, int ano)
 {
 Marca = marca;
 Modelo = modelo;
 Ano = ano;
 }
}
