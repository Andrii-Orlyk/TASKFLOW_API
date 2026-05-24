# Authentication and Authorization

## Auth flow

### Register

1. Validate request.
2. Check email uniqueness.
3. Hash password.
4. Create user.
5. Save user.
6. Generate JWT.
7. Return AuthResponse.

### Login

1. Find user by email.
2. Verify password.
3. Generate JWT.
4. Return AuthResponse.

### Me

1. Read current user id from JWT claims.
2. Load user.
3. Return CurrentUserResponse.

## JWT claims

Required claims:

```text
NameIdentifier
Email
Role
firstName
lastName
```

## Protected endpoints

All business endpoints must use `[Authorize]`:

```text
/projects
/tasks
/comments
/dashboard
/auth/me
```

## Security rules

- Never store plain text passwords.
- Do not return PasswordHash in API responses.
- Do not leak whether email or password specifically is wrong during login.
- Use 404 for foreign resource access to avoid revealing resource existence.
- Keep JWT secret out of Git.
