# Swagger / API Terminal Smoke Test

A repeatable `curl`-based script that tests the same flow a reviewer runs through Swagger UI.
No browser required — runs fully in the terminal.

## Prerequisites

- [jq](https://jqlang.github.io/jq/) — JSON processor

  ```bash
  brew install jq       # macOS
  sudo apt install jq   # Debian/Ubuntu
  ```

- API running at `http://localhost:5000` (see "Start the API" below)

---

## Start the API

### Option A — Docker Compose (recommended)

```bash
docker compose up --build
```

- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`
- PostgreSQL: `localhost:5432` (migrations run automatically)

### Option B — dotnet run (requires a local PostgreSQL)

```bash
dotnet run --project src/TaskFlow.Api
```

---

## Run the smoke test

Open a **second terminal** while the API is running:

```bash
chmod +x scripts/swagger-api-smoke-test.sh
./scripts/swagger-api-smoke-test.sh
```

### Custom base URL

```bash
BASE_URL=http://localhost:5017 ./scripts/swagger-api-smoke-test.sh
```

---

## What the script tests

### Happy path (Steps 0 – 12)

| Step | Endpoint | What is verified |
|------|----------|-----------------|
| 0 | `GET /swagger/v1/swagger.json` | OpenAPI JSON is reachable; all 6 main paths present |
| 1 | `POST /api/auth/register` | 200 with token |
| 2 | `POST /api/auth/login` | 200 with token |
| 3 | `GET /api/auth/me` | 200; email, firstName, lastName, role match |
| 4 | `POST /api/projects` | 200 with id |
| 5 | `GET /api/projects` | 200; created project in list |
| 6 | `POST /api/tasks` | 200 with **string enum** `"priority":"High"`; response `status="Todo"` |
| 7 | `GET /api/tasks` | 200; created task in list |
| 8 | `PATCH /api/tasks/{id}/status` | 200 with `"status":"InProgress"` |
| 9 | `PATCH /api/tasks/{id}/status` | 200 with `"status":"Done"` and `completedAt` set |
| 10 | `POST /api/tasks/{id}/comments` | 200 with id |
| 11 | `GET /api/tasks/{id}/comments` | 200; comment in list |
| 12 | `GET /api/dashboard/summary` | 200; `totalProjects≥1`, `totalTasks≥1`, `doneTasks≥1` |

### Negative cases (A – G)

| Test | Endpoint | Expected |
|------|----------|----------|
| A | `GET /api/auth/me` (no token) | 401 with `ApiErrorResponse` body |
| B | `POST /api/auth/login` (wrong password) | 401 `auth.invalid_credentials` |
| C | `POST /api/auth/register` (duplicate email) | 409 `auth.email_exists` |
| D | `POST /api/projects` (whitespace name) | 400 `validation.failed` or `project.invalid_name` |
| E | `POST /api/tasks` (empty title) | 400 `validation.failed` — **critical; 200 = broken validator** |
| F | `POST /api/tasks` (past due date) | 400 `validation.failed` or `task.invalid_due_date` |
| G | `PATCH /api/tasks/{id}/status` (Todo→Done skip on a **fresh** Todo task) | 409 `task.invalid_status_transition` |
| H | `PATCH /api/tasks/<nonexistent-id>/status` — body: `{"status":"InProgress"}` | 404 `task.not_found` |

> **Note for test G:** the script creates a new task specifically for this test. The task from the happy path is already in `"Done"` status and cannot be reused — the transition policy only applies from `"Todo"`.

> **Note for test H:** The script uses a well-known UUID (`3fa85f64-5717-4562-b3fc-2c963f66afa6`) that will not exist in the test database. Any 404 with `task.not_found` proves ownership and lookup correctness. If you receive 200 during the happy-path status update, you are likely using a placeholder id instead of the real `taskId` from the `POST /api/tasks` response.

---

## Swagger UI vs curl — key difference

| Client | How to send the JWT |
|--------|---------------------|
| **Swagger UI** — Authorize popup | Paste **raw token only** — no `Bearer ` prefix.<br>Swagger adds the prefix automatically. |
| **curl / Postman / script** | Full header: `Authorization: Bearer <token>` |

### Common mistake — "Bearer Bearer"

If you paste `Bearer eyJ…` into the Swagger Authorize popup, Swagger sends:

```
Authorization: Bearer Bearer eyJ…
```

→ 401 Unauthorized. Always paste only the raw token (starting with `eyJ…`).

---

## What failure messages mean

| Message | Cause | Fix |
|---------|-------|-----|
| `Swagger JSON not available` | API not running or wrong port | `docker compose up --build` |
| `BUG: API does not accept string enum "High"` | `JsonStringEnumConverter` not registered | Add in `Program.cs` `AddControllers().AddJsonOptions(…)` |
| `CRITICAL BUG: empty task title accepted` | `RequestValidationService` silently skips validators | Fix reflection lookup in `RequestValidationService.cs` |
| `401 body is empty` | `JwtBearerEvents.OnChallenge` not configured | Wire `OnChallenge` in `Program.cs` JWT options |
| `Bearer Bearer` in logs | User pasted `Bearer …` into Swagger Authorize | Paste raw token only |
| Status update returns 404 | Wrong `taskId` — used a placeholder, `projectId`, or another user's task | Copy the `"id"` from `POST /api/tasks` response |
| Status update returns 409 | Invalid transition (e.g. `Todo` → `Done` directly) | Send `InProgress` first (step 8), then `Done` (step 9) |

---

## Sample output (all passing)

```
TaskFlow API — terminal smoke test
  Base URL : http://localhost:5000
  Email    : reviewer.1748032400@example.com

Step 0 — Swagger JSON
  ✔ Swagger JSON returned 200
  ✔ Path present: /api/auth/register
  …

Step 6 — Create task (priority = "High")
  HTTP 200  (expected 200)
  ✔ taskId = a1b2c3d4-…
  ✔ status = Todo
  ✔ priority = High

…

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  TaskFlow API — terminal smoke test summary
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Swagger JSON                             PASS
  Register                                 PASS
  Login                                    PASS
  Me                                       PASS
  Create project                           PASS
  Get projects                             PASS
  Create task (string enum)                PASS
  Get tasks                                PASS
  Update status InProgress                 PASS
  Update status Done                       PASS
  Create comment                           PASS
  Get comments                             PASS
  Dashboard                                PASS
  Missing token 401                        PASS
  Invalid login                            PASS
  Duplicate register                       PASS
  Invalid project name                     PASS
  Empty task title                         PASS
  Past due date                            PASS
  Invalid transition                       PASS
  Wrong task id                            PASS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Passed: 21   Failed: 0   Warnings: 0
  Final result: PASS
```

---

See also: [SWAGGER_TESTING.md](SWAGGER_TESTING.md) — Swagger UI walkthrough (browser-based).
