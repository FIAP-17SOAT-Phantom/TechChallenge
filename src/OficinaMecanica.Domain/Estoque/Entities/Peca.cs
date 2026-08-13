using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Events;

namespace OficinaMecanica.Domain.Estoque.Entities;

public class Peca : AggregateRoot
{
 public string Nome { get; private set; }
 public string Codigo { get; private set; }
 public string Descricao { get; private set; }
 public decimal PrecoUnitario { get; private set; }
 public int QuantidadeEmEstoque { get; private set; }
 public int QuantidadeReservada { get; private set; }
 public int QuantidadeMinima { get; private set; }
 public int QuantidadeDisponivel => QuantidadeEmEstoque - QuantidadeReservada;

 private readonly List<Reserva> _reservas = new();
 public IReadOnlyCollection<Reserva> Reservas => _reservas.AsReadOnly();

 private Peca() { }

 public Peca(string nome, string codigo, string descricao, decimal precoUnitario, int quantidadeEmEstoque, int quantidadeMinima)
 {
 Nome = nome;
 Codigo = codigo;
 Descricao = descricao;
 PrecoUnitario = precoUnitario;
 QuantidadeEmEstoque = quantidadeEmEstoque;
 QuantidadeMinima = quantidadeMinima;
 QuantidadeReservada = 0;
 }

 public void AtualizarEstoque(int quantidade)
 {
 QuantidadeEmEstoque += quantidade;
 }

 public void AtualizarPreco(decimal novoPreco)
 {
 PrecoUnitario = novoPreco;
 }

 public Result<Reserva> Reservar(Guid ordemDeServicoId, int quantidade)
 {
 if (quantidade > QuantidadeDisponivel)
 return Result.Failure<Reserva>($"Estoque insuficiente para {Nome}. Disponivel: {QuantidadeDisponivel}, Solicitado: {quantidade}");

 QuantidadeReservada += quantidade;
 var reserva = new Reserva(Id, ordemDeServicoId, quantidade);
 _reservas.Add(reserva);

 if (QuantidadeDisponivel <= QuantidadeMinima)
 RaiseDomainEvent(new EstoqueBaixoEvent(Id, Nome, QuantidadeDisponivel, QuantidadeMinima));

 return Result.Success(reserva);
 }

 public Result LiberarReserva(Guid reservaId)
 {
 var reserva = _reservas.FirstOrDefault(r => r.Id == reservaId);
 if (reserva is null)
 return Result.Failure("Reserva nao encontrada");

 reserva.Liberar();
 QuantidadeReservada -= reserva.Quantidade;
 return Result.Success();
 }

 public Result Consumir(Guid reservaId)
 {
 var reserva = _reservas.FirstOrDefault(r => r.Id == reservaId);
 if (reserva is null)
 return Result.Failure("Reserva nao encontrada");

 reserva.Consumir();
 QuantidadeEmEstoque -= reserva.Quantidade;
 QuantidadeReservada -= reserva.Quantidade;
 return Result.Success();
 }
}
