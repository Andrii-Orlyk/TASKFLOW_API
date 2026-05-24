# Swagger Testing Guide — TaskFlow API

Swagger UI is available when the API runs in **Development** mode.

| Start method | Swagger URL |
|---|---|
| `docker compose up --build` | `http://localhost:5000/swagger` |
| `dotnet run --project src/TaskFlow.Api` | Check console output for the port — it may differ |

**Swagger Authorize rule:** click the **Authorize** button (lock icon) and paste the **raw JWT token only** — do not type or paste the `Bearer ` prefix. Swagger adds it automatically.

Pasting `Bearer eyJ…` makes Swagger send `Authorization: Bearer Bearer eyJ…`, which fails with 401 on every request.

---

## Section 1 — Happy Path

Execute these steps **in order**. Every step should return **200 OK**. Do not skip steps — later steps require IDs from earlier responses.

| Step | Action | Expected | Note |
|------|--------|----------|------|
| 1 | `POST /api/auth/register` | 200 OK | Use a unique email |
| 2 | Copy token from response | — | Save as `<jwt-token>` |
| 3 | Click **Authorize**, paste raw token | — | No `Bearer ` prefix |
| 4 | `GET /api/auth/me` | 200 OK | Confirms token is valid |
| 5 | `POST /api/projects` | 200 OK | Save returned `id` as `<projectId>` |
| 6 | `GET /api/projects` | 200 OK | Created project appears in list |
| 7 | `POST /api/tasks` — use `<projectId>`, `"priority": "High"` | 200 OK | Save returned `id` as `<taskId>`; status defaults to `"Todo"` |
| 8 | `GET /api/tasks` | 200 OK | Created task appears in list |
| 9 | `PATCH /api/tasks/<taskId>/status` — body: `{"status":"InProgress"}` | 200 OK | `completedAt` is `null` |
| 10 | `PATCH /api/tasks/<taskId>/status` — body: `{"status":"Done"}` | 200 OK | `completedAt` is set |
| 11 | `POST /api/tasks/<taskId>/comments` — body: `{"content":"…"}` | 200 OK | Save returned `id` as `<commentId>` |
| 12 | `GET /api/tasks/<taskId>/comments` | 200 OK | Created comment appears in list |
| 13 | `GET /api/dashboard/summary` | 200 OK | Counts reflect current user's data only |

> **Important:** steps 9 and 10 must be executed in order — `InProgress` first, then `Done`. Sending `"Done"` directly from `"Todo"` is an invalid transition and returns 409. That case belongs to Section 2, not the happy path.

### `GET /api/auth/me` — "No parameters" in Swagger

`GET /api/auth/me` shows **"No parameters"** in Swagger UI. This is correct. The endpoint reads the current user identity from JWT claims — it does not accept query, path, or body parameters. Click **Execute** directly after authorizing.

---

## Section 2 — Negative Tests / Expected Failures

Run these scenarios only to verify error handling. They are **expected to fail**. Do not include them in the happy-path flow above.

| Scenario | Endpoint | Expected status | Expected `code` |
|----------|----------|:---------------:|-----------------|
| Missing JWT token | `GET /api/auth/me` (no header) | 401 | `auth.unauthorized` |
| Invalid login | `POST /api/auth/login` wrong password | 401 | `auth.invalid_credentials` |
| Duplicate register | `POST /api/auth/register` same email twice | 409 | `auth.email_exists` |
| Whitespace project name | `POST /api/projects` `"name": "   "` | 400 | `validation.failed` or `project.invalid_name` |
| Empty task title | `POST /api/tasks` `"title": ""` | 400 | `validation.failed` |
| Past due date | `POST /api/tasks` `"dueDate": "2020-01-01T00:00:00Z"` | 400 | `validation.failed` or `task.invalid_due_date` |
| Direct `Todo` → `Done` | `PATCH /api/tasks/<freshTaskId>/status` `{"status":"Done"}` | 409 | `task.invalid_status_transition` |
| Wrong task id | `PATCH /api/tasks/<wrongTaskId>/status` | 404 | `task.not_found` |
| Ownership isolation | User A token + User B resource | 404 | `project.not_found` / `task.not_found` |

> For the **direct `Todo` → `Done`** test: create a fresh task first so it starts in `"Todo"` status. Do not reuse the task from the happy path — it was already moved to `"Done"`.

> For **ownership isolation**: register a second user to get User B's token, then call a project or task endpoint using User A's `<projectId>` or `<taskId>` with User B's token. The API returns 404 (not 403) intentionally — returning 403 would reveal that the resource exists.

---

## Section 3 — Troubleshooting

**"Bearer Bearer" — 401 on every request**

Cause: `Bearer eyJ…` was pasted into the Swagger Authorize popup instead of the raw token.

Fix:
1. Click **Authorize** → **Logout**.
2. Paste only the raw JWT — it starts with `eyJ` and contains no spaces.
3. Click **Authorize** → **Close**.

---

**`PATCH status` returns 404**

Possible causes:
- Using a placeholder `<taskId>` from docs instead of the real `id` returned by `POST /api/tasks`.
- Using `<projectId>` or `<userId>` in the URL instead of the task's own `id`.
- The task belongs to a different user than the active token.

Fix: copy the `"id"` field directly from the `POST /api/tasks` response body.

---

**`PATCH status` returns 409**

Cause: the status transition is not allowed. Common case: trying `Todo` → `Done` directly.

Fix: send `"InProgress"` first (step 9), then `"Done"` (step 10).

---

**`POST /api/tasks` fails with an enum error**

Cause: the API is not configured to accept string enum values. The docs and Swagger UI use string enums — `"High"`, `"InProgress"`, `"Done"`.

Fix: verify that `Program.cs` registers `JsonStringEnumConverter`:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

A correctly configured API accepts `"priority": "High"` and returns `"status": "Todo"`.

---

**`GET /api/auth/me` shows "No parameters" in Swagger — is this a bug?**

No. The endpoint reads user identity from JWT claims, not from request parameters. Click **Execute** without filling in any fields. A 200 response confirms the token is valid.

---

## Error response format

All API errors use the same shape:

```json
{
  "statusCode": 400,
  "code": "validation.failed",
  "message": "Validation failed",
  "errors": ["'Title' must not be empty."]
}
```

| Scenario | Status | `code` |
|---|:---:|---|
| Validation failure | 400 | `validation.failed` |
| Invalid due date / business rule | 400 | `task.invalid_due_date` / `project.invalid_name` |
| Missing / invalid JWT | 401 | `auth.unauthorized` |
| Wrong login credentials | 401 | `auth.invalid_credentials` |
| Foreign resource (ownership) | 404 | `project.not_found` / `task.not_found` |
| Duplicate email | 409 | `auth.email_exists` |
| Invalid status transition | 409 | `task.invalid_status_transition` |

## Enum reference

**TaskPriority**: `"Low"` `"Medium"` `"High"`

**TaskItemStatus**: `"Todo"` `"InProgress"` `"Done"` `"Cancelled"`

---

See also [API_EXAMPLES.md](API_EXAMPLES.md) for full request/response examples with bodies.
