using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed class CriarUsuarioValidator : AbstractValidator<CriarUsuarioCommand>
{
    private static readonly string[] Roles = ["Admin", "Atendente", "Mecanico", "Cliente"];

    public CriarUsuarioValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email e obrigatorio").EmailAddress().WithMessage("Email invalido");
        RuleFor(x => x.Role).Must(role => Roles.Contains(role)).WithMessage("Role invalida");
        RuleFor(x => x.ClienteId).NotEmpty().When(x => x.Role == "Cliente").WithMessage("ClienteId e obrigatorio para a role Cliente");
        RuleFor(x => x.ClienteId).Null().When(x => x.Role != "Cliente").WithMessage("ClienteId so pode ser informado para a role Cliente");
    }
}
