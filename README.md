# Ranking de Candidatos

## Descrição

Para cidadãos, cujo objetivo para as próximas eleições é avaliar melhor os candidatos, o Ranking de Candidatos, que é uma plataforma web, irá permitir criar o perfil de um candidato em que o cidadão tenha interesse em dar seu voto, para centralizar e organizar informações que ajudem a avaliar e comparar com outros candidatos, organizando posts, reportagens, vídeos e quaisquer propostas e opiniões por área como educação, saúde e planejamento. O cidadão poderá pontuar as informações como positivas ou negativas e a plataforma faz um cálculo para apresentar quais candidatos têm maior compatibilidade com os interesses do cidadão.

## Stack atual

- Backend: ASP.NET Core (.NET 10) + EF Core + PostgreSQL
- Frontend: Next.js (App Router + TypeScript)
- Infra local: PostgreSQL em container Docker
- Autenticação: fake/dev por enquanto
- Automação: Cake via ferramenta local do .NET

## Pré-requisitos

- Docker Desktop em execução
- .NET SDK 10
- Node.js 22+
- npm 10+

## Setup inicial

1. Na raiz do projeto, restaure as ferramentas locais do .NET:

```bash
dotnet tool restore
```

2. Entre no diretório do backend:

```bash
cd backend
```

3. Suba a infraestrutura local:

```bash
dotnet cake --target=DevUp
```

4. Em um terminal separado, a partir da raiz do projeto, suba a API:

```bash
dotnet run --project backend/RankingCandidatos.Api
```

5. Em outro terminal, a partir da raiz do projeto, suba o frontend:

```bash
cd frontend/rankingcandidatos-web
npm install
npm run dev
```

6. Acesse a aplicação:

- Frontend: http://localhost:3000
- API: http://localhost:5000

## Comandos de automação local

Comandos disponíveis em `backend/build.cake`:

- `dotnet cake --target=DevUp` -> sobe o container PostgreSQL local
- `dotnet cake --target=DevUpApps` -> sobe PostgreSQL e inicia API + frontend em processos separados
- `dotnet cake --target=DbMigrate` -> sobe PostgreSQL e aplica migrations pendentes
- `dotnet cake --target=DevDown` -> remove o container PostgreSQL local mantendo o volume de dados
- `dotnet cake --target=DevReset` -> remove container e volume, recriando o banco local limpo
- `dotnet cake --target=DbLogs` -> mostra logs do container PostgreSQL
- `dotnet cake --target=RunApi` -> roda a API
- `dotnet cake --target=RunFrontend` -> instala dependências e roda o frontend
- `dotnet cake --target=Build` -> compila a API e o frontend

Execute esses comandos a partir do diretório `backend`.

## Banco de dados e EF Core

- A API utiliza EF Core com PostgreSQL.
- Ao iniciar, a API aplica automaticamente as migrations pendentes no banco configurado.
- A conexão padrão de desenvolvimento está em:
	- `backend/RankingCandidatos.Api/appsettings.Development.json`
- String padrão local:

```text
Host=127.0.0.1;Port=5432;Database=rankingcandidatos;Username=postgres;Password=postgres
```

Também é possível sobrescrever via variável de ambiente:

```text
ConnectionStrings__DefaultConnection
```

### Migrations

Depois de alterar entidades ou mapeamentos do EF Core, crie uma nova migration a partir da raiz do projeto:

```bash
dotnet tool restore
dotnet tool run dotnet-ef -- migrations add NomeDaMigration --project backend/RankingCandidatos.Api
```

Para aplicar manualmente sem iniciar a API:

```bash
cd backend
dotnet cake --target=DbMigrate
```
