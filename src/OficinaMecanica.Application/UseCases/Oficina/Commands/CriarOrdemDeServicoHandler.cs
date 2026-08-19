using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Oficina.Entities;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class CriarOrdemDeServicoHandler : IRequestHandler<CriarOrdemDeServicoCommand, Result<Guid>>
{
    private readonly IOrdemDeServicoRepository _osRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarOrdemDeServicoHandler(
    IOrdemDeServicoRepository osRepository,
    IClienteRepository clienteRepository,
    IVeiculoRepository veiculoRepository,
    IUnitOfWork unitOfWork)
    {
        _osRepository = osRepository;
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CriarOrdemDeServicoCommand request, CancellationToken cancellationToken)
    {
        // Validar existencia do cliente
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken);
        if (cliente is null)
            return Result.Failure<Guid>("Cliente nao encontrado");

        // Validar existencia do veiculo
        var veiculo = await _veiculoRepository.GetByIdAsync(request.VeiculoId, cancellationToken);
        if (veiculo is null)
            return Result.Failure<Guid>("Veiculo nao encontrado");

        // Regra de dominio: veiculo deve pertencer ao cliente (delegada ao aggregate)
        if (!veiculo.PertenceAoCliente(request.ClienteId))
            return Result.Failure<Guid>("Veiculo nao pertence ao cliente informado");

        // Gerar numero sequencial
        var numero = await _osRepository.GerarProximoNumeroAsync(cancellationToken);

        // Criar aggregate (dispara OrdemDeServicoCriadaEvent)
        var os = new OrdemDeServico(numero, request.ClienteId, request.VeiculoId);

        // Persistir
        await _osRepository.AddAsync(os, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(os.Id);
    }
}
