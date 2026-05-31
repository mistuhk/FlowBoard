# FlowBoard: Comprehensive Requirements Document

## 1. Overview

This document defines the functional and non-functional requirements for **FlowBoard**, a
multi-tenant SaaS project management platform. The system enables teams and organisations
to collaborate, manage projects, track tasks, and monitor activity through a modern web
application.

### Technology Stack

| Layer | Technology |
|---|---|
| Frontend | React + TypeScript |
| Backend | ASP.NET Core Web API (.NET 8) |
| Database | PostgreSQL 16 |
| Caching | Redis |
| Object Storage | S3-compatible (AWS S3 / MinIO) |
| Background Jobs | Hangfire |
| Containerisation | Docker + Docker Compose |
| CI/CD | GitHub Actions |

---

## 2. Multi-Tenancy Strategy

This is the most critical architectural decision for a SaaS platform.

FlowBoard uses a **shared database, shared schema** model with **organisation-scoped row isolation**.

### Rationale

A shared schema approach is chosen over per-tenant schema or per-tenant database because:
- It minimises operational overhead at this stage of the product
- It allows cost-effective scaling of a large number of small tenants
- It is the most common pattern for early-stage SaaS products

### Isolation Enforcement

Tenant isolation is enforced at **three layers**:

1. **Application layer**: all repository queries are scoped by `OrganisationId`, which is resolved from the authenticated user's JWT claims. This is enforced via a `TenantContext` service injected into all repositories.

2. **PostgreSQL Row-Level Security (RLS)**: RLS policies are defined on all tenant-scoped tables. The application sets `app.current_organisation_id` as a session-level parameter before executing queries, and RLS policies enforce that only rows matching that value are visible.

3. **Integration tests**: cross-tenant data leak scenarios must be covered by automated tests as part of the CI pipeline.

### Tenant Resolution

The `OrganisationId` is resolved from the authenticated user's JWT access token. Every API request targeting tenant-scoped resources must include the organisation context, either:
- Embedded in the JWT claim (`org_id`), or
- Provided as a route segment: `/api/v1/organisations/{organisationId}/...`

Route-based organisation scoping is preferred for its explicitness.

---

## 3. User Roles & Permissions

### 3.1 Role Hierarchy

| Role | Scope | Capabilities |
|---|---|---|
| **Owner** | Organisation | Full control including deletion, billing, ownership transfer |
| **Admin** | Organisation | Manage projects, manage members, manage settings |
| **Member** | Organisation | Create and manage tasks, participate in projects |
| **Guest** | Project-level only | Read-only access to explicitly shared projects |

### 3.2 Permission Matrix

| Action | Owner | Admin | Member | Guest |
|---|---|---|---|---|
| Delete organisation | ✅ | ❌ | ❌ | ❌ |
| Transfer ownership | ✅ | ❌ | ❌ | ❌ |
| Invite / remove members | ✅ | ✅ | ❌ | ❌ |
| Assign roles | ✅ | ✅ | ❌ | ❌ |
| Create / archive projects | ✅ | ✅ | ❌ | ❌ |
| Manage project members | ✅ | ✅ | ❌ | ❌ |
| Create / update tasks | ✅ | ✅ | ✅ | ❌ |
| Delete tasks | ✅ | ✅ | Own only | ❌ |
| Add / edit comments | ✅ | ✅ | ✅ | ❌ |
| View project content | ✅ | ✅ | ✅ | ✅ |

### 3.3 Invariants

- An organisation must always have **exactly one** Owner.
- Ownership transfer is the only mechanism by which the Owner role may move from one user to another.
- A user cannot be removed from an organisation if they are the current Owner; ownership must be transferred first.
- A Guest can only access projects they have been explicitly added to.

---

## 4. API Design Standards

### 4.1 Versioning

All API routes are prefixed with `/api/v1/`. Breaking changes require a new version prefix. Backwards-compatible changes may be introduced within the current version.

### 4.2 Resource URL Convention

APIs follow a **resource-scoped hierarchy** rooted at the organisation:

```
/api/v1/organisations/{orgId}/projects
/api/v1/organisations/{orgId}/projects/{projectId}/tasks
/api/v1/organisations/{orgId}/projects/{projectId}/tasks/{taskId}/comments
```

