using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Atendimento.Queries;

public sealed class ListarClientesHandler : IRequestHandler<ListarClientesQuery, IReadOnlyList<ClienteDto>>
{
    private readonly IClienteRepository _clienteRepository;

    public ListarClientesHandler(IClienteRepository clienteRepository)
    {
        _clienteRepository = clienteRepository;
    }

    public async Task<IReadOnlyList<ClienteDto>> Handle(ListarClientesQuery request, CancellationToken cancellationToken)
    {
        var clientes = await _clienteRepository.GetAllAsync(cancellationToken);

        return clientes
            .Select(cliente => new ClienteDto(
                cliente.Id,
                cliente.Nome,
                cliente.Cpf.Numero,
                cliente.Telefone,
                cliente.Email.Endereco))
            .ToList();
    }
}
