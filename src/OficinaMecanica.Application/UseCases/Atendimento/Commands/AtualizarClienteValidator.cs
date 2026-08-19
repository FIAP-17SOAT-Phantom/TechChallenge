using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class AtualizarClienteValidator : AbstractValidator<AtualizarClienteCommand>
{
    public AtualizarClienteValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("Cliente e obrigatorio");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome e obrigatorio")
            .MaximumLength(200).WithMessage("Nome deve ter no maximo 200 caracteres");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone e obrigatorio")
            .MaximumLength(20).WithMessage("Telefone deve ter no maximo 20 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email e obrigatorio")
            .MaximumLength(255).WithMessage("Email deve ter no maximo 255 caracteres");
    }
}
