# Testing Plan

## Unit tests

Location: `tests/TaskFlow.UnitTests`

Covers services, policies, strategies, factories, and validators. Examples:

```text
CreateTask_ShouldFail_WhenDueDateIsInPast
CreateTask_ShouldFail_WhenProjectDoesNotBelongToCurrentUser
UpdateTaskStatus_ShouldSetCompletedAt_WhenStatusIsDone
UpdateTaskStatus_ShouldClearCompletedAt_WhenStatusChangesFromDone
TaskStatusPolicy_ShouldRejectInvalidTransition
PriorityStrategy_ShouldIncreasePriority_WhenDeadlineIsSoon
TaskFactory_ShouldCreateTaskWithDefaultTodoStatus
```

## Integration tests

Location: `tests/TaskFlow.IntegrationTests`

Uses `WebApplicationFactory` with HTTP calls. Examples:

```text
Register_ShouldReturnToken_WhenDataIsValid
Login_ShouldReturnToken_WhenCredentialsAreValid
Me_ShouldReturnCurrentUser_WhenTokenIsValid
GetProjects_ShouldReturnOnlyCurrentUserProjects
CreateTask_ShouldReturn404_WhenProjectBelongsToAnotherUser
GetTask_ShouldReturn404_WhenTaskBelongsToAnotherUser
Dashboard_ShouldCountOnlyCurrentUserData
```

## SQLite integration test limitation

Integration tests **do not use PostgreSQL**. `CustomWebApplicationFactory` replaces the DbContext with **SQLite in-memory** (`EnsureCreated` per test reset).

Implications:

- Fast, deterministic CI-friendly tests
- Schema is created from the EF model, not from PostgreSQL migration scripts
- Does **not** catch PostgreSQL-specific issues (types, constraints, Npgsql behavior)
- Local Docker / manual runs against Postgres remain the check for real database behavior

For portfolio honesty: mention SQLite in tests and PostgreSQL in runtime when discussing the project.

## Commands

```bash
dotnet restore
dotnet build
dotnet test
```

Release (matches CI):

```bash
dotnet build --no-restore --configuration Release
dotnet test --no-build --configuration Release
```

## Test quality rules

- Test business behavior, not internal implementation details.
- Do not delete tests to make build green.
- Keep tests deterministic.
- Include user data isolation tests before publishing to GitHub.
