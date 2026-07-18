# Vendo-FormBuilder

Production-ready ASP.NET Core (.NET 10) microservice for creating, versioning, publishing, and collecting responses for unlimited dynamic forms — without code changes.

## Architecture

Clean Architecture with inward-pointing dependencies:

| Project | Responsibility |
|---------|----------------|
| `Vendo.FormBuilder.Api` | REST endpoints, Serilog, Swagger, global exception handling |
| `Vendo.FormBuilder.Application` | CQRS (MediatR), FluentValidation, DTOs, business workflows |
| `Vendo.FormBuilder.Domain` | Entities, enums, domain rules, repository interfaces |
| `Vendo.FormBuilder.Infrastructure` | EF Core, SQL Server, persistence implementations |

```
Api → Application → Domain
      Infrastructure → Application / Domain
```

## Technology Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10 + SQL Server
- MediatR (CQRS)
- FluentValidation
- Serilog
- Swagger / OpenAPI
- Docker / Docker Compose (optional)

## Features

- Create, edit, soft-delete forms
- Version forms (clone published/archived → new draft)
- Publish and archive lifecycle
- Unlimited fields per form with reorder support
- Validation rules and selectable options
- Submit and retrieve form responses
- Pagination & filtering
- Optimistic concurrency (`RowVersion`)
- Soft delete + global query filters
- ProblemDetails error responses
- Async APIs throughout

## Out of Scope (v1)

The initial version intentionally does **not** include:

- Authentication
- Authorization
- User management
- Workflow engine
- Notifications
- Frontend UI
- Reporting

Optional `createdBy` / `updatedBy` / `submittedBy` strings are audit metadata only — not a user system.

## Supported Field Types

`Text`, `MultilineText`, `Number`, `Decimal`, `Date`, `Time`, `DateTime`, `Email`, `Phone`, `Url`, `Checkbox`, `RadioButton`, `Dropdown`, `MultiSelect`, `Password`, `FileUpload`, `ImageUpload`

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (local or Docker)

### Run with Docker Compose

```bash
docker compose up --build
```

- API: http://localhost:8080
- Swagger UI: http://localhost:8080 (Development)
- SQL Server: `localhost:1433` (sa / `Your_strong_Password123`)

Migrations apply automatically when `Database__MigrateOnStartup=true`.

### Run locally

```bash
# Start SQL Server (example)
docker compose up sqlserver -d

# Apply migrations
dotnet ef database update \
  --project src/Vendo.FormBuilder.Infrastructure \
  --startup-project src/Vendo.FormBuilder.Api

# Run API
dotnet run --project src/Vendo.FormBuilder.Api
```

### Tests

```bash
dotnet test
```

## API Overview

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/forms` | List forms (page, search, status) |
| `GET` | `/api/forms/{id}` | Get form with fields |
| `POST` | `/api/forms` | Create draft form |
| `PUT` | `/api/forms/{id}` | Update draft form |
| `POST` | `/api/forms/{id}/publish` | Publish form |
| `POST` | `/api/forms/{id}/archive` | Archive form |
| `POST` | `/api/forms/{id}/versions` | Create next draft version |
| `DELETE` | `/api/forms/{id}` | Soft-delete form |
| `POST` | `/api/forms/{id}/fields` | Add field |
| `PUT` | `/api/forms/{id}/fields/{fieldId}` | Update field |
| `DELETE` | `/api/forms/{id}/fields/{fieldId}` | Soft-delete field |
| `PUT` | `/api/forms/{id}/fields/reorder` | Reorder fields |
| `POST` | `/api/forms/{id}/responses` | Submit response |
| `GET` | `/api/forms/{id}/responses` | List responses |
| `GET` | `/api/responses/{id}` | Get response by id |
| `GET` | `/api/health` | Health check |

### Example: Create form → add field → publish → submit

```bash
# 1. Create form
curl -X POST http://localhost:8080/api/forms \
  -H "Content-Type: application/json" \
  -d '{"name":"Contact Us","description":"Website contact","slug":"contact-us","createdBy":"admin"}'

# 2. Add field (replace FORM_ID)
curl -X POST http://localhost:8080/api/forms/FORM_ID/fields \
  -H "Content-Type: application/json" \
  -d '{
    "name":"email",
    "label":"Email",
    "fieldType":"Email",
    "displayOrder":0,
    "isRequired":true,
    "validationRules":[{"ruleType":"Email","value":"true","errorMessage":"Invalid email"}]
  }'

# 3. Publish
curl -X POST http://localhost:8080/api/forms/FORM_ID/publish \
  -H "Content-Type: application/json" \
  -d '{"actor":"admin"}'

# 4. Submit response (replace FIELD_ID)
curl -X POST http://localhost:8080/api/forms/FORM_ID/responses \
  -H "Content-Type: application/json" \
  -d '{
    "submittedBy":"user@example.com",
    "values":[{"fieldId":"FIELD_ID","value":"user@example.com"}]
  }'
```

## Design Notes

- **Draft-only edits**: Published/archived forms are immutable; create a new version to change structure.
- **Optimistic concurrency**: Send `rowVersion` (Base64) on update endpoints.
- **Soft delete**: Entities are marked deleted and excluded via EF global query filters.
- **Response validation**: Required rules, type checks, option membership, min/max, and regex are enforced at submission time.

## Solution Structure

```
src/
  Vendo.FormBuilder.Api/
  Vendo.FormBuilder.Application/
  Vendo.FormBuilder.Domain/
  Vendo.FormBuilder.Infrastructure/
tests/
  Vendo.FormBuilder.UnitTests/
docker-compose.yml
Dockerfile
```

## License

MIT
