# FlowBoard

A multi-tenant SaaS project management platform built as a .NET 10
modular monolith following Clean Architecture and DDD principles.

## Tech stack
- **Backend:** ASP.NET Core 10, EF Core, MediatR, FluentValidation
- **Database:** PostgreSQL 16 (with Row-Level Security)
- **Cache:** Redis
- **Storage:** S3-compatible (MinIO for local dev)
- **Jobs:** Hangfire
- **Observability:** Serilog, OpenTelemetry

## Getting started

### Prerequisites
- .NET 10 SDK
- Docker Desktop

### Run infrastructure
docker compose up -d

### Apply migrations
dotnet ef database update \
  --project src/FlowBoard.Infrastructure \
  --startup-project src/FlowBoard.Api

### Run the API
dotnet run --project src/FlowBoard.Api