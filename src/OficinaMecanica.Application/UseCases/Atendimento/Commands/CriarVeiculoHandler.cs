using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class CriarVeiculoHandler : IRequestHandler<CriarVeiculoCommand, Result<Guid>>
{
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarVeiculoHandler(IVeiculoRepository veiculoRepository, IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
    {
        _veiculoRepository = veiculoRepository;
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CriarVeiculoCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken);

        if (cliente is null)
        {
            return Result.NotFound<Guid>("Cliente nao encontrado");
        }

        var placaResult = Placa.Criar(request.Placa);

        if (placaResult.IsFailure)
        {
            return Result.Failure<Guid>(placaResult.Error, placaResult.ErrorType);
        }

        if (await _veiculoRepository.ExistsByPlacaAsync(placaResult.Value, cancellationToken))
        {
            return Result.Conflict<Guid>("Ja existe um veiculo cadastrado com esta placa");
        }

        var veiculo = new Veiculo(placaResult.Value, request.Marca, request.Modelo, request.Ano, request.ClienteId);

        await _veiculoRepository.AddAsync(veiculo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(veiculo.Id);
    }
}
