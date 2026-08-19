using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed class AutenticarValidator : AbstractValidator<AutenticarCommand>
{
    public AutenticarValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email e obrigatorio").EmailAddress().WithMessage("Email invalido");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Senha e obrigatoria");
    }
}
