using OficinaMecanica.Domain.Common;
using System.Text.RegularExpressions;

namespace OficinaMecanica.Domain.Atendimento.ValueObjects;

public partial class Email : ValueObject
{
    public string Endereco { get; }

    private Email(string endereco) => Endereco = endereco;

    public static Result<Email> Criar(string email)
    {
        var limpo = email?.Trim().ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(limpo))
            return Result.Failure<Email>("Email e obrigatorio");

        if (!PadraoEmail().IsMatch(limpo))
            return Result.Failure<Email>("Email invalido");

        return Result.Success(new Email(limpo));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Endereco;
    }

    public override string ToString() => Endereco;

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.None, 1000)]
    private static partial Regex PadraoEmail();
}
