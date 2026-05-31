# FlowBoard: Backend Architecture

## 1. Architectural Style

The backend is a **modular monolith** following **Clean Architecture** and
**Domain-Driven Design (DDD)** principles. Each bounded context is a self-contained
module with its own domain, application, infrastructure, and API layers.

The modular monolith is chosen deliberately over microservices at this stage:

- Simpler deployment and local development
- Shared transactional boundaries where needed
- Module boundaries are explicit enough that extraction to services remains an option later
- No distributed systems overhead while the domain is still being discovered

---

## 2. Bounded Contexts / Modules

The system is divided into the following bounded contexts. Each maps to a top-level module:

| Module | Responsibility |
|---|---|
| **Identity** | User accounts, authentication, JWT issuance, password management |
| **Organisations** | Organisation lifecycle, membership, invitations, roles |
| **Projects** | Project lifecycle, project membership |
| **Tasks** | Tasks, comments, file attachments |
| **Notifications** | In-app and email notification fan-out |
| **ActivityLog** | Immutable event recording across all bounded contexts |
| **Search** | Full-text search across tasks, projects, and users |

Each module has **no direct dependency on another module's internals**. Cross-module
communication happens exclusively through:

1. **Domain events** published to an in-process event bus (MediatR `INotification`)
2. **Shared kernel types** (e.g. `OrganisationId`, `UserId` value objects defined in `Shared`)

---

## 3. Solution Structure

```
src/
  FlowBoard.Api/                    # HTTP entry point
    Controllers/
    Middleware/
      ExceptionHandlingMiddleware
      TenantResolutionMiddleware
    Program.cs
    appsettings.json

  FlowBoard.Application/            # Application layer (shared abstractions)
    Abstractions/
      IUnitOfWork.cs
      ICurrentUserService.cs
      ITenantContext.cs
      IEmailService.cs
      IStorageService.cs
    Behaviours/                     # MediatR pipeline behaviours
      ValidationBehaviour.cs
      LoggingBehaviour.cs
      AuthorisationBehaviour.cs
      PerformanceBehaviour.cs
      TransactionBehaviour.cs

  FlowBoard.Domain/                 # Domain layer (shared kernel)
    Primitives/
      Entity.cs                     # Base entity with Id and domain events
      AggregateRoot.cs
      ValueObject.cs
      DomainEvent.cs
      IDomainEventHandler.cs
    Shared/
      OrganisationId.cs
      UserId.cs
      Result.cs                     # Result<T> / Error pattern

  FlowBoard.Shared/
    Guards/
    Extensions/
    Utilities/

  Modules/
    Identity/
      Domain/
        User.cs                     # Aggregate root
        UserCreatedEvent.cs
        EmailVerifiedEvent.cs
        ValueObjects/
          Email.cs
          HashedPassword.cs
      Application/
        Commands/
          RegisterUser/
            RegisterUserCommand.cs
            RegisterUserCommandHandler.cs
            RegisterUserCommandValidator.cs
          LoginUser/
          VerifyEmail/
          ResetPassword/
        Queries/
          GetUserProfile/
        EventHandlers/
      Infrastructure/
        Persistence/
          UserRepository.cs
          UserConfiguration.cs     # EF Core Fluent API config
      Api/
        AuthController.cs
        UsersController.cs

    Organisations/
      Domain/
        Organisation.cs            # Aggregate root
        Membership.cs              # Entity within Organisation aggregate
        Invitation.cs              # Entity within Organisation aggregate
        Events/
          OrganisationCreatedEvent.cs
          MemberInvitedEvent.cs
          MemberJoinedEvent.cs
          MemberRemovedEvent.cs
          OwnershipTransferredEvent.cs
        ValueObjects/
          OrganisationName.cs
          MemberRole.cs
      Application/
        Commands/ ...
        Queries/ ...
        EventHandlers/ ...
      Infrastructure/
        Persistence/ ...
      Api/
        OrganisationsController.cs
        MembershipsController.cs

    Projects/
      Domain/
        Project.cs                 # Aggregate root
        Events/
          ProjectCreatedEvent.cs
          ProjectArchivedEvent.cs
        ValueObjects/
          ProjectStatus.cs
      Application/ ...
      Infrastructure/ ...
      Api/
        ProjectsController.cs

    Tasks/
      Domain/
        Task.cs                    # Aggregate root
        Comment.cs                 # Entity within Task aggregate
        Attachment.cs              # Entity within Task aggregate
        Events/
          TaskCreatedEvent.cs
          TaskAssignedEvent.cs
          TaskStatusChangedEvent.cs
          TaskPriorityChangedEvent.cs
          CommentAddedEvent.cs
          UserMentionedEvent.cs
        ValueObjects/
          TaskStatus.cs
          Priority.cs
          CommentContent.cs
      Application/ ...
      Infrastructure/ ...
      Api/
        TasksController.cs
        CommentsController.cs
        AttachmentsController.cs

    Notifications/
      Domain/
        Notification.cs            # Aggregate root
        ValueObjects/
          NotificationType.cs
      Application/
        Commands/
          CreateNotification/
          MarkAsRead/
        EventHandlers/
          TaskAssignedEventHandler.cs
          UserMentionedEventHandler.cs
          MemberInvitedEventHandler.cs
        Jobs/
          SendEmailNotificationJob.cs
      Infrastructure/ ...
      Api/
        NotificationsController.cs

    ActivityLog/
      Domain/
        ActivityLogEntry.cs        # No aggregate, append-only entity
        ValueObjects/
          EntityType.cs
          EventType.cs
      Application/
        EventHandlers/
          TaskCreatedActivityHandler.cs
          TaskAssignedActivityHandler.cs
          # ... one handler per domain event
      Infrastructure/
        Persistence/
          ActivityLogRepository.cs
      Api/
        ActivityLogController.cs

    Search/
      Application/
        Queries/
          GlobalSearch/
          SearchTasks/
      Infrastructure/
        Persistence/
          SearchRepository.cs      # Uses Postgres tsvector queries
      Api/
        SearchController.cs

  FlowBoard.Infrastructure/        # Shared infrastructure
    Persistence/
      AppDbContext.cs
      UnitOfWork.cs
      Migrations/
    Outbox/
      OutboxMessage.cs
      OutboxProcessor.cs           # Hangfire job
    Caching/
      RedisCacheService.cs
    Storage/
      S3StorageService.cs
    Email/
      SmtpEmailService.cs
    Jobs/
      HangfireJobSetup.cs
      HardDeleteJob.cs
      ExpireInvitationsJob.cs
```

