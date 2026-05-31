# FlowBoard: Architecture Diagrams

This document contains the full set of architectural diagrams for FlowBoard,
written in Mermaid so they render directly in GitHub, Rider, and most Markdown
viewers. Referenced by `flowboard-development-roadmap.md`.

Diagrams follow the **C4 model** convention (Context → Container → Component)
plus sequence, entity-relationship, and deployment diagrams.

---

## 1. System Context Diagram (C4 Level 1)

Shows FlowBoard as a single system and its relationships with users and external
systems.

```mermaid
graph TB
    subgraph users[People]
        owner["👤 Organisation Owner<br/>Manages org, billing, members"]
        member["👤 Member<br/>Creates and manages tasks"]
        guest["👤 Guest<br/>Read-only project access"]
    end

    flowboard["🟦 FlowBoard<br/><br/>Multi-tenant SaaS project<br/>management platform"]

    subgraph external[External Systems]
        email["📧 Email Provider<br/>SMTP delivery"]
        storage["🗄️ Object Storage<br/>S3-compatible file storage"]
        sentry["🐞 Sentry<br/>Error tracking"]
    end

    owner -->|"Uses via browser (HTTPS)"| flowboard
    member -->|"Uses via browser (HTTPS)"| flowboard
    guest -->|"Uses via browser (HTTPS)"| flowboard

    flowboard -->|"Sends notification<br/>and invitation emails"| email
    flowboard -->|"Stores and retrieves<br/>file attachments"| storage
    flowboard -->|"Reports unhandled<br/>exceptions"| sentry

    style flowboard fill:#1168bd,stroke:#0b4884,color:#fff
    style owner fill:#08427b,stroke:#052e56,color:#fff
    style member fill:#08427b,stroke:#052e56,color:#fff
    style guest fill:#08427b,stroke:#052e56,color:#fff
    style email fill:#999,stroke:#666,color:#fff
    style storage fill:#999,stroke:#666,color:#fff
    style sentry fill:#999,stroke:#666,color:#fff
```

---

## 2. Container Diagram (C4 Level 2)

Shows the high-level technology building blocks and how they communicate.

```mermaid
graph TB
    user["👤 User<br/>Web browser"]

    subgraph flowboard[FlowBoard System]
        spa["🖥️ Single-Page App<br/>React + TypeScript<br/>Served via nginx"]
        api["⚙️ API Application<br/>ASP.NET Core 10<br/>Modular monolith"]
        worker["🔄 Background Worker<br/>Hangfire<br/>(in-process or separate)"]

        db[("🗃️ Database<br/>PostgreSQL 16<br/>+ Row-Level Security")]
        cache[("⚡ Cache<br/>Redis<br/>Tokens, sessions, counts")]
        objectstore[("🗄️ Object Storage<br/>MinIO / S3")]
    end

    email["📧 Email Provider<br/>SMTP"]

    user -->|"HTTPS"| spa
    spa -->|"JSON over HTTPS<br/>/api/v1/*"| api

    api -->|"Reads/writes<br/>EF Core + Npgsql"| db
    api -->|"Reads/writes<br/>StackExchange.Redis"| cache
    api -->|"Pre-signed URLs"| objectstore
    api -->|"Enqueues jobs"| worker

    worker -->|"Processes outbox,<br/>sends emails"| db
    worker -->|"Sends emails"| email
    worker -->|"Cleans up files"| objectstore

    spa -.->|"Direct upload via<br/>pre-signed URL"| objectstore

    style spa fill:#1168bd,stroke:#0b4884,color:#fff
    style api fill:#1168bd,stroke:#0b4884,color:#fff
    style worker fill:#1168bd,stroke:#0b4884,color:#fff
    style db fill:#2d882d,stroke:#1d5c1d,color:#fff
    style cache fill:#882d2d,stroke:#5c1d1d,color:#fff
    style objectstore fill:#88882d,stroke:#5c5c1d,color:#fff
    style user fill:#08427b,stroke:#052e56,color:#fff
    style email fill:#999,stroke:#666,color:#fff
```

---

## 3. Component Diagram (C4 Level 3): Modular Monolith Internals

