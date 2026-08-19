using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class ExcluirClienteHandler : IRequestHandler<ExcluirClienteCommand, Result>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExcluirClienteHandler(IClienteRepository clienteRepository, IVeiculoRepository veiculoRepository, IOrdemDeServicoRepository ordemDeServicoRepository, IUnitOfWork unitOfWork)
    {
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ExcluirClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken);

        if (cliente is null)
        {
            return Result.Failure("Cliente nao encontrado");
        }

        var veiculos = await _veiculoRepository.GetByClienteIdAsync(request.ClienteId, cancellationToken);

        if (veiculos.Count > 0)
        {
            return Result.Failure("Cliente possui veiculos vinculados");
        }

        var ordensDeServico = await _ordemDeServicoRepository.GetByClienteIdAsync(request.ClienteId, cancellationToken);

        if (ordensDeServico.Count > 0)
        {
            return Result.Failure("Cliente possui ordens de servico vinculadas");
        }

        _clienteRepository.Remove(cliente);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
