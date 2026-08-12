# Decisao de Banco de Dados - PostgreSQL

## Contexto

O sistema de oficina mecanica possui um dominio com entidades fortemente relacionadas (Cliente, Veiculo, Ordem de Servico, Orcamento, Pecas, Servicos) e requer transacoes ACID para garantir consistencia - especialmente no fluxo de reserva de estoque apos aprovacao de orcamento.

Avaliamos as seguintes opcoes:

1. **PostgreSQL** (relacional, open-source)
2. **SQL Server** (relacional, Microsoft)
3. **MySQL/MariaDB** (relacional, open-source)
4. **MongoDB** (NoSQL, documentos)

## Por que banco relacional?

O dominio da oficina mecanica exige banco relacional pelos seguintes motivos:

- **Relacionamentos fortes entre entidades**: Cliente possui Veiculos, Veiculo possui Ordens de Servico, OS possui Orcamento, Orcamento possui Itens que referenciam Pecas e Servicos
- **Transacoes ACID**: ao aprovar um orcamento, precisamos reservar pecas no estoque de forma atomica
- **Integridade referencial**: foreign keys garantem que nao existam OS sem cliente, orcamentos sem OS, ou reservas sem peca valida
- **Consultas complexas**: relatorios de tempo medio de execucao, listagem de OS com filtros

## Por que NAO MongoDB/NoSQL?

- Dominio tem relacionamentos cruzados entre agregados
- Transacoes multi-documento no MongoDB nao sao idiomaticas
- EF Core e projetado para bancos relacionais
- NoSQL seria adequado para documentos auto-contidos sem relacionamentos

## Comparativo: PostgreSQL vs SQL Server

| Criterio | PostgreSQL | SQL Server |
|----------|-----------|------------|
| Custo | 100%% gratuito | Gratuito so no Express/Developer |
| Imagem Docker | ~80MB (alpine) | ~1.5GB |
| Startup Docker | Segundos | Mais lento, exige aceitar EULA |
| EF Core Provider | Npgsql (maduro) | Nativo Microsoft |
| Licenciamento | PostgreSQL License | Proprietario |
| Compatibilidade OS | Todos igualmente | Otimizado para Windows |
| Mercado | Startups e cloud | Enterprise corporativo |

## Decisao: PostgreSQL 16

Escolhemos **PostgreSQL** pelos seguintes motivos:

### 1. Open-source e gratuito
Sem preocupacao com licenciamento em nenhum cenario. A licenca PostgreSQL e uma das mais permissivas que existem.

### 2. Docker leve e rapido
A imagem postgres:16-alpine tem ~80MB. Para a demonstracao em video, o ambiente sobe em segundos.

### 3. Suporte maduro no EF Core
O provider Npgsql suporta: Migrations, LINQ queries, tipos nativos (jsonb, arrays), connection pooling e transacoes.

### 4. Funciona igual em qualquer OS
Membros do grupo podem usar Windows, macOS ou Linux sem ajustes.

### 5. Amplamente adotado no mercado
PostgreSQL e o banco mais adotado em novos projetos cloud (AWS RDS, Azure, GCP).

### 6. Recursos avancados disponiveis
JSONB, LISTEN/NOTIFY, full-text search, extensoes (pg_trgm).

## Configuracao

### Connection String
 Host=localhost;Database=oficina_mecanica;Username=oficina;Password=oficina_dev

### Pacotes NuGet
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.EntityFrameworkCore.Design

## Justificativa resumida (para o relatorio)

> Escolhemos PostgreSQL como banco de dados por ser open-source, leve para containerizacao (imagem Docker de ~80MB), com suporte maduro no Entity Framework Core via provider Npgsql. O modelo relacional e adequado ao nosso dominio que possui entidades com relacionamentos fortes e requer transacoes ACID para garantir consistencia nas operacoes de reserva de estoque e aprovacao de orcamentos.

**Status:** Aprovada pela equipe
