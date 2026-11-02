using MediatR;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed record AbrirOrdemDeServicoCommand(DadosClienteAbertura Cliente, DadosVeiculoAbertura Veiculo, IReadOnlyList<ServicoSolicitadoRequest> Servicos, IReadOnlyList<PecaSolicitadaRequest> Pecas) : IRequest<Result<OrdemDeServicoCriadaDto>>;
public sealed record DadosClienteAbertura(string Nome, string Cpf, string Telefone, string Email);
public sealed record DadosVeiculoAbertura(string Placa, string Marca, string Modelo, int Ano);
public sealed record ServicoSolicitadoRequest(Guid ServicoId, int Quantidade, string? Observacao);
public sealed record PecaSolicitadaRequest(Guid PecaId, int Quantidade, string? Observacao);
public sealed record OrdemDeServicoCriadaDto(Guid Id, string Numero);
