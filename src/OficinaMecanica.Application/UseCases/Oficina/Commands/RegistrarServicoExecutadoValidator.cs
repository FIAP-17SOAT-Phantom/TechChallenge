using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class RegistrarServicoExecutadoValidator : AbstractValidator<RegistrarServicoExecutadoCommand>
{
    public RegistrarServicoExecutadoValidator()
    {
        RuleFor(x => x.OrdemDeServicoId).NotEmpty().WithMessage("Ordem de Servico e obrigatoria");
        RuleFor(x => x.ServicoId).NotEmpty().WithMessage("Servico e obrigatorio");
    }
}
