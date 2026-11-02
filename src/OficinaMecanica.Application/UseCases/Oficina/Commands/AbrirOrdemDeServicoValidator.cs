using FluentValidation;

namespace OficinaMecanica.Application.UseCases.Oficina.Commands;

public sealed class AbrirOrdemDeServicoValidator : AbstractValidator<AbrirOrdemDeServicoCommand>
{
    public AbrirOrdemDeServicoValidator()
    {
        RuleFor(x => x.Cliente).NotNull().WithMessage("Dados do cliente sao obrigatorios").SetValidator(new DadosClienteAberturaValidator());
        RuleFor(x => x.Veiculo).NotNull().WithMessage("Dados do veiculo sao obrigatorios").SetValidator(new DadosVeiculoAberturaValidator());
        RuleFor(x => x.Servicos).NotNull().WithMessage("Servicos sao obrigatorios").NotEmpty().WithMessage("Informe pelo menos um servico");
        RuleFor(x => x.Servicos).Must(servicos => servicos is null || servicos.Count <= 100).WithMessage("Informe no maximo 100 servicos");
        RuleFor(x => x.Servicos).Must(NaoPossuiServicosRepetidos).WithMessage("Nao e permitido repetir servicos");
        RuleForEach(x => x.Servicos).NotNull().WithMessage("Servico informado e invalido").SetValidator(new ServicoSolicitadoRequestValidator());
        RuleFor(x => x.Pecas).NotNull().WithMessage("A lista de pecas e obrigatoria");
        RuleFor(x => x.Pecas).Must(pecas => pecas is null || pecas.Count <= 100).WithMessage("Informe no maximo 100 pecas");
        RuleFor(x => x.Pecas).Must(NaoPossuiPecasRepetidas).WithMessage("Nao e permitido repetir pecas");
        RuleForEach(x => x.Pecas).NotNull().WithMessage("Peca informada e invalida").SetValidator(new PecaSolicitadaRequestValidator());
    }

    private static bool NaoPossuiServicosRepetidos(IReadOnlyList<ServicoSolicitadoRequest>? servicos) => servicos is null || servicos.All(item => item is not null) && servicos.Select(item => item.ServicoId).Distinct().Count() == servicos.Count;
    private static bool NaoPossuiPecasRepetidas(IReadOnlyList<PecaSolicitadaRequest>? pecas) => pecas is null || pecas.All(item => item is not null) && pecas.Select(item => item.PecaId).Distinct().Count() == pecas.Count;
}

public sealed class DadosClienteAberturaValidator : AbstractValidator<DadosClienteAbertura>
{
    public DadosClienteAberturaValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome e obrigatorio").MaximumLength(200).WithMessage("Nome deve ter no maximo 200 caracteres");
        RuleFor(x => x.Cpf).NotEmpty().WithMessage("CPF e obrigatorio");
        RuleFor(x => x.Telefone).NotEmpty().WithMessage("Telefone e obrigatorio").MaximumLength(20).WithMessage("Telefone deve ter no maximo 20 caracteres");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email e obrigatorio").MaximumLength(255).WithMessage("Email deve ter no maximo 255 caracteres");
    }
}

public sealed class DadosVeiculoAberturaValidator : AbstractValidator<DadosVeiculoAbertura>
{
    public DadosVeiculoAberturaValidator()
    {
        RuleFor(x => x.Placa).NotEmpty().WithMessage("Placa e obrigatoria");
        RuleFor(x => x.Marca).NotEmpty().WithMessage("Marca e obrigatoria").MaximumLength(50).WithMessage("Marca deve ter no maximo 50 caracteres");
        RuleFor(x => x.Modelo).NotEmpty().WithMessage("Modelo e obrigatorio").MaximumLength(50).WithMessage("Modelo deve ter no maximo 50 caracteres");
        RuleFor(x => x.Ano).InclusiveBetween(1900, DateTime.UtcNow.Year + 2).WithMessage("Ano invalido");
    }
}

public sealed class ServicoSolicitadoRequestValidator : AbstractValidator<ServicoSolicitadoRequest>
{
    public ServicoSolicitadoRequestValidator()
    {
        RuleFor(x => x.ServicoId).NotEmpty().WithMessage("ServicoId e obrigatorio");
        RuleFor(x => x.Quantidade).GreaterThan(0).WithMessage("Quantidade do servico deve ser maior que zero");
        RuleFor(x => x.Observacao).MaximumLength(500).WithMessage("Observacao deve ter no maximo 500 caracteres");
    }
}

public sealed class PecaSolicitadaRequestValidator : AbstractValidator<PecaSolicitadaRequest>
{
    public PecaSolicitadaRequestValidator()
    {
        RuleFor(x => x.PecaId).NotEmpty().WithMessage("PecaId e obrigatorio");
        RuleFor(x => x.Quantidade).GreaterThan(0).WithMessage("Quantidade da peca deve ser maior que zero");
        RuleFor(x => x.Observacao).MaximumLength(500).WithMessage("Observacao deve ter no maximo 500 caracteres");
    }
}