### 4.3 Pagination Contract

All list endpoints use **cursor-based pagination** (preferred over offset for large datasets and real-time data):

**Request parameters:**

| Parameter | Type | Description |
|---|---|---|
| `cursor` | string (opaque) | Cursor from previous response. Omit for first page. |
| `limit` | integer | Items per page. Default: `25`. Max: `100`. |
| `sort` | string | Sort field. Default: `created_at`. |
| `direction` | `asc` \| `desc` | Sort direction. Default: `desc`. |

**Response envelope:**

```json
{
  "data": [...],
  "pagination": {
    "nextCursor": "eyJpZCI6IjEyMyJ9",
    "hasNextPage": true,
    "limit": 25
  }
}
```

### 4.4 Error Response Contract

All errors return a consistent problem details envelope (RFC 7807):

```json
{
  "type": "https://flowboard.io/errors/validation-failed",
  "title": "Validation Failed",
  "status": 422,
  "detail": "One or more fields failed validation.",
  "errors": {
    "title": ["Title is required.", "Title must not exceed 255 characters."]
  },
  "traceId": "00-4bf5122f344554c61...–01"
}
```

### 4.5 Soft Delete Behaviour

Deleted resources are **soft-deleted** using a `deleted_at` timestamp. Soft-deleted records are excluded from all standard queries. No resource exposes a hard delete endpoint to callers. Hard deletion is performed by a background maintenance job after a configurable retention period (default: 30 days).

### 4.6 API Documentation

Swagger/OpenAPI documentation is auto-generated via `Swashbuckle`. All endpoints must have XML doc comments on controller actions. The docs UI is exposed at `/swagger` in non-production environments.

---

## 5. Functional Requirements

### 5.1 Authentication & Identity

Users must be able to:

- Register with email and password
- Verify their email address before accessing the platform
- Log in with email/password
- Request a password reset via email
- Log out (access token revoked via server-side token blocklist in Redis)
- Update their profile (display name, avatar)
- Delete their account (GDPR right to erasure, see Section 11)

**Token strategy:**

- **Access token**: Short-lived JWT (15 minutes). Signed with RS256. Contains `user_id`, `org_id`, `role` claims.
- **Refresh token**: Long-lived opaque token (30 days). Stored server-side in Redis. Rotated on use.

**Future / optional:**

- OAuth 2.0 login (Google, GitHub) via OpenID Connect
- TOTP-based two-factor authentication

---

### 5.2 Organisation Management

Users must be able to:

- Create an organisation (creator becomes Owner automatically)
- Rename an organisation
- Invite members by email (generates a time-limited invitation token, 48 hours)
- Accept or decline an invitation
- Remove members
- Assign and change member roles (below their own role level)
- View all members and their roles

Owners can additionally:

- Transfer ownership to another Admin or Member
- Delete the organisation (soft delete; hard delete after retention period)

---

### 5.3 Project Management

Admins and Owners can:

- Create a project within an organisation
- Edit project name and description
- Archive a project (archived projects are read-only)
- Restore an archived project
- Add and remove project members
- Soft-delete a project

All org members can view active projects they have been added to. Guests can view projects they have been explicitly granted access to.

---

### 5.4 Task Management

Members can:

- Create tasks within a project
- Edit task title, description, priority, and due date
- Update task status
- Assign a task to any member of the organisation
- Attach files to tasks
- Soft-delete tasks they created (Admins/Owners can delete any task)

**Task Statuses:** `Todo` → `In Progress` → `Blocked` → `Done`

**Task Priorities:** `Low`, `Medium`, `High`, `Critical`

**Business rules:**

- A task may only be assigned to a user who is a member of the task's organisation.
- When a task is assigned to a user, a `TaskAssigned` notification is created for that user.
- Changing a task's status fires a `TaskStatusChanged` domain event, which is recorded in the activity log.

---

### 5.5 Comments

Members can:

- Add a comment to any task they can access
- Edit their own comments
- Soft-delete their own comments (Admins/Owners can delete any comment)

Comments support:

- Markdown formatting (rendered client-side)
- `@mentions` using `@username` syntax. Mentioning a user fires a `UserMentioned` notification for each mentioned user.

---

### 5.6 Activity Feed

The system records an immutable activity log entry for all significant state changes:

| Event | Triggered by |
|---|---|
| `task.created` | Task creation |
| `task.status_changed` | Task status update |
| `task.assigned` | Task assignment |
| `task.priority_changed` | Task priority update |
| `project.created` | Project creation |
| `project.archived` | Project archival |
| `member.invited` | Member invitation sent |
| `member.joined` | Invitation accepted |
| `member.removed` | Member removed |
| `comment.added` | Comment posted |

Users can view:

- Project-level activity feed (scoped to a specific project)
- Personal activity feed (events involving the current user)

Activity log records are immutable, they may never be updated or deleted.

---

### 5.7 Notifications

Users receive in-app notifications for:

- Being assigned a task
- Being `@mentioned` in a comment
- Being invited to an organisation or project
- A task they are assigned to being moved to `Blocked`

Each notification includes a reference to the entity it concerns (`entity_type`, `entity_id`) so the frontend can deep-link directly.

**Email notifications** are dispatched asynchronously via a background job. Users can configure per-notification-type email preferences.

---

### 5.8 Search

Global search is implemented using **PostgreSQL full-text search** (`tsvector`). A dedicated `search_vector` generated column is maintained on `tasks` and `projects`. This is the preferred approach over a dedicated search service at this stage; the design should allow a future migration to Elasticsearch or Meilisearch without changes to the domain layer.

**Searchable entities:** Tasks, Projects, Users (by display name / email)

**Filter parameters (task search):**

| Filter | Values |
|---|---|
| `status` | `todo`, `in_progress`, `blocked`, `done` |
| `priority` | `low`, `medium`, `high`, `critical` |
| `assigneeId` | UUID |
| `dueBefore` | ISO 8601 datetime |
| `dueAfter` | ISO 8601 datetime |
| `projectId` | UUID |

All search results respect tenant scoping, a user can only find resources within their organisation.

---

### 5.9 Dashboard

The authenticated user's dashboard presents:

- Tasks assigned to them (grouped by status)
- Overdue tasks (past `due_date`, not yet `done`)
- Active projects they are a member of
- Recent activity (last 20 events across their organisation)

Visualisations (client-side, using chart library):

- Task completion rate over the last 30 days
- Workload distribution across team members (tasks by assignee)

Dashboard data is served by dedicated read-optimised query endpoints, not assembled from multiple general-purpose list endpoints.

---

### 5.10 File Attachments

Users may attach files to tasks. File handling follows this flow:

1. Client requests a **pre-signed upload URL** from the API: `POST /api/v1/organisations/{orgId}/attachments/upload-url`
2. Client uploads the file directly to object storage using the pre-signed URL (no file bytes pass through the API server)
3. Client confirms the upload: `POST /api/v1/organisations/{orgId}/attachments/confirm`
4. API persists the attachment metadata to the database

**Constraints:**

- Maximum file size: 25 MB
- Allowed MIME types are configurable
- Storage keys are namespaced per tenant: `{orgId}/{taskId}/{uuid}/{filename}`

---

### 5.11 Audit Logging

An immutable audit log records security and compliance-relevant events:

- Authentication events (login, logout, failed login, password reset)
- Permission changes (role assignments)
- Project and organisation deletions
- Membership changes

Audit log records must never be updated or deleted (not even soft-deleted). The table has no `updated_at` or `deleted_at` column by design.

---

## 6. Non-Functional Requirements

### 6.1 Performance

| Metric | Target |
|---|---|
| API p95 response time | < 200ms |
| API p99 response time | < 500ms |
| Dashboard load (cold) | < 1s |
| Search query response | < 300ms |

Response time targets exclude file upload/download endpoints.

### 6.2 Scalability

- API servers are stateless and horizontally scalable behind a load balancer
- Session/token state is held in Redis, not in-process memory
- Background job processors (Hangfire) can be scaled independently
- Database read replicas may be introduced for reporting queries without application changes

### 6.3 Security