Shows the bounded contexts within the API and how they relate. Modules never
reference each other directly, they communicate only via domain events through
the outbox and the shared kernel.

```mermaid
graph TB
    subgraph api[API Application - Modular Monolith]
        subgraph presentation[Presentation Layer]
            controllers["Controllers<br/>Auth, Users, Organisations,<br/>Projects, Tasks, Comments,<br/>Notifications, Search"]
            middleware["Middleware<br/>ExceptionHandling<br/>TenantResolution"]
        end

        subgraph modules[Bounded Context Modules]
            identity["🔐 Identity<br/>User, auth, JWT"]
            orgs["🏢 Organisations<br/>Org, membership, roles"]
            projects["📁 Projects<br/>Project, members"]
            tasks["✅ Tasks<br/>Task, comment, attachment"]
            notifications["🔔 Notifications<br/>In-app + email"]
            activity["📜 ActivityLog<br/>Append-only audit"]
            search["🔍 Search<br/>Full-text search"]
        end

        subgraph shared[Shared Kernel & Cross-Cutting]
            domain["Domain Primitives<br/>Entity, AggregateRoot,<br/>ValueObject, Result,<br/>strongly-typed IDs"]
            pipeline["MediatR Pipeline<br/>Logging → Validation →<br/>Performance → Transaction"]
            outbox["Outbox Processor<br/>Dispatches domain events"]
        end
    end

    controllers --> pipeline
    pipeline --> identity
    pipeline --> orgs
    pipeline --> projects
    pipeline --> tasks
    pipeline --> notifications
    pipeline --> search

    identity -.->|"domain events"| outbox
    orgs -.->|"domain events"| outbox
    projects -.->|"domain events"| outbox
    tasks -.->|"domain events"| outbox

    outbox -.->|"MemberInvited,<br/>TaskAssigned,<br/>UserMentioned"| notifications
    outbox -.->|"all events"| activity

    identity --> domain
    orgs --> domain
    projects --> domain
    tasks --> domain
    notifications --> domain
    activity --> domain
    search --> domain

    style identity fill:#1168bd,stroke:#0b4884,color:#fff
    style orgs fill:#1168bd,stroke:#0b4884,color:#fff
    style projects fill:#1168bd,stroke:#0b4884,color:#fff
    style tasks fill:#1168bd,stroke:#0b4884,color:#fff
    style notifications fill:#5d3a9b,stroke:#3d2666,color:#fff
    style activity fill:#5d3a9b,stroke:#3d2666,color:#fff
    style search fill:#5d3a9b,stroke:#3d2666,color:#fff
    style domain fill:#2d882d,stroke:#1d5c1d,color:#fff
    style pipeline fill:#882d2d,stroke:#5c1d1d,color:#fff
    style outbox fill:#88882d,stroke:#5c5c1d,color:#fff
```

---

## 4. Clean Architecture Layer Dependencies

The dependency rule: arrows point inward. Inner layers know nothing about outer
layers. This is enforced by the architecture tests.

```mermaid
graph TB
    subgraph outer[" "]
        presentation["Presentation<br/>Controllers, Middleware<br/>(depends on Application)"]
        infrastructure["Infrastructure<br/>EF Core, Redis, Hangfire,<br/>Storage, Email<br/>(implements Application interfaces)"]
    end

    application["Application<br/>Commands, Queries, Handlers,<br/>Validators, Pipeline Behaviours,<br/>Abstractions (interfaces)"]

    domain["Domain<br/>Aggregates, Entities,<br/>Value Objects, Domain Events,<br/>Invariants<br/><br/>ZERO external dependencies"]

    presentation -->|depends on| application
    infrastructure -->|implements interfaces from| application
    application -->|depends on| domain

    style domain fill:#2d882d,stroke:#1d5c1d,color:#fff
    style application fill:#1168bd,stroke:#0b4884,color:#fff
    style presentation fill:#882d2d,stroke:#5c1d1d,color:#fff
    style infrastructure fill:#88882d,stroke:#5c5c1d,color:#fff
```

---

## 5. Sequence Diagram: User Login Flow

