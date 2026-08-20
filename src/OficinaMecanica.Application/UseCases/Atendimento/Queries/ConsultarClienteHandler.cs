using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed class ConsultarClienteHandler : IRequestHandler<ConsultarClienteQuery, Result<ClienteDto>>
{
    private readonly IClienteRepository _clienteRepository;

    public ConsultarClienteHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<Result<ClienteDto>> Handle(ConsultarClienteQuery request, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId, cancellationToken);

        if (cliente is null)
        {
            return Result.NotFound<ClienteDto>("Cliente nao encontrado");
        }

        var dto = new ClienteDto(
            cliente.Id,
            cliente.Nome,
            cliente.Cpf.Numero,
            cliente.Telefone,
            cliente.Email.Endereco);

        return Result.Success(dto);
    }
}
