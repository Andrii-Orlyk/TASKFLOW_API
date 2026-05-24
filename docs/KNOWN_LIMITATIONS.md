# Known Limitations

TaskFlow API is a **portfolio / learning project**. It is not positioned as production-ready infrastructure.

## Scope

- No frontend application
- No OAuth / social login
- No refresh tokens or email verification
- No admin panel, payments, or background job processing
- No cloud deployment configuration in this repository

## Runtime and data

- **Development auto-migration:** When `ASPNETCORE_ENVIRONMENT` is `Development`, the API calls `Database.Migrate()` on startup. This is for local runs and Docker Compose only. Production should apply migrations through an explicit, controlled process.
- **Docker secrets:** Compose uses placeholder JWT values. Replace them for any shared environment; never commit real secrets.
- **PostgreSQL vs SQLite:** Production-style runs use PostgreSQL. Integration tests use an in-memory **SQLite** database for speed and isolation—they do not validate PostgreSQL-specific behavior (see [TESTING.md](TESTING.md)).

## Operations

- No health-check endpoints beyond Docker Compose `pg_isready` for Postgres
- No structured observability (APM, distributed tracing)
- CI runs build and tests only—no deployment stage

## Security (portfolio level)

- Password hashing and JWT are implemented for demonstration
- Rate limiting, account lockout, and advanced threat controls are out of scope

For intentional future work, see [ROADMAP.md](ROADMAP.md).
