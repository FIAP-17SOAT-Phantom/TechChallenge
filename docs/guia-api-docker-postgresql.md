# Guia da API, Docker e PostgreSQL

## 1. Visao geral

Este projeto possui dois processos principais:

```text
Navegador, Swagger ou frontend
              |
              | HTTP - localhost:8080
              v
      API ASP.NET Core 8
              |
              | PostgreSQL - db:5432
              v
       Banco PostgreSQL 16
```

Quando o projeto e executado com Docker Compose, a API e o PostgreSQL rodam em containers separados. O Docker cria uma rede privada para que eles consigam conversar e cria um volume para preservar os arquivos do banco.

## 2. Como a API funciona

A API recebe uma requisicao HTTP por um Controller e encaminha o caso de uso pelo MediatR. O Handler executa a operacao utilizando as regras do Domain e as interfaces de repositorio. A Infrastructure implementa os repositorios e utiliza o Entity Framework Core para ler e gravar no PostgreSQL.

```text
Requisicao HTTP
    -> Controller
    -> MediatR
    -> Command ou Query
    -> Handler
    -> Domain e repositorio
    -> Entity Framework Core
    -> PostgreSQL
```

Responsabilidades dos projetos:

| Projeto | Responsabilidade |
|---------|------------------|
| `OficinaMecanica.API` | Endpoints, JWT, autorizacao, Swagger e tratamento de erros |
| `OficinaMecanica.Application` | Commands, Queries, Handlers, validacoes e contratos |
| `OficinaMecanica.Domain` | Entidades, Value Objects, eventos e regras de negocio |
| `OficinaMecanica.Infrastructure` | Entity Framework Core, PostgreSQL, Identity, repositorios e migrations |
| `OficinaMecanica.Tests` | Testes automatizados de Domain e Application |

### Exemplo de uma requisicao

Ao enviar um cadastro de cliente:

```text
POST /api/clientes
    -> ClientesController
    -> CriarClienteCommand
    -> CriarClienteCommandHandler
    -> Cliente e suas regras de dominio
    -> ClienteRepository
    -> AppDbContext
    -> tabela Clientes no PostgreSQL
```

O Controller nao acessa diretamente o PostgreSQL. Essa separacao reduz o acoplamento e permite testar as regras sem iniciar a API ou o banco.

## 3. Autenticacao e autorizacao

O endpoint `POST /api/auth/login` valida o usuario armazenado nas tabelas do ASP.NET Core Identity. Quando as credenciais estao corretas, a API emite um token JWT.

O cliente envia esse token nas proximas requisicoes:

```http
Authorization: Bearer TOKEN_JWT
```

O JWT identifica o usuario e sua role, como `Admin`, `Atendente`, `Mecanico` ou `Cliente`. A API usa essas informacoes para permitir ou negar cada operacao. O segredo utilizado para assinar o token vem de `JWT_SECRET` no arquivo `.env` e nao deve ser enviado ao Git.

## 4. O que o Docker faz

Docker executa aplicacoes em ambientes isolados chamados containers. Um container nao e uma maquina virtual completa: ele empacota o processo, suas bibliotecas e sua configuracao de execucao.

Neste projeto, o arquivo `docker-compose.yml` coordena dois servicos:

| Servico | Container | Funcao |
|---------|-----------|--------|
| `api` | API .NET 8 | Receber requisicoes e executar os casos de uso |
| `db` | PostgreSQL 16 Alpine | Armazenar os dados da aplicacao |

O Compose tambem cria automaticamente:

- uma rede interna entre `api` e `db`;
- o volume `postgres_data`;
- o mapeamento da porta `8080` para a API;
- o mapeamento da porta `5432` para o PostgreSQL.

### Imagem e container

A imagem e o modelo imutavel usado para iniciar um container. O container e a instancia em execucao dessa imagem.

Para a API, a imagem e produzida pelo `Dockerfile`. Ele restaura os pacotes, compila, publica a aplicacao e usa a imagem de runtime do ASP.NET Core 8 para executa-la.

