# Planejamento Frontend - React

## Decisao

O frontend sera desenvolvido em **React 18 + TypeScript** como complemento ao back-end. Nao e obrigatorio na Fase 1, mas esta planejado para agregar valor ao projeto e facilitar a demonstracao no video.

**Prioridade:** Implementar APOS o back-end estar completo e testado.

---

## Stack

| Tecnologia | Proposito |
|-----------|-----------|
| React 18 | UI library |
| TypeScript | Tipagem estatica |
| Vite | Build tool (rapido, leve) |
| React Router | Navegacao SPA |
| Axios | HTTP client com interceptors JWT |
| TanStack Query | Cache e fetch de dados do servidor |
| Tailwind CSS | Estilizacao utility-first |
| Shadcn/ui | Componentes acessiveis e customizaveis |
| React Hook Form + Zod | Formularios com validacao tipada |
| JWT decode | Decodificar token para roles |

---

## Paginas

| Pagina | Role | Funcionalidade |
|--------|------|---------------|
| /login | Todos | Login com email/senha, retorna JWT |
| /dashboard | Admin | Visao geral (OS abertas, alertas estoque) |
| /clientes | Admin, Atendente | CRUD de clientes |
| /veiculos | Admin, Atendente | CRUD de veiculos vinculados a clientes |
| /ordens-servico | Admin, Atendente, Mecanico | Lista de OS com filtros e status |
| /ordens-servico/:id | Todos (conforme role) | Detalhe da OS, timeline de estados |
| /orcamento/:id | Admin, Atendente, Cliente | Visualizar/aprovar/rejeitar orcamento |
| /pecas | Admin | CRUD de pecas, alertas de estoque baixo |
| /servicos | Admin | CRUD de servicos do catalogo |
| /minha-os | Cliente | Consulta de status e aprovacao de orcamento |

---

## Estrutura de Pastas

 frontend/
 public/
 src/
 api/ <- Axios instance + endpoints
 client.ts (axios com interceptor JWT)
 clientes.ts
 ordensServico.ts
 orcamentos.ts
 pecas.ts
 auth.ts
 components/ <- Componentes reutilizaveis
 ui/ (shadcn: Button, Input, Table, Dialog)
 Layout.tsx
 Sidebar.tsx
 StatusBadge.tsx
 ProtectedRoute.tsx
 pages/ <- Uma pasta por pagina
 Login/
 Dashboard/
 Clientes/
 Veiculos/
 OrdensServico/
 Orcamento/
 Pecas/
 Servicos/
 MinhaOS/
 hooks/ <- Custom hooks
 useAuth.ts
 useOrdemServico.ts
 contexts/ <- Context API
 AuthContext.tsx
 types/ <- TypeScript interfaces
 cliente.ts
 ordemServico.ts
 orcamento.ts
 peca.ts
 utils/ <- Helpers
 formatters.ts
 validators.ts
 App.tsx
 main.tsx
 .env
 package.json
 tsconfig.json
 tailwind.config.js
 vite.config.ts

---

## Fluxo de Autenticacao

1. Usuario faz login -> POST /api/auth/login
2. API retorna JWT com role no payload
3. Frontend armazena token (localStorage)
4. Axios interceptor adiciona Authorization: Bearer <token> em toda request
5. ProtectedRoute verifica role antes de renderizar pagina
6. Token expirado -> redirect pro /login

---

## Integracao com a API

### Axios client com interceptor

 // api/client.ts
 import axios from 'axios';

 const api = axios.create({
 baseURL: import.meta.env.VITE_API_URL || 'http://localhost:8080',
 });

 api.interceptors.request.use((config) => {
 const token = localStorage.getItem('token');
 if (token) config.headers.Authorization = Bearer ;
 return config;
 });

 api.interceptors.response.use(
 (response) => response,
 (error) => {
 if (error.response?.status === 401) {
 localStorage.removeItem('token');
 window.location.href = '/login';
 }
 return Promise.reject(error);
 }
 );

 export default api;

---

## Roteamento com Protecao por Role

 // components/ProtectedRoute.tsx
 interface Props {
 children: React.ReactNode;
 allowedRoles: string[];
 }

 export function ProtectedRoute({ children, allowedRoles }: Props) {
 const { user } = useAuth();

 if (!user) return <Navigate to="/login" />;
 if (!allowedRoles.includes(user.role)) return <Navigate to="/unauthorized" />;

 return children;
 }

---

## Docker (quando implementar)

Adicionar ao docker-compose.yml:

 frontend:
 build: ./frontend
 ports:
 - 3000:3000
 depends_on:
 - api
 environment:
 - VITE_API_URL=http://localhost:8080

Dockerfile do frontend:

 FROM node:20-alpine AS build
 WORKDIR /app
 COPY package*.json ./
 RUN npm ci
 COPY . .
 RUN npm run build

 FROM nginx:alpine
 COPY --from=build /app/dist /usr/share/nginx/html
 COPY nginx.conf /etc/nginx/conf.d/default.conf
 EXPOSE 3000

---

## Cronograma sugerido

| Fase | Tarefa | Tempo estimado |
|------|--------|---------------|
| 1 | Setup (Vite + Tailwind + Shadcn + Router) | 2h |
| 2 | Auth (login, context, protected routes) | 4h |
| 3 | CRUD Clientes e Veiculos | 6h |
| 4 | Fluxo OS (lista, detalhe, timeline estados) | 8h |
| 5 | Orcamento (visualizar, aprovar, rejeitar) | 4h |
| 6 | Pecas e Servicos | 4h |
| 7 | Dashboard com metricas | 4h |
| 8 | Polimento visual e responsividade | 4h |
| **Total** | | **~36h** |

---

## Decisoes de Design

- **Tema escuro** como padrao (dark mode)
- **Sidebar** com navegacao por modulo
- **Status badges** coloridos para estados da OS
- **Tabelas** com paginacao, filtros e busca
- **Formularios** com validacao inline (Zod)
- **Toasts** para feedback de acoes (sucesso/erro)
- **Loading skeletons** enquanto dados carregam
- **Responsive** para funcionar em tablet (mecanico na oficina)

---

**Status:** Planejado (implementar apos back-end concluido)
