# API Examples

Sample requests and responses for GitHub reviewers. Base URL (local): `http://localhost:5000`.

Protected routes require:

```http
Authorization: Bearer <jwt-token>
```

Use Swagger (**Development** only): http://localhost:5000/swagger — click **Authorize** and paste the token without the `Bearer ` prefix.

**Enum values:**

- `TaskPriority`: `"Low"` `"Medium"` `"High"`
- `TaskItemStatus`: `"Todo"` `"InProgress"` `"Done"` `"Cancelled"`

> **Important — ID placeholders:**
> IDs shown in examples are placeholders. During Swagger/Postman/curl testing, always replace them with the real `id` returned by the previous response:
> - `<projectId>` comes from `POST /api/projects`
> - `<taskId>` comes from `POST /api/tasks`
> - `<commentId>` comes from `POST /api/tasks/{taskId}/comments`
>
> Do not use the example GUIDs directly. They will not exist in your local database.

---

## Part 1 — Positive Scenarios / Happy Path

Execute the steps below **in order**. Every step is expected to return **200 OK**. Do not skip steps — later steps depend on IDs saved in earlier ones.

---

### 1. Register

_Positive scenario — Expected: 200 OK_

```http
POST /api/auth/register
Content-Type: application/json
```

```json
{
  "email": "jane.doe@example.com",
  "password": "Password123!",
  "firstName": "Jane",
  "lastName": "Doe"
}
```

**200 OK** — save the `token` value; you will use it in every following request:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.example-token"
}
```

---

### 2. Login

_Positive scenario — Expected: 200 OK_

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "jane.doe@example.com",
  "password": "Password123!"
}
```

