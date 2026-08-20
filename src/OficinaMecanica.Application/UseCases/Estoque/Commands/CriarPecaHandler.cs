using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Estoque.Entities;

namespace OficinaMecanica.Application.UseCases.Estoque.Commands;

public sealed class CriarPecaHandler : IRequestHandler<CriarPecaCommand, Result<Guid>>
{
    private readonly IPecaRepository _pecaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CriarPecaHandler(IPecaRepository pecaRepository, IUnitOfWork unitOfWork)
    {
        _pecaRepository = pecaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CriarPecaCommand request, CancellationToken cancellationToken)
    {
        var codigo = request.Codigo.Trim().ToUpperInvariant();
        var existente = await _pecaRepository.GetByCodigoAsync(codigo, cancellationToken);

        if (existente is not null)
        {
            return Result.Conflict<Guid>("Ja existe uma peca cadastrada com este codigo");
        }

        var peca = new Peca(request.Nome, codigo, request.Descricao, request.PrecoUnitario, request.QuantidadeEmEstoque, request.QuantidadeMinima);

        await _pecaRepository.AddAsync(peca, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(peca.Id);
    }
}