```mermaid
sequenceDiagram
    actor U as User
    participant SPA as React SPA
    participant API as AuthController
    participant M as MediatR Pipeline
    participant H as LoginUserHandler
    participant DB as PostgreSQL
    participant R as Redis

    U->>SPA: Enter email + password
    SPA->>API: POST /api/v1/auth/login
    API->>M: Send(LoginUserCommand)

    Note over M: Logging → Validation →<br/>Performance behaviours run
    M->>H: Handle(command)

    H->>DB: Find user by email
    DB-->>H: User aggregate
    H->>H: Verify password (Argon2id)
    H->>H: Check IsEmailVerified

    alt Invalid credentials
        H-->>API: 401 Unauthorized
        API-->>SPA: Problem Details
    else Email not verified
        H-->>API: 403 Forbidden
        API-->>SPA: Problem Details
    else Success
        H->>H: Generate RS256 JWT (15min)
        H->>H: Generate refresh token
        H->>R: Store refresh:{token} → userId (30d TTL)
        H->>DB: user.RecordLogin()
        H-->>API: accessToken + refreshToken
        API-->>SPA: 200 OK + httpOnly refresh cookie
        SPA->>SPA: Store access token in memory
        SPA-->>U: Redirect to dashboard
    end
```

---

## 6. Sequence Diagram: Create Task with Outbox & Notification

This shows the full Transactional Outbox Pattern: the command, the transaction,
the outbox write, and the asynchronous event dispatch that creates a notification.

```mermaid
sequenceDiagram
    actor U as User
    participant API as TasksController
    participant TB as TransactionBehaviour
    participant H as AssignTaskHandler
    participant Agg as Task Aggregate
    participant UoW as UnitOfWork
    participant DB as PostgreSQL
    participant OP as OutboxProcessor<br/>(Hangfire)
    participant NH as Notification<br/>EventHandler

    U->>API: POST /tasks/{id}/assign
    API->>TB: Send(AssignTaskCommand)

    TB->>UoW: BeginTransactionAsync()
    UoW->>DB: BEGIN + SET LOCAL org_id

    TB->>H: Handle(command)
    H->>Agg: task.Assign(assigneeId)
    Agg->>Agg: Validate assignee is org member
    Agg->>Agg: Raise TaskAssignedEvent
    H-->>TB: Result

    TB->>UoW: SaveChangesAsync()
    UoW->>UoW: Collect domain events
    UoW->>DB: INSERT task change
    UoW->>DB: INSERT outbox_message (TaskAssignedEvent)
    TB->>UoW: CommitTransactionAsync()
    UoW->>DB: COMMIT

    API-->>U: 200 OK

    Note over OP: Runs every 5 seconds
    OP->>DB: SELECT unprocessed outbox messages
    DB-->>OP: TaskAssignedEvent
    OP->>NH: Publish via MediatR
    NH->>DB: INSERT notification (task_assigned)
    OP->>DB: UPDATE outbox SET processed_at
```

---

## 7. Sequence Diagram: File Attachment Upload (Pre-Signed URL)

No file bytes pass through the API server.

```mermaid
sequenceDiagram
    actor U as User
    participant SPA as React SPA
    participant API as AttachmentsController
    participant S3 as Object Storage<br/>(MinIO / S3)
    participant DB as PostgreSQL

    U->>SPA: Drag file onto task
    SPA->>SPA: Validate size ≤ 25MB, MIME type

    SPA->>API: POST /attachments/upload-url<br/>(filename, size, type)
    API->>API: Validate, generate storage key<br/>{orgId}/{taskId}/{uuid}/{file}
    API->>S3: Generate pre-signed PUT URL (5min)
    S3-->>API: Pre-signed URL
    API-->>SPA: { uploadUrl, storageKey }

    SPA->>S3: PUT file directly (bytes never touch API)
    S3-->>SPA: 200 OK

    SPA->>API: POST /attachments/confirm (storageKey)
    API->>S3: Verify object exists
    S3-->>API: Confirmed
    API->>DB: INSERT attachment metadata
    API-->>SPA: 201 Created
    SPA-->>U: Attachment appears on task
```

---

## 8. Entity Relationship Diagram

