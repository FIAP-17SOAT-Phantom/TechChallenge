using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class RegistrarDiagnosticoValidator : AbstractValidator<RegistrarDiagnosticoCommand>
{
    public RegistrarDiagnosticoValidator()
    {
        RuleFor(x => x.OrdemDeServicoId).NotEmpty().WithMessage("Ordem de Servico e obrigatoria");
        RuleFor(x => x.Diagnostico).NotEmpty().WithMessage("Diagnostico e obrigatorio").MaximumLength(2000).WithMessage("Diagnostico deve ter no maximo 2000 caracteres");
        RuleFor(x => x.Itens).NotEmpty().WithMessage("Ao menos um item e obrigatorio");
        RuleForEach(x => x.Itens).SetValidator(new ItemDiagnosticoValidator());
    }
}

public sealed class ItemDiagnosticoValidator : AbstractValidator<ItemDiagnosticoRequest>
{
    public ItemDiagnosticoValidator()
    {
        RuleFor(x => x.ServicoId).NotEmpty().WithMessage("Servico e obrigatorio");
        RuleFor(x => x.Quantidade).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");
        RuleFor(x => x.Observacao).MaximumLength(500).WithMessage("Observacao deve ter no maximo 500 caracteres");
    }
}
