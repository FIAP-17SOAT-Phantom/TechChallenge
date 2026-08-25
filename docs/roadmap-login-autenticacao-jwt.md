# Roadmap do Login e da Autenticacao JWT

## 1. Objetivo

Este documento mostra o caminho completo do login na API, desde a requisicao HTTP ate a consulta ao PostgreSQL e a emissao do JWT. Tambem indica os arquivos envolvidos e a ordem recomendada de breakpoints para acompanhar o processo no Visual Studio.

## 2. Visao geral do login

```text
Swagger ou frontend
        |
        | POST /api/auth/login
        v
AutenticacaoController.Login
        |
        | IMediator.Send(AutenticarCommand)
        v
ValidationBehavior
        |
        | AutenticarValidator
        v
AutenticarHandler
        |
        | IIdentityService.AutenticarAsync
        v
IdentityService
        |
        | UserManager<UsuarioSistema>
        v
AppDbContext + ASP.NET Core Identity
        |
        v
PostgreSQL - AspNetUsers, AspNetRoles e AspNetUserRoles
        |
        | usuario e senha validos
        v
IdentityService monta claims e assina o JWT
        |
        v
TokenAcessoDto -> Handler -> Controller -> HTTP 200
```

## 3. Requisicao de login

O cliente chama:

```http
POST /api/auth/login
Content-Type: application/json
```

Corpo:

```json
{
  "email": "admin@oficina.local",
  "senha": "senha-configurada-no-env"
}
```

O endpoint possui `[AllowAnonymous]`, pois ainda nao existe um token antes do login. As demais rotas continuam protegidas pela politica global de autorizacao.

## 4. Etapa 1 - Controller

Arquivo:

```text
src/OficinaMecanica.API/Controllers/AutenticacaoController.cs
```

Metodo executado:

```csharp
public async Task<IActionResult> Login(AutenticarCommand command, CancellationToken cancellationToken)
```

O ASP.NET Core converte automaticamente o JSON em `AutenticarCommand`. O Controller nao consulta o banco e nao valida senha. Ele apenas encaminha o comando:

```csharp
var result = await _mediator.Send(command, cancellationToken);
```

Se ocorrer uma falha conhecida, `ToProblem` converte o `Result` em `ProblemDetails`. Se o login funcionar, o Controller devolve `200 OK` com `TokenAcessoDto`.

## 5. Etapa 2 - Command

Arquivo:

```text
src/OficinaMecanica.Application/UseCases/Seguranca/Commands/AutenticarCommand.cs
```

O comando representa a intencao de autenticar:

```csharp
public sealed record AutenticarCommand(string Email, string Senha) : IRequest<Result<TokenAcessoDto>>;
```

Ele possui os dados de entrada e declara que o processamento devolve um `Result<TokenAcessoDto>`.

## 6. Etapa 3 - Validacao pelo pipeline

Arquivos:

```text
src/OficinaMecanica.Application/Common/Behaviors/ValidationBehavior.cs
src/OficinaMecanica.Application/UseCases/Seguranca/Commands/AutenticarValidator.cs
src/OficinaMecanica.Application/DependencyInjection.cs
```

O MediatR foi configurado com `ValidationBehavior`. Antes de chamar o Handler, o pipeline encontra o `AutenticarValidator` e verifica:

- email obrigatorio;
- formato valido de email;
- senha obrigatoria.

Se a entrada for invalida, o Handler nao e executado. A excecao de validacao chega ao tratamento global e vira uma resposta HTTP `400` padronizada.

## 7. Etapa 4 - Handler

Arquivo:

```text
src/OficinaMecanica.Application/UseCases/Seguranca/Commands/AutenticarHandler.cs
```

O MediatR associa automaticamente `AutenticarCommand` a `AutenticarHandler`. O Handler depende de `IIdentityService`, e nao de uma implementacao ou do banco:

```csharp
return await _identityService.AutenticarAsync(request.Email, request.Senha, cancellationToken);
```

Essa dependencia preserva a Clean Architecture: Application conhece um contrato, enquanto Infrastructure fornece a implementacao.

## 8. Etapa 5 - Contrato de identidade

Arquivo:

```text
src/OficinaMecanica.Application/Common/Interfaces/IIdentityService.cs
```

O contrato define autenticacao, criacao de usuario, troca de senha, bloqueio, redefinicao de senha e consultas administrativas.

O resultado do login e:

```csharp
public sealed record TokenAcessoDto(string Token, DateTime ExpiraEm, string Email, IReadOnlyList<string> Roles, bool TrocaSenhaObrigatoria);
```

## 9. Etapa 6 - IdentityService e PostgreSQL

Arquivos:

```text
src/OficinaMecanica.Infrastructure/Identity/IdentityService.cs
src/OficinaMecanica.Infrastructure/Identity/UsuarioSistema.cs
src/OficinaMecanica.Infrastructure/Persistence/AppDbContext.cs
```

A implementacao utiliza `UserManager<UsuarioSistema>` do ASP.NET Core Identity.

Primeiro, procura o usuario pelo email:

```csharp
var usuario = await _userManager.FindByEmailAsync(email);
```

Depois verifica:

- se o usuario existe;
- se esta bloqueado;
- se o hash da senha corresponde a senha informada.

```csharp
await _userManager.CheckPasswordAsync(usuario, senha)
```

A senha original nao e armazenada no banco. O Identity armazena um hash em `AspNetUsers.PasswordHash` e realiza a comparacao de forma segura.

Quando email ou senha estiverem incorretos, a API devolve uma mensagem generica:

```text
Email ou senha invalidos
```

Nao informar qual dos dois campos falhou reduz a exposicao de usuarios cadastrados.

## 10. Tabelas utilizadas no login

| Tabela | Finalidade |
|--------|------------|
| `AspNetUsers` | Usuario, email, hash da senha, bloqueio, `ClienteId` e troca obrigatoria |
| `AspNetRoles` | Roles disponiveis |
| `AspNetUserRoles` | Relacao entre usuario e role |
| `AspNetUserClaims` | Claims persistidas, quando utilizadas |
| `AspNetUserTokens` | Tokens internos do Identity, como operacoes de redefinicao |

O JWT emitido pela API nao precisa ser salvo nessas tabelas. Ele e autocontido e validado por assinatura, emissor, audiencia e prazo de expiracao.

## 11. Etapa 7 - Criacao do JWT

Depois de validar as credenciais, `IdentityService` consulta as roles e monta as claims:

| Claim | Conteudo |
|-------|----------|
| `sub` | ID do usuario |
| `email` | Email do usuario |
| `NameIdentifier` | ID usado pelos Controllers |
| `Role` | `Admin`, `Atendente`, `Mecanico` ou `Cliente` |
| `cliente_id` | ID do cliente vinculado, quando aplicavel |
| `troca_senha_obrigatoria` | Indica primeiro acesso ou senha redefinida |

O token recebe ainda:

- `Issuer`: quem emitiu;
- `Audience`: para quem foi emitido;
- `Expires`: quando deixa de ser valido;
- assinatura HMAC SHA-256 baseada em `Jwt:Secret`.

O segredo assina e valida o token. Ele nunca deve ser colocado no JSON de login, no Swagger, no frontend ou no Git.

## 12. Resposta do login

Exemplo conceitual:

```json
{
  "token": "eyJ...",
  "expiraEm": "2026-08-25T15:00:00Z",
  "email": "admin@oficina.local",
  "roles": [
    "Admin"
  ],
  "trocaSenhaObrigatoria": false
}
```

O frontend deve manter o token apenas pelo tempo necessario e envia-lo nas chamadas protegidas:

```http
Authorization: Bearer eyJ...
```

## 13. Como o token e validado nas proximas requisicoes

O fluxo muda depois do login:

```text
Requisicao com Authorization: Bearer TOKEN
        |
        v
JwtBearer middleware
        |
        | valida assinatura, issuer, audience e expiracao
        v
ClaimsPrincipal em HttpContext.User
        |
        v
Middleware de estado do usuario
        |
        | consulta bloqueio e troca obrigatoria
        v
Authorization middleware
        |
        | verifica [Authorize] e Roles
        v
Controller autorizado
```

Essa configuracao fica em:

```text
src/OficinaMecanica.API/Program.cs
```

O `AddJwtBearer` configura a validacao criptografica. `UseAuthentication` identifica o usuario e `UseAuthorization` verifica as regras de acesso.

O projeto define uma `FallbackPolicy` que exige autenticacao por padrao. Assim, uma rota nova fica protegida mesmo que o desenvolvedor esqueca `[Authorize]`. Somente endpoints marcados explicitamente com `[AllowAnonymous]`, como o login, ficam publicos.

## 14. Verificacao de usuario ativo e senha temporaria

Entre autenticacao e autorizacao, `Program.cs` executa um middleware proprio. Ele extrai o ID da claim `NameIdentifier` e chama:

```text
IIdentityService.ObterEstadoAcessoAsync
```

Esse middleware verifica novamente no banco:

- se o usuario ainda existe;
- se continua ativo;
- se precisa trocar a senha.

Consequencias:

- usuario desativado recebe `401` mesmo que possua um JWT ainda nao expirado;
- usuario com senha temporaria recebe `403` nas rotas comuns;
- a rota `/api/auth/alterar-senha` continua liberada para concluir o primeiro acesso.

Essa consulta a cada requisicao permite revogar o acesso imediatamente, mas adiciona uma consulta ao banco por chamada autenticada.

## 15. Roles e Controllers

Exemplos de protecao:

```csharp
[Authorize(Roles = "Admin")]
```

Somente administradores.

```csharp
[Authorize(Roles = "Admin,Atendente")]
```

Administrador ou atendente.

```csharp
[Authorize(Roles = "Cliente")]
```

Somente cliente autenticado. Nessas rotas, a claim `cliente_id` limita a consulta aos dados pertencentes ao proprio cliente.

## 16. Primeiro acesso de um novo usuario

```text
Admin autenticado
    -> POST /api/auth/usuarios
    -> sistema gera senha temporaria aleatoria
    -> UsuarioSistema.DeveAlterarSenha = true
    -> senha temporaria aparece apenas na resposta de criacao
    -> usuario faz POST /api/auth/login
    -> recebe JWT com TrocaSenhaObrigatoria = true
    -> demais endpoints retornam 403
    -> POST /api/auth/alterar-senha
    -> Identity troca o hash da senha
    -> DeveAlterarSenha = false
    -> usuario faz login novamente
    -> recebe acesso normal
```

O administrador tambem pode redefinir uma senha. Nesse caso, outra senha temporaria e gerada e a troca volta a ser obrigatoria.

## 17. Criacao do administrador inicial

Arquivo:

```text
src/OficinaMecanica.Infrastructure/Identity/IdentitySeeder.cs
```

Durante a inicializacao da API:

```text
Program.cs
    -> SeedIdentityAsync
    -> aplica migrations
    -> cria roles ausentes
    -> procura o administrador pelo email
    -> cria o administrador se ainda nao existir
    -> adiciona a role Admin
```

As configuracoes chegam por:

```text
ADMIN_EMAIL
ADMIN_PASSWORD
```

No Docker, o Compose converte essas variaveis em `Authentication__SeedUsers__0__Email` e `Authentication__SeedUsers__0__Password`.

O seed nao recria o usuario nem troca sua senha a cada inicializacao. Se o usuario ja existe, apenas garante que ele possui a role configurada.

## 18. Configuracao do JWT

Arquivos:

```text
.env
.env.example
docker-compose.yml
src/OficinaMecanica.API/appsettings.json
src/OficinaMecanica.API/Configuration/DotEnvLoader.cs
src/OficinaMecanica.API/Program.cs
src/OficinaMecanica.Infrastructure/Identity/JwtOptions.cs
src/OficinaMecanica.Infrastructure/DependencyInjection.cs
```

O `appsettings.json` define valores nao secretos:

```json
{
  "Jwt": {
    "Issuer": "OficinaMecanica.API",
    "Audience": "OficinaMecanica.Client",
    "ExpirationMinutes": 60
  }
}
```

O `.env` fornece `JWT_SECRET`. Na execucao local, `DotEnvLoader` converte:

```text
JWT_SECRET -> Jwt__Secret -> Jwt:Secret
```

