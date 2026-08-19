using OficinaMecanica.Domain.Common;
using System.Text.RegularExpressions;

namespace OficinaMecanica.Domain.Atendimento.ValueObjects;

public class Email : ValueObject
{
    public string Endereco { get; }

    private Email(string endereco) => Endereco = endereco;

    public static Result<Email> Criar(string email)
    {
        var limpo = email?.Trim().ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(limpo))
            return Result.Failure<Email>("Email e obrigatorio");

        var padrao = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        if (!padrao.IsMatch(limpo))
            return Result.Failure<Email>("Email invalido");

        return Result.Success(new Email(limpo));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Endereco;
    }

    public override string ToString() => Endereco;
}
