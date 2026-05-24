# Docker + PostgreSQL (local Development)

## Quick start

```bash
docker compose config
docker compose up --build
```

- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- PostgreSQL: `localhost:5432` (user `postgres`, password `postgres`, database `taskflow`)

## Configuration

`docker-compose.yml` sets:

- `ASPNETCORE_ENVIRONMENT=Development` (enables Swagger and auto-migration)
- `ConnectionStrings__DefaultConnection` pointing at the `taskflow-db` service
- Placeholder JWT settings (not production secrets)

Local `dotnet run` without Docker uses `appsettings.json` (`Host=localhost`).

## EF Core migrations (Development only)

On startup, when `ASPNETCORE_ENVIRONMENT` is **Development**, the API runs `Database.Migrate()` once. This is intended **only** for local development and Docker Compose—not for production deployments.

Production should apply migrations explicitly (CI job, `dotnet ef database update`, or your platform migration step).

See also [KNOWN_LIMITATIONS.md](KNOWN_LIMITATIONS.md) and the main [README.md](../README.md).

## Health checks

`taskflow-db` uses `pg_isready` before `taskflow-api` starts (`depends_on: service_healthy`).

## Runtime verification (2026-05-22)

Verified on macOS with Docker Desktop:

```bash
docker compose config
docker compose down -v
docker compose up --build
```

| Check | Result |
|-------|--------|
| `docker compose config` | Valid |
| `taskflow-db` healthcheck | **healthy** (`pg_isready`) |
| API container | **running** |
| EF migrations | **applied** (`20260520061455_InitialCreate`, EF 8.0.11) |
| Swagger UI | **HTTP 200** at http://localhost:5000/swagger |
| OpenAPI JSON | **HTTP 200** at http://localhost:5000/swagger/v1/swagger.json |

### Docker build notes

- The API image restores/publishes `TaskFlow.Api` only (test projects are not required in the image).
- `.dockerignore` excludes `bin/`, `obj/`, and malformed `bin\Debug` artifact folders so Linux builds do not fail with MSB3552.

### Stop and reset

```bash
docker compose down -v
```
