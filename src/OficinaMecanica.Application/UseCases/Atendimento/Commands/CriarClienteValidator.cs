using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Atendimento.Commands;

public sealed class CriarClienteValidator : AbstractValidator<CriarClienteCommand>
{
 public CriarClienteValidator()
 {
 RuleFor(x => x.Nome)
 .NotEmpty().WithMessage("Nome e obrigatorio")
 .MaximumLength(200).WithMessage("Nome deve ter no maximo 200 caracteres");

 RuleFor(x => x.Cpf)
 .NotEmpty().WithMessage("CPF e obrigatorio");

 RuleFor(x => x.Telefone)
 .NotEmpty().WithMessage("Telefone e obrigatorio");

 RuleFor(x => x.Email)
 .NotEmpty().WithMessage("Email e obrigatorio");
 }
}
