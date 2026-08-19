using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Oficina.Entities;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class RegistrarDiagnosticoHandler : IRequestHandler<RegistrarDiagnosticoCommand, Result>
{
    private readonly IOrdemDeServicoRepository _ordemDeServicoRepository;
    private readonly IServicoRepository _servicoRepository;
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarDiagnosticoHandler(IOrdemDeServicoRepository ordemDeServicoRepository, IServicoRepository servicoRepository, IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _ordemDeServicoRepository = ordemDeServicoRepository;
        _servicoRepository = servicoRepository;
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RegistrarDiagnosticoCommand request, CancellationToken cancellationToken)
    {
        var ordemDeServico = await _ordemDeServicoRepository.GetByIdAsync(request.OrdemDeServicoId, cancellationToken);

        if (ordemDeServico is null)
        {
            return Result.Failure("Ordem de Servico nao encontrada");
        }

        var itens = new List<ItemOS>();

        foreach (var itemRequest in request.Itens)
        {
            var servico = await _servicoRepository.GetByIdAsync(itemRequest.ServicoId, cancellationToken);

            if (servico is null || !servico.Ativo)
            {
                return Result.Failure($"Servico {itemRequest.ServicoId} nao encontrado ou inativo");
            }

            if (itemRequest.PecaId.HasValue)
            {
                var peca = await _pecaRepository.GetByIdAsync(itemRequest.PecaId.Value, cancellationToken);

                if (peca is null)
                {
                    return Result.Failure($"Peca {itemRequest.PecaId.Value} nao encontrada");
                }
            }

            itens.Add(new ItemOS(itemRequest.ServicoId, itemRequest.PecaId, itemRequest.Quantidade, itemRequest.Observacao));
        }

        var result = ordemDeServico.RegistrarDiagnostico(request.Diagnostico, itens);

        if (result.IsFailure)
        {
            return result;
        }

        _ordemDeServicoRepository.Update(ordemDeServico);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