**200 OK** — save this token as `<jwt-token>`; prefer the login token over the register token for subsequent calls:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.example-token"
}
```

---

### 3. Authorize

_No HTTP request — configuration step_

**Swagger UI:**
1. Click the **Authorize** button (lock icon at the top of the page).
2. Paste the **raw JWT token only** — do **not** type or paste the `Bearer ` prefix.
3. Click **Authorize**, then **Close**.

Swagger adds the `Authorization: Bearer` prefix automatically. Pasting `Bearer eyJ…` produces `Authorization: Bearer Bearer eyJ…` and every subsequent call fails with 401.

**curl / Postman:** send the full header manually:

```http
Authorization: Bearer <jwt-token>
```

---

### 4. Me

_Positive scenario — Expected: 200 OK_

```http
GET /api/auth/me
Authorization: Bearer <jwt-token>
```

> `GET /api/auth/me` shows **"No parameters"** in Swagger UI. This is correct — the endpoint reads the current user identity from JWT claims, not from query, path, or body parameters.

**200 OK**

```json
{
  "id": "<userId>",
  "email": "jane.doe@example.com",
  "firstName": "Jane",
  "lastName": "Doe",
  "role": "User"
}
```

---

### 5. Create project

_Positive scenario — Expected: 200 OK — save the returned `id` as `<projectId>`_

```http
POST /api/projects
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "name": "Portfolio Backend",
  "description": "TaskFlow API sample project"
}
```

**200 OK** — copy the `id` value and save it as `<projectId>`; you will need it for task creation:

```json
{
  "id": "<projectId>",
  "name": "Portfolio Backend",
  "description": "TaskFlow API sample project",
  "createdAt": "2026-05-22T10:00:00Z",
  "updatedAt": "2026-05-22T10:00:00Z"
}
```

---

### 6. Get projects

_Positive scenario — Expected: 200 OK_

```http
GET /api/projects
Authorization: Bearer <jwt-token>
```

**200 OK** — response includes the project created in step 5. Only the current user's projects are returned.

```json
[
  {
    "id": "<projectId>",
    "name": "Portfolio Backend",
    "description": "TaskFlow API sample project",
    "createdAt": "2026-05-22T10:00:00Z",
    "updatedAt": "2026-05-22T10:00:00Z"
  }
]
```

---

### 7. Create task

_Positive scenario — Expected: 200 OK — save the returned `id` as `<taskId>`_

Replace `<projectId>` with the real id from step 5.

```http
POST /api/tasks
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "projectId": "<projectId>",
  "title": "Add integration tests",
  "description": "Cover auth, tasks, and dashboard",
  "priority": "High",
  "dueDate": "2026-06-01T00:00:00Z"
}
```

**200 OK** — `status` defaults to `"Todo"`. Copy the `id` value and save it as `<taskId>`:

```json
{
  "id": "<taskId>",
  "projectId": "<projectId>",
  "title": "Add integration tests",
  "description": "Cover auth, tasks, and dashboard",
  "status": "Todo",
  "priority": "High",
  "dueDate": "2026-06-01T00:00:00Z",
  "completedAt": null,
  "createdAt": "2026-05-22T10:05:00Z",
  "updatedAt": "2026-05-22T10:05:00Z"
}
```

`priority` accepts string values: `"Low"` `"Medium"` `"High"`.

---

### 8. Get tasks

_Positive scenario — Expected: 200 OK_

```http
GET /api/tasks
Authorization: Bearer <jwt-token>
```

**200 OK** — response includes the task created in step 7. Only the current user's tasks are returned.

```json
{
  "items": [
    {
      "id": "<taskId>",
      "projectId": "<projectId>",
      "title": "Add integration tests",
      "status": "Todo",
      "priority": "High"
    }
  ]
}
```

---

### 9. Update task status — positive flow

_Positive scenario — two sub-steps required in order_

Valid positive transition sequence: `Todo` → `InProgress` → `Done`.

> Direct `Todo` → `Done` is an **invalid transition** and returns 409 Conflict. That is a negative scenario covered in [Part 2, scenario 7](#7-invalid-status-transition). Do not use `"Done"` here on the first PATCH call.

Replace `<taskId>` with the real id from step 7.

#### Step 9.1 — Todo → InProgress

_Positive scenario — Expected: 200 OK_

```http
PATCH /api/tasks/<taskId>/status
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "status": "InProgress"
}
```

**200 OK** — `completedAt` remains `null`:

```json
{
  "id": "<taskId>",
  "status": "InProgress",
  "completedAt": null,
  "updatedAt": "2026-05-22T10:30:00Z"
}
```

#### Step 9.2 — InProgress → Done

_Positive scenario — Expected: 200 OK_

```http
PATCH /api/tasks/<taskId>/status
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "status": "Done"
}
```

**200 OK** — `completedAt` is set to the completion timestamp:

```json
{
  "id": "<taskId>",
  "status": "Done",
  "priority": "High",
  "dueDate": "2026-06-01T00:00:00Z",
  "completedAt": "2026-05-22T11:00:00Z",
  "createdAt": "2026-05-22T10:05:00Z",
  "updatedAt": "2026-05-22T11:00:00Z"
}
```

---

### 10. Create comment

_Positive scenario — Expected: 200 OK — save the returned `id` as `<commentId>` if needed_

Replace `<taskId>` with the real id from step 7.

```http
POST /api/tasks/<taskId>/comments
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "content": "Started wiring dashboard tests."
}
```

**200 OK** — save the `id` field as `<commentId>` for the next step:

```json
{
  "id": "<commentId>",
  "taskItemId": "<taskId>",
  "authorId": "<userId>",
  "content": "Started wiring dashboard tests.",
  "createdAt": "2026-05-22T11:15:00Z",
  "updatedAt": "2026-05-22T11:15:00Z"
}
```

---

### 11. Get comments

_Positive scenario — Expected: 200 OK_

Replace `<taskId>` with the real id from step 7.

```http
GET /api/tasks/<taskId>/comments
Authorization: Bearer <jwt-token>
```

**200 OK** — response includes the comment created in step 10:

```json
[
  {
    "id": "<commentId>",
    "taskItemId": "<taskId>",
    "authorId": "<userId>",
    "content": "Started wiring dashboard tests.",
    "createdAt": "2026-05-22T11:15:00Z",
    "updatedAt": "2026-05-22T11:15:00Z"
  }
]
```

---

### 12. Dashboard summary

_Positive scenario — Expected: 200 OK_

```http
GET /api/dashboard/summary
Authorization: Bearer <jwt-token>
```

**200 OK** — all counts include only the authenticated user's own data:

```json
{
  "totalProjects": 1,
  "totalTasks": 1,
  "todoTasks": 0,
  "inProgressTasks": 0,
  "doneTasks": 1,
  "overdueTasks": 0
}
```

---

## Part 2 — Negative Scenarios / Expected Failures

These scenarios are expected to fail. They prove validation, authorization, ownership isolation, and business rules. Run them only after the happy path if you want to verify error handling. Do not include them in the happy-path flow.

All error responses use the same `ApiErrorResponse` shape:

```json
{
  "statusCode": 0,
  "code": "string",
  "message": "string",
  "errors": []
}
```

---

### 1. Missing JWT token

_Negative scenario — Expected failure: 401 Unauthorized_

Send the request with no `Authorization` header.

```http
GET /api/auth/me
```

**401 Unauthorized** — expected body:

```json
{
  "statusCode": 401,
  "code": "auth.unauthorized",
  "message": "User is not authenticated.",
  "errors": []
}
```

> If the response body is empty instead of the JSON above, the `JwtBearerEvents.OnChallenge` handler is not configured in `Program.cs`. Document the actual behavior as a known gap in that case.

---

### 2. Invalid login

_Negative scenario — Expected failure: 401 Unauthorized_

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "jane.doe@example.com",
  "password": "WrongPassword123!"
}
```