The full database schema. Append-only tables (`activity_logs`, `outbox_messages`)
have no soft-delete or update columns by design.

```mermaid
erDiagram
    USERS ||--o{ MEMBERSHIPS : "is member via"
    USERS ||--o{ ORGANISATIONS : "owns"
    ORGANISATIONS ||--o{ MEMBERSHIPS : "has"
    ORGANISATIONS ||--o{ INVITATIONS : "sends"
    ORGANISATIONS ||--o{ PROJECTS : "owns"
    PROJECTS ||--o{ PROJECT_MEMBERS : "grants access via"
    USERS ||--o{ PROJECT_MEMBERS : "has access to"
    PROJECTS ||--o{ TASKS : "contains"
    TASKS ||--o{ COMMENTS : "has"
    TASKS ||--o{ FILE_ATTACHMENTS : "has"
    USERS ||--o{ TASKS : "is assigned"
    USERS ||--o{ COMMENTS : "authors"
    USERS ||--o{ NOTIFICATIONS : "receives"
    ORGANISATIONS ||--o{ ACTIVITY_LOGS : "records"
    ORGANISATIONS ||--o{ OUTBOX_MESSAGES : "generates"

    USERS {
        uuid id PK
        text email UK
        text password_hash
        text display_name
        text avatar_url
        boolean is_email_verified
        timestamptz created_at
        timestamptz updated_at
        timestamptz last_login_at
        timestamptz deleted_at
    }

    ORGANISATIONS {
        uuid id PK
        text name
        text slug UK
        uuid owner_id FK
        timestamptz created_at
        timestamptz updated_at
        timestamptz deleted_at
    }

    MEMBERSHIPS {
        uuid id PK
        uuid user_id FK
        uuid organisation_id FK
        text role
        uuid invited_by_id FK
        timestamptz joined_at
    }

    INVITATIONS {
        uuid id PK
        uuid organisation_id FK
        text invited_email
        uuid invited_by_id FK
        text token_hash UK
        text role
        timestamptz expires_at
        timestamptz accepted_at
        timestamptz created_at
    }

    PROJECTS {
        uuid id PK
        uuid organisation_id FK
        text name
        text description
        text status
        uuid created_by_id FK
        timestamptz created_at
        timestamptz updated_at
        timestamptz deleted_at
    }

    PROJECT_MEMBERS {
        uuid id PK
        uuid project_id FK
        uuid user_id FK
        timestamptz added_at
    }

    TASKS {
        uuid id PK
        uuid project_id FK
        uuid organisation_id FK
        text title
        text description
        text status
        text priority
        uuid assignee_id FK
        uuid created_by_id FK
        timestamptz due_date
        tsvector search_vector
        timestamptz created_at
        timestamptz updated_at
        timestamptz deleted_at
    }

    COMMENTS {
        uuid id PK
        uuid task_id FK
        uuid organisation_id FK
        uuid author_id FK
        text content
        timestamptz created_at
        timestamptz updated_at
        timestamptz deleted_at
    }

    FILE_ATTACHMENTS {
        uuid id PK
        uuid task_id FK
        uuid organisation_id FK
        uuid uploaded_by_id FK
        text file_name
        bigint file_size_bytes
        text mime_type
        text storage_key
        timestamptz created_at
        timestamptz deleted_at
    }

    NOTIFICATIONS {
        uuid id PK
        uuid user_id FK
        uuid organisation_id FK
        text type
        text message
        text entity_type
        uuid entity_id
        boolean is_read
        timestamptz created_at
    }

    ACTIVITY_LOGS {
        uuid id PK
        uuid organisation_id FK
        text entity_type
        uuid entity_id
        text event_type
        uuid actor_id FK
        jsonb metadata
        timestamptz created_at
    }

    OUTBOX_MESSAGES {
        uuid id PK
        text aggregate_type
        uuid aggregate_id
        text event_type
        jsonb payload
        timestamptz created_at
        timestamptz processed_at
        text error
    }
```

---

## 9. Aggregate Boundaries

Shows which entities belong to which aggregate. The aggregate root is the only
entry point; child entities are accessed only through the root.

