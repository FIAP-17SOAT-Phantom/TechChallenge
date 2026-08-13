using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class CriarOrdemDeServicoValidator : AbstractValidator<CriarOrdemDeServicoCommand>
{
 public CriarOrdemDeServicoValidator()
 {
 RuleFor(x => x.ClienteId)
 .NotEmpty().WithMessage("ClienteId e obrigatorio");

 RuleFor(x => x.VeiculoId)
 .NotEmpty().WithMessage("VeiculoId e obrigatorio");
 }
}
