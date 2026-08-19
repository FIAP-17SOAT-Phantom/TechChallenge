using MediatR;
using OficinaMecanica.Application.Common.Interfaces;

namespace OficinaMecanica.Application.UseCases.Estoque.Queries;

public sealed class ListarPecasHandler : IRequestHandler<ListarPecasQuery, IReadOnlyList<PecaDto>>
{
    private readonly IPecaRepository _pecaRepository;

    public ListarPecasHandler(IPecaRepository pecaRepository)
    {
        _pecaRepository = pecaRepository;
    }

    public async Task<IReadOnlyList<PecaDto>> Handle(ListarPecasQuery request, CancellationToken cancellationToken)
    {
        var pecas = request.SomenteEstoqueBaixo
            ? await _pecaRepository.GetComEstoqueBaixoAsync(cancellationToken)
            : await _pecaRepository.GetAllAsync(cancellationToken);

        return pecas.Select(peca => new PecaDto(peca.Id, peca.Nome, peca.Codigo, peca.Descricao, peca.PrecoUnitario, peca.QuantidadeEmEstoque, peca.QuantidadeReservada, peca.QuantidadeDisponivel, peca.QuantidadeMinima)).ToList();
    }
}
