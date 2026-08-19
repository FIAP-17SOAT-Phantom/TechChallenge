using OficinaMecanica.Domain.Atendimento.ValueObjects;
using OficinaMecanica.Domain.Common;

namespace OficinaMecanica.Domain.Atendimento.Entities;

public class Cliente : AggregateRoot
{
    public string Nome { get; private set; } = null!;
    public Cpf Cpf { get; private set; } = null!;
    public string Telefone { get; private set; } = null!;
    public Email Email { get; private set; } = null!;

    private Cliente() { } // EF Core

    public Cliente(string nome, Cpf cpf, string telefone, Email email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome e obrigatorio");

        Nome = nome;
        Cpf = cpf;
        Telefone = telefone;
        Email = email;
    }

    public void Atualizar(string nome, string telefone, Email email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome e obrigatorio");

        Nome = nome;
        Telefone = telefone;
        Email = email;
    }
}
