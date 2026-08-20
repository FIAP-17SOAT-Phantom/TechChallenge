using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class AtualizarClienteHandler : IRequestHandler<AtualizarClienteCommand, Result>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AtualizarClienteHandler(IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
    {
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AtualizarClienteCommand request, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken);

        if (cliente is null)
        {
            return Result.NotFound("Cliente nao encontrado");
        }

        var emailResult = Email.Criar(request.Email);

        if (emailResult.IsFailure)
        {
            return emailResult;
        }

        cliente.Atualizar(request.Nome, request.Telefone, emailResult.Value);

        _clienteRepository.Update(cliente);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
