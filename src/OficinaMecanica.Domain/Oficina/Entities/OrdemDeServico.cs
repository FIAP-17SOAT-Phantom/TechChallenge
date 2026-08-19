using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Oficina.Enums;
using OficinaMecanica.Domain.Oficina.Events;

namespace OficinaMecanica.Domain.Oficina.Entities;

public class OrdemDeServico : AggregateRoot
{
    public string Numero { get; private set; } = null!;
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public Guid? MecanicoId { get; private set; }
    public StatusOS Status { get; private set; }
    public DateTime DataAbertura { get; private set; }
    public DateTime? DataFinalizacao { get; private set; }
    public string? Diagnostico { get; private set; }
    public Guid? OrcamentoId { get; private set; }

    private readonly List<ItemOS> _itens = new();
    public IReadOnlyCollection<ItemOS> Itens => _itens.AsReadOnly();

    private OrdemDeServico() { } // EF Core

    public OrdemDeServico(string numero, Guid clienteId, Guid veiculoId)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("Numero da OS e obrigatorio");

        Numero = numero;
        ClienteId = clienteId;
        VeiculoId = veiculoId;
        Status = StatusOS.Recebida;
        DataAbertura = DateTime.UtcNow;

        RaiseDomainEvent(new OrdemDeServicoCriadaEvent(Id, numero));
    }

    public Result IniciarDiagnostico(Guid mecanicoId)
    {
        if (Status != StatusOS.Recebida)
            return Result.Failure("OS deve estar com status Recebida para iniciar diagnostico");

        if (mecanicoId == Guid.Empty)
            return Result.Failure("Mecanico e obrigatorio");

        MecanicoId = mecanicoId;
        Status = StatusOS.EmDiagnostico;
        return Result.Success();
    }

    public Result RegistrarDiagnostico(string diagnostico, List<ItemOS> itens)
    {
        if (Status != StatusOS.EmDiagnostico)
            return Result.Failure("OS deve estar Em Diagnostico para registrar diagnostico");

        if (string.IsNullOrWhiteSpace(diagnostico))
            return Result.Failure("Diagnostico nao pode ser vazio");

        if (itens is null || itens.Count == 0)
            return Result.Failure("Diagnostico deve possuir pelo menos um item");

        Diagnostico = diagnostico;
        _itens.Clear();
        _itens.AddRange(itens);
        Status = StatusOS.AguardandoAprovacao;
        return Result.Success();
    }

    public Result VincularOrcamento(Guid orcamentoId)
    {
        if (Status != StatusOS.AguardandoAprovacao)
            return Result.Failure("OS deve estar Aguardando Aprovacao para vincular orcamento");

        OrcamentoId = orcamentoId;
        return Result.Success();
    }

    public Result IniciarExecucao()
    {
        if (Status != StatusOS.AguardandoAprovacao)
            return Result.Failure("OS deve estar Aguardando Aprovacao para iniciar execucao");

        Status = StatusOS.EmExecucao;
        return Result.Success();
    }

    public Result Finalizar()
    {
        if (Status != StatusOS.EmExecucao)
            return Result.Failure("OS deve estar Em Execucao para ser finalizada");

        Status = StatusOS.Finalizada;
        DataFinalizacao = DateTime.UtcNow;

        RaiseDomainEvent(new OrdemDeServicoFinalizadaEvent(Id));
        return Result.Success();
    }

    public Result RegistrarEntrega()
    {
        if (Status != StatusOS.Finalizada)
            return Result.Failure("OS deve estar Finalizada para registrar entrega");

        Status = StatusOS.Entregue;
        return Result.Success();
    }

    public Result Cancelar()
    {
        if (Status == StatusOS.Finalizada || Status == StatusOS.Entregue || Status == StatusOS.Cancelada)
            return Result.Failure("Nao e possivel cancelar OS neste status");

        var statusAnterior = Status;
        Status = StatusOS.Cancelada;

        RaiseDomainEvent(new OrdemDeServicoCanceladaEvent(Id, statusAnterior));
        return Result.Success();
    }
}