Para o banco, nao foi necessario criar um Dockerfile. O Compose utiliza diretamente a imagem oficial:

```yaml
image: postgres:16-alpine
```

## 5. Como o PostgreSQL e configurado no Docker

O servico `db` possui esta configuracao:

```yaml
db:
  image: postgres:16-alpine
  ports:
    - "5432:5432"
  environment:
    POSTGRES_DB: oficina_mecanica
    POSTGRES_USER: oficina
    POSTGRES_PASSWORD: oficina_dev
  healthcheck:
    test: ["CMD-SHELL", "pg_isready -U oficina -d oficina_mecanica"]
    interval: 5s
    timeout: 5s
    retries: 5
  volumes:
    - postgres_data:/var/lib/postgresql/data
```

Na primeira inicializacao, a imagem oficial usa as variaveis `POSTGRES_DB`, `POSTGRES_USER` e `POSTGRES_PASSWORD` para criar o banco e seu usuario. Esses valores so inicializam um volume novo; alterar as variaveis depois nao recria automaticamente usuarios ou bancos existentes.

O PostgreSQL e um servidor de banco de dados completo rodando dentro do container. Ele continua suportando tabelas, indices, chaves estrangeiras, transacoes, usuarios e consultas SQL normalmente. O Docker apenas fornece o ambiente em que o processo do PostgreSQL e executado.

### Onde os dados ficam

Internamente, o PostgreSQL grava seus arquivos em:

```text
/var/lib/postgresql/data
```

Essa pasta esta associada ao volume nomeado `postgres_data`. Portanto, os dados nao dependem do ciclo de vida do container:

```text
Container PostgreSQL
        |
        v
/var/lib/postgresql/data
        |
        v
Volume Docker postgres_data
```

Se o container for recriado, o novo container monta o mesmo volume e encontra os dados anteriores.

Comportamento dos comandos:

| Comando | Containers | Dados do volume |
|---------|------------|-----------------|
| `docker compose stop` | Para | Mantem |
| `docker compose start` | Inicia novamente | Mantem |
| `docker compose down` | Remove | Mantem |
| `docker compose up -d` | Cria ou inicia | Reutiliza |
| `docker compose down -v` | Remove | Apaga |

O comando com `-v` deve ser usado somente quando houver intencao de apagar o banco local.

## 6. Como a API encontra o banco

No Compose, a API recebe esta connection string:

```yaml
ConnectionStrings__DefaultConnection: Host=db;Database=oficina_mecanica;Username=oficina;Password=oficina_dev
```

O nome `db` e resolvido pela rede interna do Docker para o container do PostgreSQL. Dentro do container da API, `localhost` apontaria para a propria API, e nao para o banco.

```text
De dentro do container da API: Host=db
De uma ferramenta no computador: Host=localhost
```

O mapeamento `5432:5432` permite que DBeaver, pgAdmin ou `psql` no computador acessem o mesmo PostgreSQL pela porta local.

Dados para uma ferramenta grafica:

```text
Host: localhost
Porta: 5432
Database: oficina_mecanica
Usuario: oficina
Senha: oficina_dev
```

Essas credenciais sao adequadas apenas para o ambiente local de desenvolvimento. Em producao, devem ser substituidas por secrets e senhas fortes.

## 7. Como as tabelas sao criadas

As tabelas nao sao escritas manualmente no `docker-compose.yml`. O Compose cria apenas o servidor e o banco vazio. A estrutura da aplicacao e criada pelo Entity Framework Core por meio das migrations existentes no projeto Infrastructure.

Durante a inicializacao, a API executa:

```csharp
await dbContext.Database.MigrateAsync(cancellationToken);
```

O Entity Framework consulta a tabela `__EFMigrationsHistory`, identifica quais migrations ainda nao foram aplicadas e executa somente as pendentes.

```text
PostgreSQL fica saudavel
    -> API inicia
    -> EF Core conecta no banco
    -> consulta __EFMigrationsHistory
    -> aplica migrations pendentes
    -> cria ou altera tabelas e indices
    -> cria roles e usuario administrativo inicial
    -> API comeca a atender requisicoes
```

