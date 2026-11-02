using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Atendimento.Entities;
using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Oficina.Entities;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class AbrirOrdemDeServicoHandler : IRequestHandler<AbrirOrdemDeServicoCommand, Result<OrdemDeServicoCriadaDto>>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IVeiculoRepository _veiculoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AbrirOrdemDeServicoHandler(IOrdemDeServicoRepository ordemDeServicoRepository, IClienteRepository clienteRepository, IVeiculoRepository veiculoRepository, IServicoRepository servicoRepository, IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _clienteRepository = clienteRepository;
        _veiculoRepository = veiculoRepository;
        _servicoRepository = servicoRepository;
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<OrdemDeServicoCriadaDto>> Handle(AbrirOrdemDeServicoCommand request, CancellationToken cancellationToken) => _unitOfWork.ExecuteInTransactionAsync(token => AbrirAsync(request, token), cancellationToken);

    private async Task<Result<OrdemDeServicoCriadaDto>> AbrirAsync(AbrirOrdemDeServicoCommand request, CancellationToken cancellationToken)
    {
        var estruturaResult = ValidarEstrutura(request);

        if (estruturaResult.IsFailure)
            return ConverterFalha<OrdemDeServicoCriadaDto>(estruturaResult);

        var dadosResult = NormalizarDados(request);

        if (dadosResult.IsFailure)
            return ConverterFalha<OrdemDeServicoCriadaDto>(dadosResult);

        var clienteResult = await ObterClienteAsync(request.Cliente, dadosResult.Value, cancellationToken);

        if (clienteResult.IsFailure)
            return ConverterFalha<OrdemDeServicoCriadaDto>(clienteResult);

        var veiculoResult = await ObterVeiculoAsync(request.Veiculo, dadosResult.Value.Placa, clienteResult.Value.Cliente, cancellationToken);

        if (veiculoResult.IsFailure)
            return ConverterFalha<OrdemDeServicoCriadaDto>(veiculoResult);

        var catalogoResult = await ValidarCatalogoAsync(request, cancellationToken);

        if (catalogoResult.IsFailure)
            return ConverterFalha<OrdemDeServicoCriadaDto>(catalogoResult);

        return await PersistirAsync(request, clienteResult.Value, veiculoResult.Value, cancellationToken);
    }

    private static Result ValidarEstrutura(AbrirOrdemDeServicoCommand request)
    {
        if (request.Cliente is null || request.Veiculo is null || request.Servicos is null || request.Pecas is null)
            return Result.Failure("Dados de abertura da OS incompletos", ErrorType.Validation);

        if (request.Servicos.Count == 0)
            return Result.Failure("Informe pelo menos um servico", ErrorType.Validation);

        if (request.Servicos.Any(item => item is null) || request.Pecas.Any(item => item is null))
            return Result.Failure("Servicos e pecas informados devem ser validos", ErrorType.Validation);

        if (request.Servicos.Any(item => item.ServicoId == Guid.Empty || item.Quantidade <= 0 || item.Observacao?.Length > 500) || request.Pecas.Any(item => item.PecaId == Guid.Empty || item.Quantidade <= 0 || item.Observacao?.Length > 500))
            return Result.Failure("Servicos e pecas devem possuir identificadores validos e quantidades maiores que zero", ErrorType.Validation);

        return request.Servicos.Select(item => item.ServicoId).Distinct().Count() != request.Servicos.Count || request.Pecas.Select(item => item.PecaId).Distinct().Count() != request.Pecas.Count ? Result.Failure("Nao e permitido repetir servicos ou pecas", ErrorType.Validation) : Result.Success();
    }

    private static Result<DadosNormalizados> NormalizarDados(AbrirOrdemDeServicoCommand request)
    {
        var cpfResult = Cpf.Criar(request.Cliente.Cpf);

        if (cpfResult.IsFailure)
            return Result.Failure<DadosNormalizados>(cpfResult.Error, ErrorType.Validation);

        var emailResult = Email.Criar(request.Cliente.Email);

        if (emailResult.IsFailure)
            return Result.Failure<DadosNormalizados>(emailResult.Error, ErrorType.Validation);

        var placaResult = Placa.Criar(request.Veiculo.Placa);
        return placaResult.IsFailure ? Result.Failure<DadosNormalizados>(placaResult.Error, ErrorType.Validation) : Result.Success(new DadosNormalizados(cpfResult.Value, emailResult.Value, placaResult.Value));
    }

    private async Task<Result<ClienteResolvido>> ObterClienteAsync(DadosClienteAbertura dados, DadosNormalizados normalizados, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.GetByCpfAsync(normalizados.Cpf, cancellationToken);

        if (cliente is not null && DadosClienteDivergem(cliente, dados, normalizados.Email))
            return Result.Conflict<ClienteResolvido>("Os dados informados divergem do cliente cadastrado. Atualize o cadastro do cliente antes de abrir a OS");

        return Result.Success(cliente is null ? new ClienteResolvido(new Cliente(dados.Nome, normalizados.Cpf, dados.Telefone, normalizados.Email), true) : new ClienteResolvido(cliente, false));
    }

    private async Task<Result<VeiculoResolvido>> ObterVeiculoAsync(DadosVeiculoAbertura dados, Placa placa, Cliente cliente, CancellationToken cancellationToken)
    {
        var veiculo = await _veiculoRepository.GetByPlacaAsync(placa, cancellationToken);

        if (veiculo is not null && !veiculo.PertenceAoCliente(cliente.Id))
            return Result.Conflict<VeiculoResolvido>("Veiculo ja cadastrado para outro cliente");

        if (veiculo is not null && DadosVeiculoDivergem(veiculo, dados))
            return Result.Conflict<VeiculoResolvido>("Os dados informados divergem do veiculo cadastrado. Atualize o cadastro do veiculo antes de abrir a OS");

        return Result.Success(veiculo is null ? new VeiculoResolvido(new Veiculo(placa, dados.Marca, dados.Modelo, dados.Ano, cliente.Id), true) : new VeiculoResolvido(veiculo, false));
    }

    private async Task<Result> ValidarCatalogoAsync(AbrirOrdemDeServicoCommand request, CancellationToken cancellationToken)
    {
        var servicoIds = request.Servicos.Select(item => item.ServicoId).ToArray();
        var servicos = await _servicoRepository.GetByIdsAsync(servicoIds, cancellationToken);
        var servicoAusente = servicoIds.Except(servicos.Select(servico => servico.Id)).FirstOrDefault();

        if (servicoAusente != Guid.Empty)
            return Result.NotFound($"Servico {servicoAusente} nao encontrado");

        var servicoInativo = servicos.FirstOrDefault(servico => !servico.Ativo);

        if (servicoInativo is not null)
            return Result.Conflict($"Servico {servicoInativo.Nome} esta inativo");

        var pecaIds = request.Pecas.Select(item => item.PecaId).ToArray();
        var pecas = await _pecaRepository.GetByIdsAsync(pecaIds, cancellationToken);
        var pecaAusente = pecaIds.Except(pecas.Select(peca => peca.Id)).FirstOrDefault();
        return pecaAusente == Guid.Empty ? Result.Success() : Result.NotFound($"Peca {pecaAusente} nao encontrada");
    }

    private async Task<Result<OrdemDeServicoCriadaDto>> PersistirAsync(AbrirOrdemDeServicoCommand request, ClienteResolvido clienteResolvido, VeiculoResolvido veiculoResolvido, CancellationToken cancellationToken)
    {
        var servicosSolicitados = request.Servicos.Select(item => new ServicoSolicitadoOS(item.ServicoId, item.Quantidade, item.Observacao)).ToList();
        var pecasSolicitadas = request.Pecas.Select(item => new PecaSolicitadaOS(item.PecaId, item.Quantidade, item.Observacao)).ToList();
        var numero = await _ordemDeServicoRepository.GerarProximoNumeroAsync(cancellationToken);
        var ordemDeServico = new OrdemDeServico(numero, clienteResolvido.Cliente.Id, veiculoResolvido.Veiculo.Id, servicosSolicitados, pecasSolicitadas);

        if (clienteResolvido.Novo)
            await _clienteRepository.AddAsync(clienteResolvido.Cliente, cancellationToken);

        if (veiculoResolvido.Novo)
            await _veiculoRepository.AddAsync(veiculoResolvido.Veiculo, cancellationToken);

        await _ordemDeServicoRepository.AddAsync(ordemDeServico, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new OrdemDeServicoCriadaDto(ordemDeServico.Id, ordemDeServico.Numero));
    }

    private static Result<T> ConverterFalha<T>(Result result) => Result.Failure<T>(result.Error, result.ErrorType);
    private static bool DadosClienteDivergem(Cliente cliente, DadosClienteAbertura dados, Email email) => !string.Equals(cliente.Nome.Trim(), dados.Nome.Trim(), StringComparison.OrdinalIgnoreCase) || !string.Equals(cliente.Telefone.Trim(), dados.Telefone.Trim(), StringComparison.Ordinal) || cliente.Email != email;
    private static bool DadosVeiculoDivergem(Veiculo veiculo, DadosVeiculoAbertura dados) => !string.Equals(veiculo.Marca.Trim(), dados.Marca.Trim(), StringComparison.OrdinalIgnoreCase) || !string.Equals(veiculo.Modelo.Trim(), dados.Modelo.Trim(), StringComparison.OrdinalIgnoreCase) || veiculo.Ano != dados.Ano;
    private sealed record DadosNormalizados(Cpf Cpf, Email Email, Placa Placa);
    private sealed record ClienteResolvido(Cliente Cliente, bool Novo);
    private sealed record VeiculoResolvido(Veiculo Veiculo, bool Novo);
}