- Passwords hashed with **Argon2id** (via ASP.NET Core Identity or a direct library)
- All communication over **HTTPS** only (HSTS enforced)
- Input validated at the application layer using FluentValidation
- SQL injection prevented by EF Core parameterised queries; raw SQL is prohibited unless reviewed
- OWASP Top 10 mitigations documented and tested
- Secrets managed via environment variables / secret manager, never committed to source control
- Access tokens are short-lived; refresh token rotation on every use
- Rate limiting applied to authentication endpoints

### 6.4 Reliability

- API errors are caught by a global exception-handling middleware that returns RFC 7807 problem details
- Background jobs implement automatic retry with exponential back-off (3 retries)
- Outbox pattern ensures domain events are not lost in the event of application failure after a database commit
- Database connection resilience via EF Core's built-in retry policy

### 6.5 Observability

| Concern | Implementation |
|---|---|
| Structured logging | Serilog with JSON output sink |
| Distributed tracing | OpenTelemetry → Jaeger (or cloud equivalent) |
| Metrics | OpenTelemetry Metrics → Prometheus / Grafana |
| Health checks | ASP.NET Core Health Checks (`/health/live`, `/health/ready`) |
| Error tracking | Sentry (or equivalent) |

All log entries include `TraceId`, `UserId`, and `OrganisationId` as structured properties.

### 6.6 Caching Strategy

| Data | Cache | TTL |
|---|---|---|
| User profile | Redis | 5 minutes |
| Organisation membership | Redis | 5 minutes |
| Notification count (unread) | Redis | Invalidated on write |
| Dashboard summary | Redis | 2 minutes |

Cache keys must be namespaced by tenant: `{orgId}:{resource}:{id}`.

---

## 7. Background Jobs

All background jobs are managed by **Hangfire** with a PostgreSQL backing store.

| Job | Trigger | Description |
|---|---|---|
| `ProcessOutboxMessages` | Polling (every 5s) | Reads unprocessed outbox records and dispatches domain events |
| `SendEmailNotification` | Event-driven (enqueued) | Sends email for a notification record |
| `HardDeleteExpiredRecords` | Daily | Hard-deletes soft-deleted records past retention window |
| `ProcessPendingInvitations` | Hourly | Expires invitation tokens older than 48 hours |

---

## 8. Infrastructure

### 8.1 Containers

- All services (API, database, Redis, Hangfire worker, MinIO) are defined in `docker-compose.yml` for local development
- Production containers are built as minimal, non-root images based on `mcr.microsoft.com/dotnet/aspnet`

### 8.2 CI/CD (GitHub Actions)

Pipeline stages:

1. **Build**, `dotnet build`
2. **Test**, `dotnet test` (unit + integration tests)
3. **Lint**, `dotnet format --verify-no-changes`
4. **Container build**, Build and tag Docker image
5. **Security scan**, Trivy image scanning
6. **Deploy**, Push to container registry and deploy to target environment

---

## 9. GDPR & Data Compliance

- Users have the **right to access** their data, a data export endpoint must be provided
- Users have the **right to erasure**, deletion anonymises personal data (name, email replaced with a tombstone value); audit log entries referencing the user retain only a `user_id` that no longer resolves to a real user
- Data retention windows are configurable per deployment
- Invitation emails must include an unsubscribe / opt-out mechanism

---

## 10. Future Extensions

The architecture must not preclude:

- Real-time collaboration via WebSockets or SSE
- Kanban board views
- Reporting and analytics (separate read model)
- AI task suggestions and auto-prioritisation
- Integrations with external tools (Slack, GitHub, Jira) via webhooks
- Billing and subscription management (Stripe)

---

## 11. Success Criteria

The project is considered production-ready when:

- All functional requirements in Section 5 are implemented and covered by tests
- All API endpoints are documented in OpenAPI
- Unit test coverage ≥ 80% on the Domain and Application layers
- Integration tests cover all critical user journeys
- The CI/CD pipeline passes on every merge to `main`
- The system is deployed to a cloud environment and reachable via a public URL
- Health check endpoints return `200 OK` in steady state
- No known P1 security vulnerabilities (Trivy scan clean)
