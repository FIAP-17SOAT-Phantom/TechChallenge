using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Orcamentacao.Commands;

public sealed class GerarOrcamentoValidator : AbstractValidator<GerarOrcamentoCommand>
{
    public GerarOrcamentoValidator()
    {
        RuleFor(x => x.OrdemDeServicoId).NotEmpty().WithMessage("Ordem de Servico e obrigatoria");
        RuleFor(x => x.Observacao).MaximumLength(1000).WithMessage("Observacao deve ter no maximo 1000 caracteres");
    }
}