---

## 4. MediatR Pipeline Behaviours

All commands and queries pass through a MediatR pipeline. Behaviours execute in the
following order:

```
Request
  → LoggingBehaviour          (log request entry/exit, duration)
  → AuthorisationBehaviour    (policy-based authorisation check)
  → ValidationBehaviour       (FluentValidation, returns 422 on failure)
  → PerformanceBehaviour      (log warning if handler > 500ms)
  → TransactionBehaviour      (wraps Commands in a DB transaction; skips Queries)
  → Handler
```

**`ValidationBehaviour`** runs all registered `IValidator<TRequest>` implementations.
If any validator fails, a `ValidationException` is thrown and caught by the global
exception middleware, the pipeline never reaches the handler.

**`TransactionBehaviour`** applies only to commands (requests implementing `ICommand`).
It wraps the handler execution in a database transaction and, on successful commit, also
commits the outbox messages written during the handler.

---

## 5. Domain Event & Outbox Pattern

Domain events are the mechanism by which modules communicate and side effects are
triggered without direct coupling.

### Flow

```
Command Handler
  → Aggregate raises domain event (stored on aggregate in-memory)
  → Repository.Save() is called
  → UnitOfWork.SaveChangesAsync() fires:
      1. Serialises domain events to outbox_messages table (same DB transaction)
      2. Commits the transaction
  → OutboxProcessor (Hangfire, runs every 5s):
      1. Queries unprocessed outbox_messages (with advisory lock to avoid duplication)
      2. Deserialises each event
      3. Publishes via MediatR IPublisher
      4. MediatR dispatches to all registered INotificationHandlers
      5. Marks outbox record as processed
```

This guarantees **at-least-once delivery**: if the application crashes between step 2 and
step 5, the outbox record remains unprocessed and will be retried. Handlers must be
**idempotent**.

### Domain Event Handlers (Cross-Module Examples)

| Event | Handler Module | Side Effect |
|---|---|---|
| `TaskAssignedEvent` | Notifications | Create `task_assigned` notification |
| `TaskAssignedEvent` | ActivityLog | Record `task.assigned` log entry |
| `UserMentionedEvent` | Notifications | Create `mentioned` notification |
| `MemberInvitedEvent` | Notifications | Enqueue invitation email job |
| `MemberInvitedEvent` | ActivityLog | Record `member.invited` log entry |
| `TaskStatusChangedEvent` | ActivityLog | Record `task.status_changed` log entry |
| `ProjectArchivedEvent` | ActivityLog | Record `project.archived` log entry |

