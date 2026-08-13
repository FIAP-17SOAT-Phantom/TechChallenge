using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Enums;

namespace OficinaMecanica.Domain.Estoque.Entities;

public class Reserva : Entity
{
 public Guid PecaId { get; private set; }
 public Guid OrdemDeServicoId { get; private set; }
 public int Quantidade { get; private set; }
 public DateTime DataReserva { get; private set; }
 public StatusReserva Status { get; private set; }

 private Reserva() { }

 public Reserva(Guid pecaId, Guid ordemDeServicoId, int quantidade)
 {
 PecaId = pecaId;
 OrdemDeServicoId = ordemDeServicoId;
 Quantidade = quantidade;
 DataReserva = DateTime.UtcNow;
 Status = StatusReserva.Ativa;
 }

 public void Consumir() => Status = StatusReserva.Consumida;
 public void Liberar() => Status = StatusReserva.Liberada;
}