No Docker, `docker-compose.yml` realiza a mesma injecao diretamente no ambiente do container.

O segredo precisa ter no minimo 32 bytes. A API interrompe a inicializacao quando ele esta ausente ou e muito curto.

## 19. Registro das dependencias

Na inicializacao:

```text
Program.cs
    -> AddApplication()
       -> registra MediatR
       -> registra Handlers
       -> registra FluentValidation
       -> registra ValidationBehavior
    -> AddInfrastructure(configuration)
       -> registra AppDbContext com Npgsql
       -> registra ASP.NET Core Identity
       -> registra IIdentityService como IdentityService
       -> carrega JwtOptions
    -> AddAuthentication().AddJwtBearer()
    -> AddAuthorization()
```

Por causa da injecao de dependencia, `AutenticarHandler` solicita `IIdentityService` e recebe uma instancia de `IdentityService` automaticamente.

## 20. Ordem recomendada de breakpoints

Para acompanhar um login pelo Visual Studio, mantenha o PostgreSQL no Docker, execute a API localmente e coloque breakpoints nesta ordem:

1. `AutenticacaoController.Login`;
2. `ValidationBehavior.Handle`;
3. construtor ou regras de `AutenticarValidator`;
4. `AutenticarHandler.Handle`;
5. `IdentityService.AutenticarAsync`;
6. linha de `FindByEmailAsync`;
7. linha de `CheckPasswordAsync`;
8. consulta de roles;
9. criacao da lista de claims;
10. criacao do `JwtSecurityToken`;
11. retorno de `TokenAcessoDto`;
12. retorno `Ok` no Controller.

Para acompanhar uma rota protegida, coloque breakpoints em:

1. middleware personalizado de `Program.cs`, depois de `UseAuthentication`;
2. `IdentityService.ObterEstadoAcessoAsync`;
3. metodo do Controller escolhido;
4. Handler chamado pelo Controller.

O `JwtBearer` e o middleware de autorizacao pertencem ao framework. Para o estudo inicial, basta inspecionar `HttpContext.User`, `User.Claims` e `User.Identity.IsAuthenticated` no middleware ou Controller.

## 21. Resultado por tipo de falha

| Situacao | Resposta esperada |
|----------|-------------------|
| JSON sem email ou senha | `400 Bad Request` |
| Email ou senha incorretos | `401 Unauthorized` |
| Token ausente em rota protegida | `401 Unauthorized` |
| Token invalido ou expirado | `401 Unauthorized` |
| Usuario desativado | `401 Unauthorized` |
| Role sem permissao | `403 Forbidden` |
| Senha temporaria ainda nao trocada | `403 Forbidden`, exceto na troca de senha |
| Login valido | `200 OK` com JWT |

## 22. Resumo dos arquivos principais

| Ordem | Arquivo | Papel |
|-------|---------|-------|
| 1 | `API/Controllers/AutenticacaoController.cs` | Recebe login e devolve HTTP |
| 2 | `Application/.../AutenticarCommand.cs` | Dados e tipo do caso de uso |
| 3 | `Application/.../AutenticarValidator.cs` | Valida entrada |
| 4 | `Application/.../AutenticarHandler.cs` | Coordena o caso de uso |
| 5 | `Application/Common/Interfaces/IIdentityService.cs` | Contrato independente da Infrastructure |
| 6 | `Infrastructure/Identity/IdentityService.cs` | Valida usuario e gera JWT |
| 7 | `Infrastructure/Identity/UsuarioSistema.cs` | Modelo de usuario do Identity |
| 8 | `Infrastructure/Persistence/AppDbContext.cs` | Acesso EF Core ao PostgreSQL |
| 9 | `Infrastructure/Identity/IdentitySeeder.cs` | Migrations, roles e administrador inicial |
| 10 | `API/Program.cs` | DI, JWT, middlewares e autorizacao |

Esse percurso preserva a direcao da arquitetura:

```text
API -> Application -> Domain
Infrastructure -> Application / Domain
```

A Application conhece apenas `IIdentityService`; detalhes como Identity, hash de senha, EF Core, PostgreSQL e assinatura JWT permanecem na Infrastructure e na composicao da API.
