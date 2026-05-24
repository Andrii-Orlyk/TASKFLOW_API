# Architecture

## Architecture type

TaskFlow API uses production-style layered architecture.

```text
Client / Swagger / Postman
        ↓
TaskFlow.Api
   ┌────┴──────────────────┐
   ↓                       ↓
TaskFlow.Application    TaskFlow.Infrastructure
   ↓                       ↓
TaskFlow.Domain         PostgreSQL
```

The diagram shows runtime composition, not reverse dependencies:
- `TaskFlow.Api` references Application and Infrastructure.
- `TaskFlow.Application` references Domain.
- `TaskFlow.Infrastructure` references Application and Domain.
- `TaskFlow.Domain` has no project dependencies.

## Layer responsibilities

### TaskFlow.Api

- Controllers
- Middleware
- Swagger
- Authentication configuration
- Authorization configuration
- CurrentUserService
- HTTP request/response boundary

### TaskFlow.Application

- Services
- DTOs
- Validators
- Interfaces
- Result and PagedResult
- Policies
- Strategies
- Factories
- Business use cases

### TaskFlow.Domain

- Entities
- Enums
- Common base classes
- Domain-level concepts

### TaskFlow.Infrastructure

- EF Core DbContext
- Entity configurations
- PostgreSQL persistence
- Repositories
- JWT token generation
- Password hashing
- Dependency injection registration

## Dependency direction

```text
TaskFlow.Api -> TaskFlow.Application
TaskFlow.Api -> TaskFlow.Infrastructure

TaskFlow.Application -> TaskFlow.Domain

TaskFlow.Infrastructure -> TaskFlow.Application
TaskFlow.Infrastructure -> TaskFlow.Domain

TaskFlow.Domain -> no dependencies
```

## Core business rules

1. User sees only own projects.
2. User can create tasks only in own projects.
3. User cannot read/update/delete another user's tasks.
4. Dashboard counts only current user's data.
5. DueDate cannot be in the past.
6. Done status sets CompletedAt.
7. Reopening a done task clears CompletedAt.
8. Comment cannot be empty.