```mermaid
graph TB
    subgraph identity_bc[Identity Bounded Context]
        user_agg["🟢 User (Aggregate Root)<br/>+ Email, HashedPassword,<br/>DisplayName (value objects)"]
    end

    subgraph orgs_bc[Organisations Bounded Context]
        org_agg["🟢 Organisation (Aggregate Root)"]
        membership["🔵 Membership (child entity)"]
        invitation["🔵 Invitation (child entity)"]
        org_agg --> membership
        org_agg --> invitation
    end

    subgraph projects_bc[Projects Bounded Context]
        project_agg["🟢 Project (Aggregate Root)<br/>+ MemberIds (references)"]
    end

    subgraph tasks_bc[Tasks Bounded Context]
        task_agg["🟢 Task (Aggregate Root)"]
        comment["🔵 Comment (child entity)"]
        attachment["🔵 Attachment (child entity)"]
        task_agg --> comment
        task_agg --> attachment
    end

    subgraph notif_bc[Notifications Bounded Context]
        notif_agg["🟢 Notification (Aggregate Root)"]
    end

    subgraph activity_bc[ActivityLog Bounded Context]
        activity_entry["🟡 ActivityLogEntry<br/>(append-only, no aggregate)"]
    end

    style user_agg fill:#2d882d,stroke:#1d5c1d,color:#fff
    style org_agg fill:#2d882d,stroke:#1d5c1d,color:#fff
    style project_agg fill:#2d882d,stroke:#1d5c1d,color:#fff
    style task_agg fill:#2d882d,stroke:#1d5c1d,color:#fff
    style notif_agg fill:#2d882d,stroke:#1d5c1d,color:#fff
    style membership fill:#1168bd,stroke:#0b4884,color:#fff
    style invitation fill:#1168bd,stroke:#0b4884,color:#fff
    style comment fill:#1168bd,stroke:#0b4884,color:#fff
    style attachment fill:#1168bd,stroke:#0b4884,color:#fff
    style activity_entry fill:#88882d,stroke:#5c5c1d,color:#fff
```

---

## 10. MediatR Request Pipeline

Every command and query flows through this pipeline before reaching its handler.

```mermaid
graph LR
    request["Request<br/>(Command or Query)"]
    logging["LoggingBehaviour<br/>Log entry/exit + duration"]
    validation["ValidationBehaviour<br/>Run FluentValidation<br/>→ 422 on failure"]
    performance["PerformanceBehaviour<br/>Warn if > 500ms"]
    transaction["TransactionBehaviour<br/>Commands only:<br/>Begin → Commit/Rollback"]
    handler["Handler<br/>Business logic"]

    request --> logging
    logging --> validation
    validation --> performance
    performance --> transaction
    transaction --> handler

    handler -.->|response| transaction
    transaction -.-> performance
    performance -.-> validation
    validation -.-> logging
    logging -.->|response| request

    style request fill:#08427b,stroke:#052e56,color:#fff
    style logging fill:#1168bd,stroke:#0b4884,color:#fff
    style validation fill:#1168bd,stroke:#0b4884,color:#fff
    style performance fill:#1168bd,stroke:#0b4884,color:#fff
    style transaction fill:#882d2d,stroke:#5c1d1d,color:#fff
    style handler fill:#2d882d,stroke:#1d5c1d,color:#fff
```

---

## 11. Multi-Tenancy Isolation (Three Layers)

How tenant data isolation is enforced at every level.

```mermaid
graph TB
    request["Incoming Request<br/>with JWT containing org_id"]

    subgraph layer1[Layer 1 - Application]
        middleware["TenantResolutionMiddleware<br/>Extracts org_id from JWT,<br/>validates membership,<br/>populates ITenantContext"]
        repos["Repositories<br/>All queries scoped by<br/>WHERE organisation_id = @orgId"]
    end

    subgraph layer2[Layer 2 - Database]
        setlocal["UnitOfWork sets<br/>SET LOCAL app.current_organisation_id<br/>inside the transaction"]
        rls["PostgreSQL Row-Level Security<br/>Policies filter every row by<br/>app.current_organisation_id"]
    end

    subgraph layer3[Layer 3 - Tests]
        inttest["Integration tests<br/>Prove member of Org A<br/>cannot read Org B's data"]
    end

    request --> middleware
    middleware --> repos
    middleware --> setlocal
    setlocal --> rls
    repos --> rls
    rls -.->|verified by| inttest

    style request fill:#08427b,stroke:#052e56,color:#fff
    style middleware fill:#1168bd,stroke:#0b4884,color:#fff
    style repos fill:#1168bd,stroke:#0b4884,color:#fff
    style setlocal fill:#2d882d,stroke:#1d5c1d,color:#fff
    style rls fill:#2d882d,stroke:#1d5c1d,color:#fff
    style inttest fill:#882d2d,stroke:#5c1d1d,color:#fff
```

