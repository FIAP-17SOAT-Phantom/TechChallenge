using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.CatalogoServicos.Queries;

public sealed class ConsultarServicoHandler : IRequestHandler<ConsultarServicoQuery, Result<ServicoDto>>
{
    private readonly IServicoRepository _servicoRepository;

    public ConsultarServicoHandler(IServicoRepository servicoRepository)
    {
        _servicoRepository = servicoRepository;
    }

    public async Task<Result<ServicoDto>> Handle(ConsultarServicoQuery request, CancellationToken cancellationToken)
    {
        var servico = await _servicoRepository.GetByIdAsync(request.ServicoId, cancellationToken);

        if (servico is null)
        {
            return Result.NotFound<ServicoDto>("Servico nao encontrado");
        }

        var dto = new ServicoDto(
            servico.Id,
            servico.Nome,
            servico.Descricao,
            servico.PrecoBase,
            servico.TempoEstimadoMinutos,
            servico.Ativo);

        return Result.Success(dto);
    }
}
