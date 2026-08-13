using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Atendimento.ValueObjects;

public class Cpf : ValueObject
{
 public string Numero { get; }

 private Cpf(string numero) => Numero = numero;

 public static Result<Cpf> Criar(string numero)
 {
 var limpo = numero?.Replace(".", "").Replace("-", "").Trim() ?? "";

 if (limpo.Length != 11)
 return Result.Failure<Cpf>("CPF deve ter 11 digitos");

 if (limpo.Distinct().Count() == 1)
 return Result.Failure<Cpf>("CPF invalido");

 if (!ValidarDigitos(limpo))
 return Result.Failure<Cpf>("CPF invalido - digito verificador incorreto");

 return Result.Success(new Cpf(limpo));
 }

 private static bool ValidarDigitos(string cpf)
 {
 var multiplicador1 = new int[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
 var multiplicador2 = new int[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

 var tempCpf = cpf[..9];
 var soma = 0;

 for (int i = 0; i < 9; i++)
 soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

 var resto = soma % 11;
 var digito = resto < 2 ? 0 : 11 - resto;
 tempCpf += digito;

 soma = 0;
 for (int i = 0; i < 10; i++)
 soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

 resto = soma % 11;
 digito = resto < 2 ? 0 : 11 - resto;
 tempCpf += digito;

 return cpf.EndsWith(tempCpf[9..]);
 }

 protected override IEnumerable<object?> GetEqualityComponents()
 {
 yield return Numero;
 }

 public override string ToString() => Numero;
}
