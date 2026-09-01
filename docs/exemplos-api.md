# Exemplos da API

## Convencoes

Base local:

```text
http://localhost:8080
```

Depois do login, envie o JWT nas rotas protegidas:

```http
Authorization: Bearer TOKEN_JWT
```

Os identificadores abaixo sao ilustrativos. Utilize os IDs devolvidos pelo ambiente executado.

## 1. Login

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "admin@oficina.local",
  "senha": "senha-configurada-no-env"
}
```

Resposta `200 OK`:

```json
{
  "token": "eyJ...",
  "expiraEm": "2026-09-01T20:00:00Z",
  "email": "admin@oficina.local",
  "roles": ["Admin"],
  "trocaSenhaObrigatoria": false
}
```

## 2. Criar cliente

```http
POST /api/clientes
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "nome": "Maria da Silva",
  "cpf": "52998224725",
  "telefone": "11999999999",
  "email": "maria@exemplo.com"
}
```

Resposta `201 Created`:

```json
{
  "id": "11111111-1111-1111-1111-111111111111"
}
```

## 3. Criar veiculo

```http
POST /api/veiculos
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "placa": "ABC1D23",
  "marca": "Honda",
  "modelo": "Civic",
  "ano": 2022,
  "clienteId": "11111111-1111-1111-1111-111111111111"
}
```

Resposta `201 Created`:

```json
{
  "id": "22222222-2222-2222-2222-222222222222"
}
```

## 4. Criar servico

```http
POST /api/servicos
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "nome": "Troca de oleo",
  "descricao": "Troca de oleo e filtro",
  "precoBase": 150.00,
  "tempoEstimadoMinutos": 45
}
```

## 5. Criar peca

```http
POST /api/pecas
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "nome": "Filtro de oleo",
  "codigo": "FLT-001",
  "descricao": "Filtro compativel com o veiculo",
  "precoUnitario": 45.90,
  "quantidadeEmEstoque": 10,
  "quantidadeMinima": 3
}
```

## 6. Criar Ordem de Servico

```http
POST /api/ordens-de-servico
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "clienteId": "11111111-1111-1111-1111-111111111111",
  "veiculoId": "22222222-2222-2222-2222-222222222222"
}
```

## 7. Iniciar e registrar diagnostico

```http
PATCH /api/ordens-de-servico/{ordemDeServicoId}/iniciar-diagnostico
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "mecanicoId": "33333333-3333-3333-3333-333333333333"
}
```

```http
PATCH /api/ordens-de-servico/{ordemDeServicoId}/registrar-diagnostico
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "diagnostico": "Necessaria troca de oleo e filtro",
  "itens": [
    {
      "servicoId": "44444444-4444-4444-4444-444444444444",
      "pecaId": "55555555-5555-5555-5555-555555555555",
      "quantidade": 1,
      "observacao": "Substituir o filtro durante o servico"
    }
  ]
}
```

## 8. Gerar e enviar orcamento

```http
POST /api/orcamentos
Authorization: Bearer TOKEN_JWT
Content-Type: application/json
```

```json
{
  "ordemDeServicoId": "66666666-6666-6666-6666-666666666666",
  "observacao": "Orcamento inicial"
}
```

Depois da revisao dos itens:

```http
PATCH /api/orcamentos/{orcamentoId}/enviar
Authorization: Bearer TOKEN_JWT
```

## 9. Aprovar ou rejeitar como cliente

O usuario com role `Cliente` e claim `cliente_id` chama uma das operacoes:

```http
PATCH /api/orcamentos/{orcamentoId}/aprovar
Authorization: Bearer TOKEN_CLIENTE
```

```http
PATCH /api/orcamentos/{orcamentoId}/rejeitar
Authorization: Bearer TOKEN_CLIENTE
```

Na aprovacao, a API valida e reserva estoque dentro de uma transacao. Estoque insuficiente impede a aprovacao e retorna conflito.

## 10. Executar, finalizar e entregar

```http
PATCH /api/ordens-de-servico/{ordemDeServicoId}/servicos/{servicoId}/executar
Authorization: Bearer TOKEN_JWT
```

```http
PATCH /api/ordens-de-servico/{ordemDeServicoId}/finalizar
Authorization: Bearer TOKEN_JWT
```

```http
PATCH /api/ordens-de-servico/{ordemDeServicoId}/entregar
Authorization: Bearer TOKEN_JWT
```

Ao finalizar, as reservas sao consumidas e o estoque efetivo e reduzido.

## 11. Resposta de validacao

Exemplo conceitual de `400 Bad Request`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Erro de validacao",
  "status": 400,
  "errors": {
    "Email": ["Email invalido"]
  }
}
```

Outros status padronizados:

| HTTP | Uso |
|------|-----|
| `401` | Credenciais ou token invalidos, usuario inativo |
| `403` | Role sem permissao ou troca de senha obrigatoria |
| `404` | Recurso inexistente |
| `409` | Duplicidade, conflito de estado, relacionamento ou estoque |

## 12. Fonte completa

Com a API ativa:

- Swagger: `http://localhost:8080/swagger`;
- OpenAPI JSON: `http://localhost:8080/swagger/v1/swagger.json`.
