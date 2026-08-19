using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Estoque.Queries;

public sealed class ConsultarPecaHandler : IRequestHandler<ConsultarPecaQuery, Result<PecaDto>>
{
    private readonly IPecaRepository _pecaRepository;

    public ConsultarPecaHandler(IPecaRepository pecaRepository)
    {
        _pecaRepository = pecaRepository;
    }

    public async Task<Result<PecaDto>> Handle(ConsultarPecaQuery request, CancellationToken cancellationToken)
    {
        var peca = await _pecaRepository.GetByIdAsync(request.PecaId, cancellationToken);

        if (peca is null)
        {
            return Result.Failure<PecaDto>("Peca nao encontrada");
        }

        var dto = new PecaDto(peca.Id, peca.Nome, peca.Codigo, peca.Descricao, peca.PrecoUnitario, peca.QuantidadeEmEstoque, peca.QuantidadeReservada, peca.QuantidadeDisponivel, peca.QuantidadeMinima);

        return Result.Success(dto);
    }
}
