using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class CriarClienteHandler : IRequestHandler<CriarClienteCommand, Result<Guid>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarClienteHandler(IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
    {
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CriarClienteCommand request, CancellationToken cancellationToken)
    {
        // Criar Value Objects com validacao
        var cpfResult = Cpf.Criar(request.Cpf);
        if (cpfResult.IsFailure)
            return Result.Failure<Guid>(cpfResult.Error, cpfResult.ErrorType);

        var emailResult = Email.Criar(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error, emailResult.ErrorType);

        // Verificar duplicidade de CPF
        if (await _clienteRepository.ExistsByCpfAsync(cpfResult.Value, cancellationToken))
            return Result.Conflict<Guid>("Ja existe um cliente cadastrado com este CPF");

        // Criar aggregate
        var cliente = new Cliente(request.Nome, cpfResult.Value, request.Telefone, emailResult.Value);

        // Persistir
        await _clienteRepository.AddAsync(cliente, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(cliente.Id);
    }
}
