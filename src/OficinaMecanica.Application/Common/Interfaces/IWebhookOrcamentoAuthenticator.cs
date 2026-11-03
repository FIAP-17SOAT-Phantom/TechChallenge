using OficinaMecanica.Domain.Common;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.Common.Interfaces;

public interface IWebhookOrcamentoAuthenticator
{
    Result Validate(string externalEventId, long timestampUnix, Guid orcamentoId, RespostaOrcamentoExterna resposta, string? signature);
}