Por isso, subir somente a imagem do PostgreSQL cria o banco, mas nao cria sozinho as tabelas da oficina. As tabelas surgem quando a API executa as migrations.

## 8. Ordem de inicializacao

O servico `api` declara uma dependencia do banco:

```yaml
depends_on:
  db:
    condition: service_healthy
```

O healthcheck utiliza `pg_isready`. A API aguarda o PostgreSQL aceitar conexoes antes de iniciar. Isso evita tentar aplicar migrations enquanto o servidor ainda esta carregando.

## 9. Arquivo `.env`

O `.env` fornece valores sensiveis ou dependentes do ambiente ao Compose:

```dotenv
JWT_SECRET=uma-chave-aleatoria-com-pelo-menos-32-caracteres
ADMIN_EMAIL=admin@oficina.local
ADMIN_PASSWORD=uma-senha-forte
```

O arquivo real esta ignorado pelo Git. O `.env.example` apresenta apenas o formato que cada desenvolvedor deve copiar e preencher.

No Compose, a sintaxe `${NOME_VARIAVEL}` le o valor do `.env`. Por exemplo:

```yaml
Jwt__Secret: ${JWT_SECRET:?Defina JWT_SECRET com pelo menos 32 caracteres}
```

Os dois sublinhados em `Jwt__Secret` representam a chave hierarquica `.NET` `Jwt:Secret`.

## 10. Comandos do dia a dia

Construir as imagens e iniciar os servicos:

```powershell
docker compose up --build -d
```

Ver o estado:

```powershell
docker compose ps
```

Ver os logs da API:

```powershell
docker compose logs -f api
```

Ver os logs do banco:

```powershell
docker compose logs -f db
```

Parar sem remover:

```powershell
docker compose stop
```

Remover os containers e preservar os dados:

```powershell
docker compose down
```

## 11. Como visualizar tabelas e dados

### DBeaver ou pgAdmin

Crie uma conexao PostgreSQL usando `localhost`, porta `5432` e as credenciais apresentadas anteriormente. Depois abra:

```text
oficina_mecanica
    -> Schemas
    -> public
    -> Tables
```

### Terminal dentro do container

```powershell
docker compose exec db psql -U oficina -d oficina_mecanica
```

Comandos uteis dentro do `psql`:

```sql
\dt
SELECT * FROM "__EFMigrationsHistory";
SELECT * FROM "AspNetUsers";
\q
```

`\dt` lista as tabelas e `\q` encerra o `psql`. Os nomes entre aspas preservam maiusculas e minusculas usadas pelo Entity Framework.

## 12. Enderecos do ambiente atual

Com o Compose em execucao:

| Recurso | Endereco |
|---------|----------|
| API | `http://localhost:8080` |
| Swagger | `http://localhost:8080/swagger` |
| OpenAPI JSON | `http://localhost:8080/swagger/v1/swagger.json` |
| PostgreSQL no computador | `localhost:5432` |
| PostgreSQL para a API | `db:5432` |

O Swagger permite testar os endpoints. Depois do login, copie o token retornado, clique em `Authorize` e informe o token para acessar as rotas protegidas.

## 13. Ambiente local e hospedagem

Atualmente, os containers rodam no Docker Desktop deste computador. Isso e um ambiente local, nao uma hospedagem publica. Se o computador ou o Docker Desktop estiver desligado, a API e o PostgreSQL ficam indisponiveis.

Em uma implantacao futura, a API pode continuar em container e o banco pode seguir duas estrategias:

- PostgreSQL em outro container, com volume e rotina de backup administrados pela equipe;
- PostgreSQL gerenciado por um provedor, deixando backups, disponibilidade e atualizacoes de infraestrutura sob responsabilidade do servico contratado.

Em qualquer estrategia, a API continua usando uma connection string. O que muda sao host, credenciais, seguranca de rede e operacao do banco.
