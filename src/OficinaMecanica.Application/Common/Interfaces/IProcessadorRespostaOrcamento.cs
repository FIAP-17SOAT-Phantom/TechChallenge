using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IProcessadorRespostaOrcamento
{
    Task<Result> ProcessAsync(Guid orcamentoId, RespostaOrcamentoExterna resposta, CancellationToken cancellationToken = default);
}
