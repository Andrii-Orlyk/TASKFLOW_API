# API Specification

## Auth

```http
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

## Projects

```http
GET    /api/projects
GET    /api/projects/{id}
POST   /api/projects
PUT    /api/projects/{id}
DELETE /api/projects/{id}
```

## Tasks

```http
GET    /api/tasks
GET    /api/tasks/{id}
POST   /api/tasks
PUT    /api/tasks/{id}
PATCH  /api/tasks/{id}/status
DELETE /api/tasks/{id}
```

Task filters:

```text
projectId
status
priority
overdueOnly
search
page
pageSize
```

## Comments

```http
GET    /api/tasks/{taskId}/comments
POST   /api/tasks/{taskId}/comments
DELETE /api/comments/{id}
```

## Dashboard

```http
GET /api/dashboard/summary
```

## Error response shape

All API errors use the same contract:

```json
{
  "statusCode": 400,
  "code": "validation.failed",
  "message": "Validation failed",
  "errors": [
    "Title is required.",
    "Due date cannot be in the past."
  ]
}
```

Business failures use application error codes (for example `project.not_found`, `auth.unauthorized`). Validation uses `validation.failed`. Unexpected server errors use `internal.error` with a generic message (no stack trace in the response).