**401 Unauthorized**

```json
{
  "statusCode": 401,
  "code": "auth.invalid_credentials",
  "message": "Invalid email or password.",
  "errors": []
}
```

---

### 3. Duplicate register

_Negative scenario — Expected failure: 409 Conflict_

Call `POST /api/auth/register` a second time with the same email used in step 1 of Part 1.

```http
POST /api/auth/register
Content-Type: application/json
```

```json
{
  "email": "jane.doe@example.com",
  "password": "Password123!",
  "firstName": "Jane",
  "lastName": "Doe"
}
```

**409 Conflict**

```json
{
  "statusCode": 409,
  "code": "auth.email_exists",
  "message": "Email is already registered.",
  "errors": []
}
```

---

### 4. Whitespace project name

_Negative scenario — Expected failure: 400 Bad Request_

```http
POST /api/projects
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "name": "   ",
  "description": "Invalid project"
}
```

**400 Bad Request** — actual code depends on where validation fires (both are accepted):

```json
{
  "statusCode": 400,
  "code": "validation.failed",
  "message": "Validation failed",
  "errors": [
    "'Name' must not be empty."
  ]
}
```

Accepted codes: `validation.failed` or `project.invalid_name`.

---

### 5. Empty task title

_Negative scenario — Expected failure: 400 Bad Request_

Replace `<projectId>` with a real project id from Part 1.

```http
POST /api/tasks
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "projectId": "<projectId>",
  "title": "",
  "description": "Invalid task title",
  "priority": "High",
  "dueDate": "2026-06-01T00:00:00Z"
}
```

**400 Bad Request**

```json
{
  "statusCode": 400,
  "code": "validation.failed",
  "message": "Validation failed",
  "errors": [
    "'Title' must not be empty."
  ]
}
```

> **Warning:** if this returns **200 OK**, the runtime validation pipeline is not firing. The FluentValidation rules in `RequestValidationService.cs` are not executing. This is a critical bug.

---

### 6. Past due date

_Negative scenario — Expected failure: 400 Bad Request_

Replace `<projectId>` with a real project id from Part 1.

```http
POST /api/tasks
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "projectId": "<projectId>",
  "title": "Past due date task",
  "description": "Should fail",
  "priority": "High",
  "dueDate": "2020-01-01T00:00:00Z"
}
```

**400 Bad Request** — actual code depends on where validation fires (both are accepted):

```json
{
  "statusCode": 400,
  "code": "task.invalid_due_date",
  "message": "Due date cannot be in the past.",
  "errors": []
}
```

Accepted codes: `validation.failed` or `task.invalid_due_date`.

---

### 7. Invalid task status transition

_Negative scenario — Expected failure: 409 Conflict_

> **This is a negative test. Direct `Todo` → `Done` must fail.** The positive path is `Todo` → `InProgress` → `Done` (steps 9.1 and 9.2 in Part 1).

Create a **fresh task** (one that has never had its status changed, so its status is still `"Todo"`). Do not reuse the task from Part 1 — that task was already moved to `"Done"`.

```http
POST /api/tasks
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "projectId": "<projectId>",
  "title": "Transition test task",
  "priority": "Low",
  "dueDate": "2026-06-01T00:00:00Z"
}
```

Save the `id` from this response as `<freshTaskId>`. Then attempt to skip directly to `Done`:

```http
PATCH /api/tasks/<freshTaskId>/status
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "status": "Done"
}
```

**409 Conflict**

```json
{
  "statusCode": 409,
  "code": "task.invalid_status_transition",
  "message": "Task status transition is not allowed.",
  "errors": []
}
```

---

### 8. Wrong task id

_Negative scenario — Expected failure: 404 Not Found_

