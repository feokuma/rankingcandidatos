# Ranking de Candidatos

## Descrição

Para cidadãos, cujo objetivo para as próximas eleições é avaliar melhor os candidatos, o Ranking de Candidatos, que é uma plataforma web, irá permitir criar o perfil de um candidato em que o cidadão tenha interesse em dar seu voto, para centralizar e organizar informações que ajudem a avaliar e comparar com outros candidatos, organizando posts, reportagens, vídeos e quaisquer propostas e opiniões por área como educação, saúde e planejamento. O cidadão poderá pontuar as informações como positivas ou negativas e a plataforma faz um cálculo para apresentar quais candidatos têm maior compatibilidade com os interesses do cidadão.

## Stack atual

- Backend: ASP.NET Core (.NET 10) + EF Core + PostgreSQL
- Frontend: Next.js (App Router + TypeScript)
- Infra local: Supabase em containers (via Supabase CLI)

## Pré-requisitos

- Docker Desktop em execução
- .NET SDK 10
- Node.js 22+
- npm 10+

## Setup inicial

1. Na raiz do projeto, suba a infraestrutura local do Supabase:

```bash
npm run dev:up
```

2. Em um terminal separado, suba a API:

```bash
dotnet run --project backend/RankingCandidatos.Api
```

3. Em outro terminal, suba o frontend:

```bash
cd frontend/rankingcandidatos-web
npm install
npm run dev
```

4. Acesse a aplicação:

- Frontend: http://localhost:3000
- API: http://localhost:5000
- Supabase Studio: http://127.0.0.1:54323

## Comandos de automação local

Comandos disponíveis no `package.json` da raiz:

- `npm run dev:up` -> sobe Supabase local
- `npm run dev:up:apps` -> sobe Supabase e abre API + frontend em terminais separados
- `npm run dev:down` -> para Supabase local
- `npm run dev:reset` -> reseta banco local do Supabase (migrations + seed)
- `npm run supabase:status` -> mostra status e chaves locais

## Banco de dados e EF Core

- A API utiliza EF Core com PostgreSQL.
- A conexão padrão de desenvolvimento está em:
	- `backend/RankingCandidatos.Api/appsettings.Development.json`
- String padrão local:

```text
Host=127.0.0.1;Port=54322;Database=postgres;Username=postgres;Password=postgres
```

Também é possível sobrescrever via variável de ambiente:

```text
ConnectionStrings__DefaultConnection
```

## Preparação para autenticação Supabase

O projeto já está com configuração local do Supabase em `supabase/config.toml`, incluindo URLs de redirect para `localhost:3000`.

Quando iniciar a integração de auth, obtenha URL e chaves locais com:

```bash
npm run supabase:status
```

Use:

- `anon key` no frontend
- `service_role key` apenas no backend