---

## 6. Authentication & Authorisation

### Token Strategy

- **Access Token**: JWT, signed RS256, 15-minute lifetime.
  Claims: `sub` (userId), `org_id`, `role`, `email`, `jti`.
- **Refresh Token**: Opaque random string, stored as a Redis key with 30-day TTL.
  Rotated on every use (old token is deleted atomically when issuing the new one).

### Revocation

Access tokens cannot be revoked before expiry. A 15-minute window is acceptable.
If immediate revocation is needed (e.g. logout, role change), the `jti` claim is
added to a Redis blocklist TTL'd to the access token's remaining lifetime.

### Authorisation Model

Authorisation uses a combination of:

1. **JWT role claim**, coarse-grained gate (e.g. "must be Admin or Owner")
2. **Policy-based authorisation** via ASP.NET Core `IAuthorizationHandler`, fine-grained
   checks (e.g. "must be a member of this specific organisation")
3. **MediatR `AuthorisationBehaviour`**, command/query level authorisation using
   `IAuthorisationRequirement` per request type

---

## 7. Tenant Isolation Middleware

The `TenantResolutionMiddleware` runs on every request and:

1. Extracts `org_id` from the JWT claims
2. Validates the user is an active member of that organisation
3. Populates `ITenantContext.CurrentOrganisationId`
4. Sets the Postgres session parameter: `SET app.current_organisation_id = '{orgId}'`

All repositories read `ITenantContext.CurrentOrganisationId` and append
`WHERE organisation_id = @orgId` to every query. RLS enforces this at the database level
as a defence-in-depth measure.

---

## 8. Error Handling

A single `ExceptionHandlingMiddleware` at the top of the pipeline catches all unhandled
exceptions and maps them to RFC 7807 problem detail responses:

| Exception Type | HTTP Status |
|---|---|
| `ValidationException` | 422 Unprocessable Entity |
| `NotFoundException` | 404 Not Found |
| `UnauthorisedException` | 401 Unauthorized |
| `ForbiddenException` | 403 Forbidden |
| `ConflictException` | 409 Conflict |
| All others | 500 Internal Server Error |

All 5xx responses log the full exception with `TraceId` via Serilog. The response body
never includes a stack trace in production.

---

## 9. Caching

Redis is used for:

- Refresh token storage (key: `refresh:{token}`, value: `userId`)
- JWT blocklist (key: `blocklist:{jti}`, TTL: token remaining lifetime)
- Organisation membership cache (key: `{orgId}:membership:{userId}`, TTL: 5 min)
- Notification unread count (key: `{userId}:notifications:unread`, invalidated on write)
- Dashboard summary (key: `{userId}:dashboard`, TTL: 2 min)

Caching is abstracted behind `ICacheService` to allow swapping implementations in tests.

---

## 10. Observability

### Structured Logging

Serilog is configured with:
- Console sink (JSON in production, human-readable in development)
- `RequestLoggingMiddleware` (Serilog.AspNetCore) for HTTP request/response logs
- Enrichers: `TraceId`, `UserId`, `OrganisationId`, `MachineName`, `Environment`

### Distributed Tracing

OpenTelemetry SDK with traces exported to Jaeger (or OTLP endpoint). All outbound
HTTP calls, EF Core queries, and Redis operations are instrumented.

### Metrics

OpenTelemetry Metrics exported to Prometheus. Key metrics:

- HTTP request duration histogram (by route, status code)
- Background job duration and failure rate
- Cache hit/miss rate
- Outbox queue depth

### Health Checks

| Endpoint | Checks |
|---|---|
| `GET /health/live` | Application is running |
| `GET /health/ready` | DB reachable, Redis reachable, storage reachable |

---

## 11. Recommended Libraries

| Purpose | Library |
|---|---|
| Mediator / CQRS | MediatR |
| Validation | FluentValidation |
| ORM | Entity Framework Core 8 |
| Caching | StackExchange.Redis |
| Background jobs | Hangfire |
| Logging | Serilog |
| Tracing / Metrics | OpenTelemetry .NET SDK |
| Auth | Microsoft.AspNetCore.Authentication.JwtBearer |
| API docs | Swashbuckle |
| Testing | xUnit + Moq + Testcontainers |
| Mapping | Mapster (preferred) or AutoMapper |