---

## 12. Deployment Diagram: Production

Production topology on a managed cloud provider (Railway / Render / Fly.io).

```mermaid
graph TB
    subgraph internet[Internet]
        users["👤 Users"]
        github["GitHub Actions<br/>CI/CD Pipeline"]
        ghcr["GitHub Container<br/>Registry (ghcr.io)"]
    end

    subgraph cloud[Cloud Provider]
        subgraph edge[Edge]
            lb["Load Balancer<br/>+ Managed TLS<br/>HTTPS only"]
        end

        subgraph compute[Compute]
            spa_container["Frontend Container<br/>nginx + React build"]
            api_container["API Container<br/>ASP.NET Core<br/>(horizontally scalable)"]
            worker_container["Worker Container<br/>Hangfire jobs"]
        end

        subgraph managed[Managed Services]
            pg[("Managed PostgreSQL<br/>+ automated backups")]
            redis[("Managed Redis")]
            bucket[("Object Storage Bucket<br/>S3-compatible")]
        end
    end

    subgraph monitoring[External Monitoring]
        sentry["Sentry<br/>Error tracking"]
        uptime["Uptime Monitor<br/>Pings /health/live"]
    end

    users -->|HTTPS| lb
    lb --> spa_container
    lb --> api_container

    api_container --> pg
    api_container --> redis
    api_container --> bucket
    worker_container --> pg
    worker_container --> redis
    worker_container --> bucket

    github -->|build, test, scan| ghcr
    ghcr -->|deploy on merge to main| api_container
    ghcr -->|deploy on merge to main| spa_container
    ghcr -->|deploy on merge to main| worker_container

    api_container -.->|errors| sentry
    spa_container -.->|errors| sentry
    uptime -.->|monitors| lb

    style lb fill:#1168bd,stroke:#0b4884,color:#fff
    style spa_container fill:#1168bd,stroke:#0b4884,color:#fff
    style api_container fill:#1168bd,stroke:#0b4884,color:#fff
    style worker_container fill:#1168bd,stroke:#0b4884,color:#fff
    style pg fill:#2d882d,stroke:#1d5c1d,color:#fff
    style redis fill:#882d2d,stroke:#5c1d1d,color:#fff
    style bucket fill:#88882d,stroke:#5c5c1d,color:#fff
    style users fill:#08427b,stroke:#052e56,color:#fff
    style github fill:#333,stroke:#000,color:#fff
    style ghcr fill:#333,stroke:#000,color:#fff
    style sentry fill:#999,stroke:#666,color:#fff
    style uptime fill:#999,stroke:#666,color:#fff
```

---

## Diagram Index

| # | Diagram | C4 Level | Purpose |
|---|---|---|---|
| 1 | System Context | L1 | FlowBoard and external actors |
| 2 | Container | L2 | Technology building blocks |
| 3 | Component | L3 | Modular monolith internals |
| 4 | Clean Architecture Layers |, | Dependency rule |
| 5 | Sequence, Login |, | Authentication flow |
| 6 | Sequence, Create Task + Outbox |, | Transactional outbox pattern |
| 7 | Sequence, File Upload |, | Pre-signed URL flow |
| 8 | Entity Relationship |, | Full database schema |
| 9 | Aggregate Boundaries |, | DDD aggregate design |
| 10 | MediatR Pipeline |, | Request processing |
| 11 | Multi-Tenancy Isolation |, | Three-layer tenant security |
| 12 | Deployment |, | Production cloud topology |
