using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Seguranca.Commands;

public sealed class AlterarSenhaValidator : AbstractValidator<AlterarSenhaCommand>
{
    public AlterarSenhaValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("Usuario e obrigatorio");
        RuleFor(x => x.SenhaAtual).NotEmpty().WithMessage("Senha atual e obrigatoria");
        RuleFor(x => x.NovaSenha).NotEmpty().WithMessage("Nova senha e obrigatoria").MinimumLength(8).WithMessage("Nova senha deve ter no minimo 8 caracteres").NotEqual(x => x.SenhaAtual).WithMessage("Nova senha deve ser diferente da senha atual");
    }
}
