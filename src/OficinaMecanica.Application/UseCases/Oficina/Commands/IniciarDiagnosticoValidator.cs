using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class IniciarDiagnosticoValidator : AbstractValidator<IniciarDiagnosticoCommand>
{
    public IniciarDiagnosticoValidator()
    {
        RuleFor(x => x.OrdemDeServicoId).NotEmpty().WithMessage("Ordem de Servico e obrigatoria");
        RuleFor(x => x.MecanicoId).NotEmpty().WithMessage("Mecanico e obrigatorio");
    }
}
