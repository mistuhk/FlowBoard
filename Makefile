.PHONY: up down logs ps migrate migration test build format clean help

## Start all local infrastructure services
up:
	docker compose up -d

## Stop all local infrastructure services
down:
	docker compose down

## Tail logs from all running services
logs:
	docker compose logs -f

## Show running service status
ps:
	docker compose ps

## Apply pending EF Core migrations to the local database
migrate:
	dotnet ef database update \
		--project src/FlowBoard.Infrastructure \
		--startup-project src/FlowBoard.Api

## Create a new EF Core migration: make migration name=YourMigrationName
migration:
	dotnet ef migrations add $(name) \
		--project src/FlowBoard.Infrastructure \
		--startup-project src/FlowBoard.Api

## Run all tests
test:
	dotnet test --configuration Release

## Build the solution
build:
	dotnet build --configuration Release

## Format all code
format:
	dotnet format

## Remove all bin/ and obj/ directories
clean:
	find . -type d -name bin -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name obj -exec rm -rf {} + 2>/dev/null || true

## Show this help
help:
	@grep -E '^##' Makefile | sed 's/## /  /'
