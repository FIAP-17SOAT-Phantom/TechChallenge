using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Events;

namespace OficinaMecanica.Domain.Estoque.Entities;

public class Peca : AggregateRoot
{
 public string Nome { get; private set; } = null!;
 public string Codigo { get; private set; } = null!;
 public string Descricao { get; private set; } = null!;
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
 if (string.IsNullOrWhiteSpace(nome))
 throw new ArgumentException("Nome da peca e obrigatorio");
 if (precoUnitario < 0)
 throw new ArgumentException("Preco unitario nao pode ser negativo");
 if (quantidadeEmEstoque < 0)
 throw new ArgumentException("Quantidade em estoque nao pode ser negativa");
 if (quantidadeMinima < 0)
 throw new ArgumentException("Quantidade minima nao pode ser negativa");

 Nome = nome;
 Codigo = codigo;
 Descricao = descricao;
 PrecoUnitario = precoUnitario;
 QuantidadeEmEstoque = quantidadeEmEstoque;
 QuantidadeMinima = quantidadeMinima;
 QuantidadeReservada = 0;
 }

 public Result AdicionarEstoque(int quantidade)
 {
 if (quantidade <= 0)
 return Result.Failure("Quantidade a adicionar deve ser maior que zero");

 QuantidadeEmEstoque += quantidade;
 return Result.Success();
 }

 public void AtualizarPreco(decimal novoPreco)
 {
 if (novoPreco < 0)
 throw new ArgumentException("Preco nao pode ser negativo");
 PrecoUnitario = novoPreco;
 }

 public void Atualizar(string nome, string descricao, decimal precoUnitario, int quantidadeMinima)
 {
 Nome = nome;
 Descricao = descricao;
 PrecoUnitario = precoUnitario;
 QuantidadeMinima = quantidadeMinima;
 }

 public Result<Reserva> Reservar(Guid ordemDeServicoId, int quantidade)
 {
 if (quantidade <= 0)
 return Result.Failure<Reserva>("Quantidade a reservar deve ser maior que zero");

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

 var result = reserva.Liberar();
 if (result.IsFailure)
 return result;

 QuantidadeReservada -= reserva.Quantidade;
 return Result.Success();
 }

 public Result Consumir(Guid reservaId)
 {
 var reserva = _reservas.FirstOrDefault(r => r.Id == reservaId);
 if (reserva is null)
 return Result.Failure("Reserva nao encontrada");

 var result = reserva.Consumir();
 if (result.IsFailure)
 return result;

 QuantidadeEmEstoque -= reserva.Quantidade;
 QuantidadeReservada -= reserva.Quantidade;
 return Result.Success();
 }
}