Use a non-existent or mismatched id in the URL. Replace `<wrongTaskId>` with any random GUID or a known `projectId`.

```http
PATCH /api/tasks/<wrongTaskId>/status
Authorization: Bearer <jwt-token>
Content-Type: application/json
```

```json
{
  "status": "InProgress"
}
```

**404 Not Found**

```json
{
  "statusCode": 404,
  "code": "task.not_found",
  "message": "Task not found.",
  "errors": []
}
```

> If you receive 404 during the **happy-path** status update (Part 1, step 9), possible causes:
> - You used a placeholder `<taskId>` from these docs instead of the real id returned by `POST /api/tasks`.
> - You used `<projectId>` or `<userId>` in the URL instead of the task's own `id`.
> - The task was created with a different user's token than the one you are sending now.

---

### 9. Ownership isolation

_Negative scenario — Expected failure: 404 Not Found_

Register a second user (User B) and obtain their token. Then use User A's `<projectId>` or `<taskId>` with User B's token, or vice versa.

```http
GET /api/projects/<userA-projectId>
Authorization: Bearer <userB-jwt-token>
```

**404 Not Found**

```json
{
  "statusCode": 404,
  "code": "project.not_found",
  "message": "Project not found.",
  "errors": []
}
```

Other codes for the same pattern: `task.not_found`, `comment.not_found`.

> The API returns **404** instead of **403** for resources owned by another user. This is intentional — returning 403 would reveal that the resource exists, which leaks data about other users. 404 gives no information.

---

## Part 3 — Troubleshooting

**Problem: `GET /api/auth/me` returns 401**

Possible causes:
- No `Authorization` header was sent.
- Token expired (default expiry: 60 minutes).
- Token was copied only partially — missing trailing characters.
- In Swagger Authorize popup, `Bearer eyJ…` was pasted instead of the raw token. Swagger then sends `Authorization: Bearer Bearer eyJ…`, which is invalid.

Fix:
1. Click the **Authorize** button in Swagger UI.
2. Click **Logout** to clear the current value.
3. Paste only the raw JWT — it starts with `eyJ` and contains no spaces or the word `Bearer`.
4. Click **Authorize**, then **Close**.

---

**Problem: `PATCH /api/tasks/<taskId>/status` returns 404**

Possible causes:
- You used a placeholder `<taskId>` from this document instead of the real `id` returned by `POST /api/tasks`.
- You used a `projectId` instead of `taskId` in the URL.
- You used a `userId` instead of `taskId` in the URL.
- The task was created with a different user's token than the one currently in use.

Fix: copy the exact `"id"` field from the `POST /api/tasks` response body and use it in the URL. Do not use any other field or a copied GUID from documentation.

---

**Problem: `PATCH /api/tasks/<taskId>/status` returns 409**

Cause: the requested status transition is not allowed by business rules.

Common example: sending `"status": "Done"` when the task is still in `"Todo"`. Direct `Todo` → `Done` is always rejected.

Fix: follow the positive flow — send `"InProgress"` first (Part 1, step 9.1), then `"Done"` (Part 1, step 9.2).

---

**Problem: `POST /api/tasks` fails with an enum-related error**

Cause: the API is not configured to accept string enum values. The API and docs use string enums — `"High"`, `"InProgress"`, `"Done"` — not integers.

Fix: ensure `Program.cs` registers `JsonStringEnumConverter`:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

A correctly configured API accepts `"priority": "High"` and returns `"status": "Todo"`.

---

## Part 4 — Reviewer Clarity Checklist

Use this checklist before running examples through Swagger UI or Postman to avoid the most common mistakes.

- [ ] I registered or logged in successfully and have a valid token.
- [ ] I copied the full token value — no leading/trailing spaces, no missing characters.
- [ ] In Swagger Authorize, I pasted the raw JWT only — I did **not** type or paste the `Bearer ` prefix.
- [ ] I saved the real `<projectId>` from the `POST /api/projects` response — I am not using a placeholder GUID.
- [ ] I saved the real `<taskId>` from the `POST /api/tasks` response — I am not using a placeholder GUID.
- [ ] I did not use any fake GUIDs from the documentation examples.
- [ ] I ran the full happy path (Part 1, steps 1–12) before attempting any negative tests.
- [ ] I understand that direct `Todo` → `Done` is a **negative scenario** (Part 2, scenario 7) and should return **409 Conflict**, not 200.

---

See also [API.md](API.md) for the full endpoint list.
