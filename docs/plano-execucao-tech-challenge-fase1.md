# Plano de Execução — Tech Challenge Fase 1
## Sistema Integrado de Atendimento e Execução de Serviços (Oficina Mecânica)

---

## 1. Visão geral do prazo

Divida o trabalho em **5 blocos**, do design ao entregável final. Sugestão de distribuição (ajuste conforme o prazo real da sua turma):

| Bloco | Foco | % do tempo total |
|---|---|---|
| 1 | Modelagem DDD + Event Storming | 15% |
| 2 | Setup técnico (stack, repo, Docker) | 10% |
| 3 | Desenvolvimento do back-end (CRUDs + fluxos + segurança) | 45% |
| 4 | Testes automatizados + scan de vulnerabilidades | 15% |
| 5 | Documentação, vídeo e entregável final | 15% |

---

## 2. Bloco 1 — Modelagem DDD

**Objetivo:** Entender o domínio antes de escrever qualquer código.

- [ ] Rodar **Event Storming** (Miro) para os dois fluxos obrigatórios:
  - Criação e acompanhamento da OS
  - Gestão de peças e insumos
- [ ] Identificar: **Eventos de domínio**, **Comandos**, **Agregados**, **Bounded Contexts**, **Políticas**
- [ ] Definir a **Linguagem Ubíqua** (glossário: OS, Orçamento, Diagnóstico, Aprovação, etc.)
- [ ] Esboçar os **agregados principais**: `OrdemDeServico`, `Cliente`, `Veiculo`, `Servico`, `Peca`
- [ ] Desenhar diagrama de **contexto/camadas** (Domain / Application / Infrastructure / API)

**Entrega parcial:** board do Miro com Event Storming + Linguagem Ubíqua (isso já é parte do entregável final da documentação DDD).

---

## 3. Bloco 2 — Setup técnico

**Objetivo:** Preparar o esqueleto do projeto antes de codar as regras de negócio.

- [ ] Definir a stack (linguagem, framework, banco de dados) — **justificar a escolha do banco no relatório**
- [ ] Criar repositório **privado** e dar acesso ao usuário `soatarchitecture`
- [ ] Estrutura de pastas em camadas (ex: `domain`, `application`, `infrastructure`, `api`)
- [ ] Configurar `Dockerfile` e `docker-compose.yml` (app + banco)
- [ ] Configurar Swagger/OpenAPI desde o início
- [ ] Configurar autenticação JWT (esqueleto)
- [ ] Configurar pipeline de testes (framework de testes + cobertura)

---

## 4. Bloco 3 — Desenvolvimento (o núcleo do MVP)

Divida por **subdomínios**, ideal para trabalho em paralelo entre membros do grupo.

### 4.1 Módulo Cliente & Veículo
- [ ] CRUD de clientes (validação de CPF/CNPJ)
- [ ] CRUD de veículos (validação de placa, vínculo com cliente)

### 4.2 Módulo Serviços & Peças
- [ ] CRUD de serviços
- [ ] CRUD de peças/insumos com **controle de estoque**
- [ ] Regras de baixa de estoque ao usar peça em uma OS

### 4.3 Módulo Ordem de Serviço (coração do sistema)
- [ ] Criação da OS (cliente + veículo + serviços + peças)
- [ ] Geração automática de orçamento
- [ ] Envio de orçamento para aprovação
- [ ] Máquina de estados dos status (Recebida → Em diagnóstico → Aguardando aprovação → Em execução → Finalizada → Entregue)
- [ ] Transições automáticas conforme ações (ex: aprovar orçamento → muda status)
- [ ] Endpoint de consulta de status pelo cliente (API pública ao cliente, mas autenticada)

### 4.4 Módulo Administrativo
- [ ] Listagem/detalhamento de OS (com filtros)
- [ ] Cálculo de tempo médio de execução dos serviços

### 4.5 Segurança
- [ ] JWT em todas as rotas administrativas
- [ ] Validação robusta de dados sensíveis (CPF/CNPJ, placa) — evitar apenas regex simples, considerar dígito verificador
- [ ] Tratamento de erros e respostas HTTP padronizadas

**Dica de divisão em grupo:** cada pessoa pode "dono" de um módulo (ex: uma pessoa em Cliente/Veículo, outra em Peças/Estoque, outra em OS/status, outra em Segurança/Infra), com um responsável por integração.

---

## 5. Bloco 4 — Qualidade e Segurança

- [ ] Testes unitários dos domínios críticos (regras de negócio da OS, orçamento, estoque)
- [ ] Testes de integração dos principais endpoints
- [ ] Garantir **cobertura mínima de 80%** nos domínios críticos (medir com ferramenta de cobertura)
- [ ] Rodar **scan de vulnerabilidades** (ex: dependências desatualizadas, SAST) e documentar os achados
- [ ] Corrigir (ou justificar) os principais achados críticos/altos

---

## 6. Bloco 5 — Documentação e entrega

- [ ] `README.md` completo (como rodar local, objetivos, stack, endpoints principais)
- [ ] Finalizar documentação DDD no Miro (Event Storming + diagramas + Linguagem Ubíqua)
- [ ] Relatório de análise de vulnerabilidades (resultado do scan + análise)
- [ ] Gravar vídeo de até 15 min demonstrando:
  - Fluxo completo de criação e acompanhamento de OS
  - CRUDs administrativos
  - Autenticação JWT funcionando
  - Testes rodando / cobertura
  - Docker subindo o ambiente completo
- [ ] Montar o **PDF final de entrega** com:
  - Nome do grupo
  - Participantes + usernames Discord
  - Link da documentação (Miro)
  - Link do repositório
  - Relatório de vulnerabilidades

---

## 7. Checklist rápido de "pronto para entregar"

- [ ] Repositório privado com acesso a `soatarchitecture`
- [ ] `docker-compose up` sobe tudo funcionando
- [ ] Swagger acessível e completo
- [ ] JWT protegendo rotas administrativas
- [ ] Cobertura de testes ≥ 80% nos domínios críticos
- [ ] Event Storming + Linguagem Ubíqua no Miro
- [ ] Relatório de vulnerabilidades anexado
- [ ] Vídeo ≤ 15 min gravado
- [ ] PDF de entrega com todos os links e informações do grupo

---

## Próximos passos sugeridos

1. Fechar a **stack técnica** (linguagem, framework, banco)
2. Fazer o **Event Storming** no Miro
3. Modelar os **agregados e entidades** do domínio
4. Só então começar a codar

Posso ajudar com qualquer um desses agora — é só pedir.
