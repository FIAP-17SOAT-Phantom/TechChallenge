using MediatR;
using OficinaMecanica.Application.Common.Interfaces;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

/// <summary>
/// Use Case: Aprovar orcamento.
/// Ao aprovar, o domain event OrcamentoAprovadoEvent e emitido.
/// O event handler e responsavel por orquestrar: reserva de pecas + mudanca de status da OS.
/// Isso mantem a separacao entre BCs via eventos (P1 e P2 do Event Storming).
/// </summary>
public sealed class AprovarOrcamentoHandler : IRequestHandler<AprovarOrcamentoCommand, Result>
{
 private readonly IOrcamentoRepository _orcamentoRepository;
 private readonly IUnitOfWork _unitOfWork;

 public AprovarOrcamentoHandler(IOrcamentoRepository orcamentoRepository, IUnitOfWork unitOfWork)
 {
 _orcamentoRepository = orcamentoRepository;
 _unitOfWork = unitOfWork;
 }

 public async Task<Result> Handle(AprovarOrcamentoCommand request, CancellationToken cancellationToken)
 {
 var orcamento = await _orcamentoRepository.GetByIdAsync(request.OrcamentoId, cancellationToken);
 if (orcamento is null)
 return Result.Failure("Orcamento nao encontrado");

 // Transicao de estado no aggregate (emite OrcamentoAprovadoEvent)
 var result = orcamento.Aprovar();
 if (result.IsFailure)
 return result;

 _orcamentoRepository.Update(orcamento);
 await _unitOfWork.SaveChangesAsync(cancellationToken);

 return Result.Success();
 }
}
