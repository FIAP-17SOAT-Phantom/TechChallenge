using OficinaMecanica.Domain.Common;
using System.Text.RegularExpressions;

namespace OficinaMecanica.Domain.Atendimento.ValueObjects;

public partial class Placa : ValueObject
{
    public string Valor { get; }

    private Placa(string valor) => Valor = valor;

    public static Result<Placa> Criar(string placa)
    {
        var limpa = placa?.Trim().ToUpper() ?? "";

        if (!PadraoAntigo().IsMatch(limpa) && !PadraoMercosul().IsMatch(limpa))
            return Result.Failure<Placa>("Placa invalida. Use formato ABC-1234 ou ABC1D23");

        return Result.Success(new Placa(limpa.Replace("-", "")));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;

    [GeneratedRegex(@"^[A-Z]{3}-?\d{4}$", RegexOptions.None, 1000)]
    private static partial Regex PadraoAntigo();

    [GeneratedRegex(@"^[A-Z]{3}\d[A-Z]\d{2}$", RegexOptions.None, 1000)]
    private static partial Regex PadraoMercosul();
}
