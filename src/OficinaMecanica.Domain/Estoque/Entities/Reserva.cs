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
        if (quantidade <= 0)
            throw new ArgumentException("Quantidade da reserva deve ser maior que zero");

        PecaId = pecaId;
        OrdemDeServicoId = ordemDeServicoId;
        Quantidade = quantidade;
        DataReserva = DateTime.UtcNow;
        Status = StatusReserva.Ativa;
    }

    public Result Consumir()
    {
        if (Status != StatusReserva.Ativa)
            return Result.Failure("Apenas reservas ativas podem ser consumidas");

        Status = StatusReserva.Consumida;
        return Result.Success();
    }

    public Result Liberar()
    {
        if (Status != StatusReserva.Ativa)
            return Result.Failure("Apenas reservas ativas podem ser liberadas");

        Status = StatusReserva.Liberada;
        return Result.Success();
    }
}
