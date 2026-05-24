# TaskFlow API

TaskFlow API is a Junior .NET portfolio backend for a task-management web application. It demonstrates how to build a practical ASP.NET Core Web API with PostgreSQL, authentication, validation, tests, Docker, and CI.

## What this project demonstrates

- Layered architecture with clear separation of concerns
- JWT authentication and per-user data isolation
- REST API design with consistent error responses
- EF Core persistence and migrations
- FluentValidation for input rules
- Unit and integration tests
- Local Docker Compose environment
- GitHub Actions build and test pipeline

## Features

- User registration, login, and current-user profile
- Projects CRUD (scoped to the authenticated user)
- Tasks CRUD with filtering, pagination, status transitions, and priority rules
- Task comments
- Dashboard summary counts for the current user
- Centralized exception handling and unified API error contract

## Tech stack

- .NET 8 (LTS)
- ASP.NET Core Web API
- PostgreSQL 16
- EF Core 8 + Npgsql
- FluentValidation 11
- JWT Bearer authentication
- xUnit, FluentAssertions, Moq
- Docker Compose
- GitHub Actions (`.github/workflows/ci.yml`)

## Architecture

```text
Client / Swagger
       ↓
TaskFlow.Api
   ┌───┴──────────────────┐
   ↓                      ↓
TaskFlow.Application   TaskFlow.Infrastructure
   ↓                      ↓
TaskFlow.Domain       PostgreSQL
```

- `TaskFlow.Api` is the HTTP entry point and composition root.
- `TaskFlow.Application` contains services, DTOs, validators, policies, and contracts.
- `TaskFlow.Domain` contains entities and enums and has no dependency on API or Infrastructure.
- `TaskFlow.Infrastructure` implements persistence, repositories, JWT generation, and password hashing.

Details: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

## API endpoints (summary)

| Area | Endpoints |
|------|-----------|
| Auth | `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me` |
| Projects | `GET/POST /api/projects`, `GET/PUT/DELETE /api/projects/{id}` |
| Tasks | `GET/POST /api/tasks`, `GET/PUT/DELETE /api/tasks/{id}`, `PATCH /api/tasks/{id}/status` |
| Comments | `GET/POST /api/tasks/{taskId}/comments`, `DELETE /api/comments/{id}` |
| Dashboard | `GET /api/dashboard/summary` |

Full specification: [docs/API.md](docs/API.md) · Request/response examples: [docs/API_EXAMPLES.md](docs/API_EXAMPLES.md)

## Database model (summary)

```text
User → Project → TaskItem → TaskComment
```

Tables: `Users`, `Projects`, `TaskItems`, `TaskComments`. Ownership flows through `Project.OwnerId` for isolation.

Details: [docs/DATABASE.md](docs/DATABASE.md)

## How to run locally

**Prerequisites:** .NET 8 SDK, PostgreSQL (or use Docker for the database only).

1. Update `src/TaskFlow.Api/appsettings.json` connection string if needed.
2. Apply migrations:

```bash
dotnet ef database update --project src/TaskFlow.Infrastructure --startup-project src/TaskFlow.Api
```

3. Run the API:

```bash
dotnet run --project src/TaskFlow.Api
```

- Open Swagger: http://localhost:5000/swagger (port may vary; check console output).

See [docs/SWAGGER_TESTING.md](docs/SWAGGER_TESTING.md) for the full Swagger walkthrough, the Authorize flow, and the enum reference.

`ASPNETCORE_ENVIRONMENT=Development` applies pending migrations on startup (local dev only).

## How to run with Docker

```bash
docker compose config
docker compose up --build
```

- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- Postgres: `localhost:5432` (see [docs/DOCKER.md](docs/DOCKER.md))

Compose sets `Development` and runs EF migrations automatically on API startup.

## How to run tests

```bash
dotnet restore
dotnet build
dotnet test
```

See [docs/TESTING.md](docs/TESTING.md) for coverage notes and the SQLite integration-test limitation.

**Terminal Swagger/API smoke test** — validates the same flow as Swagger UI from the command line.

Start the API:

```bash
docker compose up --build
```

In another terminal, run:

```bash
chmod +x scripts/swagger-api-smoke-test.sh
./scripts/swagger-api-smoke-test.sh
```

See [docs/SWAGGER_TERMINAL_TESTING.md](docs/SWAGGER_TERMINAL_TESTING.md) for full details and expected output.

## Known limitations

This is a portfolio backend, not a production deployment. See [docs/KNOWN_LIMITATIONS.md](docs/KNOWN_LIMITATIONS.md).

## Roadmap

Completed through v1.0.0 scope (API, tests, Docker, CI, docs). Possible later work: frontend, refresh tokens, email notifications, managed deployment.

Details: [docs/ROADMAP.md](docs/ROADMAP.md)


## Additional documentation

- [docs/PROJECT_SCOPE.md](docs/PROJECT_SCOPE.md)
- [docs/API.md](docs/API.md)
- [docs/API_EXAMPLES.md](docs/API_EXAMPLES.md)
- [docs/SWAGGER_TESTING.md](docs/SWAGGER_TESTING.md)
- [docs/SWAGGER_TERMINAL_TESTING.md](docs/SWAGGER_TERMINAL_TESTING.md)
- [docs/AUTH.md](docs/AUTH.md)
- [docs/DATABASE.md](docs/DATABASE.md)
- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- [docs/DOCKER.md](docs/DOCKER.md)
- [docs/TESTING.md](docs/TESTING.md)
- [docs/KNOWN_LIMITATIONS.md](docs/KNOWN_LIMITATIONS.md)
- [docs/ROADMAP.md](docs/ROADMAP.md)
