using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OficinaMecanica.API.OpenApi;

public sealed class ApiDocumentationOperationFilter : IOperationFilter
{
    private static readonly IReadOnlyDictionary<string, string> Summaries = new Dictionary<string, string>
    {
        ["Login"] = "Autentica o usuario e retorna um token JWT",
        ["CriarUsuario"] = "Cria um usuario com senha temporaria",
        ["AlterarSenha"] = "Altera a senha do usuario autenticado",
        ["RedefinirSenhaUsuario"] = "Gera uma nova senha temporaria",
        ["ListarMinhasOrdensDeServico"] = "Lista as ordens do cliente autenticado",
        ["ConsultarMinhaOrdemDeServico"] = "Consulta uma ordem do cliente autenticado",
        ["ListarMeusOrcamentos"] = "Lista os orcamentos do cliente autenticado",
        ["ConsultarMeuOrcamento"] = "Consulta um orcamento do cliente autenticado",
        ["RegistrarDiagnostico"] = "Registra diagnostico, servicos e pecas sugeridos",
        ["RegistrarServicoExecutado"] = "Marca um servico aprovado como executado",
        ["Aprovar"] = "Aprova o orcamento, reserva estoque e inicia a execucao",
        ["Finalizar"] = "Consome as reservas e finaliza a ordem de servico"
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
        {
            return;
        }

        var actionName = actionDescriptor.ActionName;
        operation.Summary = Summaries.TryGetValue(actionName, out var summary) ? summary : SepararNome(actionName);
        operation.Description = ObterDescricaoAutorizacao(actionDescriptor);
        operation.Responses.TryAdd("400", CriarResposta("Requisicao invalida ou regra de negocio nao atendida", context));

        var method = context.ApiDescription.HttpMethod;
        var rotaComIdentificador = context.ApiDescription.RelativePath?.Contains('{') == true;

        if (rotaComIdentificador)
        {
            operation.Responses.TryAdd("404", CriarResposta("Recurso nao encontrado", context));
        }

        if (method is "POST" or "PUT" or "PATCH" or "DELETE")
        {
            operation.Responses.TryAdd("409", CriarResposta("Conflito de estado, vinculo, duplicidade ou estoque", context));
        }

        var permiteAnonimo = actionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();

        if (!permiteAnonimo)
        {
            operation.Responses.TryAdd("401", CriarResposta("Token ausente, invalido ou usuario inativo", context));
            operation.Responses.TryAdd("403", CriarResposta("Usuario sem permissao ou com troca de senha obrigatoria", context));
        }

        AplicarExemplo(operation, actionName);
    }

    private static OpenApiResponse CriarResposta(string description, OperationFilterContext context) => new() { Description = description, Content = { ["application/problem+json"] = new OpenApiMediaType { Schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository) } } };

    private static string ObterDescricaoAutorizacao(ControllerActionDescriptor actionDescriptor)
    {
        if (actionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
        {
            return "Acesso anonimo.";
        }

        var roles = actionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().SelectMany(attribute => (attribute.Roles ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).Distinct().ToList();

        return roles.Count == 0 ? "Requer usuario autenticado." : $"Roles permitidas: {string.Join(", ", roles)}.";
    }

    private static string SepararNome(string value) => System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");

    private static void AplicarExemplo(OpenApiOperation operation, string actionName)
    {
        if (operation.RequestBody?.Content.TryGetValue("application/json", out var mediaType) != true || mediaType is null)
        {
            return;
        }

        mediaType.Example = actionName switch
        {
            "Login" => new OpenApiObject { ["email"] = new OpenApiString("usuario@oficina.com"), ["senha"] = new OpenApiString("Senha@123") },
            "CriarUsuario" => new OpenApiObject { ["email"] = new OpenApiString("cliente@email.com"), ["role"] = new OpenApiString("Cliente"), ["clienteId"] = new OpenApiString("00000000-0000-0000-0000-000000000000") },
            "AlterarSenha" => new OpenApiObject { ["senhaAtual"] = new OpenApiString("senha-temporaria"), ["novaSenha"] = new OpenApiString("NovaSenha@123") },
            "Criar" when operation.Tags.Any(tag => tag.Name == "Clientes") => new OpenApiObject { ["nome"] = new OpenApiString("Cliente Exemplo"), ["cpf"] = new OpenApiString("52998224725"), ["telefone"] = new OpenApiString("11999999999"), ["email"] = new OpenApiString("cliente@email.com") },
            _ => mediaType.Example
        };
    }
}
