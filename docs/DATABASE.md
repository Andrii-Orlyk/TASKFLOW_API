# Database Model

## Tables

```text
Users
Projects
TaskItems
TaskComments
```

## Relationships

```text
User 1 -> many Projects
User 1 -> many TaskComments
Project 1 -> many TaskItems
TaskItem 1 -> many TaskComments
```

## Ownership hierarchy

```text
User -> Project -> TaskItem -> TaskComment
```

This hierarchy is used for authorization and user data isolation.

## Entities

### Users

```text
Id PK
Email UNIQUE
PasswordHash
FirstName
LastName
Role
CreatedAt
UpdatedAt
```

### Projects

```text
Id PK
Name
Description
OwnerId FK -> Users.Id
CreatedAt
UpdatedAt
```

### TaskItems

```text
Id PK
ProjectId FK -> Projects.Id
Title
Description
Status
Priority
DueDate
CompletedAt
CreatedAt
UpdatedAt
```

### TaskComments

```text
Id PK
TaskItemId FK -> TaskItems.Id
AuthorId FK -> Users.Id
Content
CreatedAt
```

## Indexes

Recommended indexes:

```text
Users.Email unique
TaskItems.ProjectId
TaskItems.Status
TaskItems.Priority
TaskItems.DueDate
```
