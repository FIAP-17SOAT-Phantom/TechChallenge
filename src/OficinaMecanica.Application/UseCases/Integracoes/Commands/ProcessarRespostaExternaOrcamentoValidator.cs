using FluentValidation;
using OficinaMecanica.Domain.Orcamentacao.Enums;

namespace OficinaMecanica.Application.UseCases.Integracoes.Commands;

public sealed class ProcessarRespostaExternaOrcamentoValidator : AbstractValidator<ProcessarRespostaExternaOrcamentoCommand>
{
    public ProcessarRespostaExternaOrcamentoValidator()
    {
        RuleFor(command => command.OrcamentoId).NotEmpty().WithMessage("OrcamentoId e obrigatorio");
        RuleFor(command => command.ExternalEventId).NotEmpty().WithMessage("ExternalEventId e obrigatorio").MaximumLength(100).WithMessage("ExternalEventId deve ter no maximo 100 caracteres").Must(value => value == value.Trim()).WithMessage("ExternalEventId nao deve possuir espacos nas extremidades");
        RuleFor(command => command.Resposta).NotEmpty().WithMessage("Resposta e obrigatoria").Must(IsValidResponse).WithMessage("Resposta deve ser Aprovado ou Rejeitado");
        RuleFor(command => command.TimestampUnix).GreaterThan(0).WithMessage("TimestampUnix deve ser valido");
    }

    private static bool IsValidResponse(string resposta) => Enum.TryParse<RespostaOrcamentoExterna>(resposta, true, out var parsed) && Enum.IsDefined(parsed);
}
