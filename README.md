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
- Multi-tenant ownership (`SubscriberId` required, `RestaurantId` optional)
- Pagination & filtering
- Optimistic concurrency (`RowVersion`)
- Soft delete + global query filters
- ProblemDetails error responses
- Async APIs throughout

## Multi-Tenant Form Ownership

Every form is owned by a subscriber and optionally a restaurant:

| Field | Required | Behavior |
|-------|----------|----------|
| `SubscriberId` | Yes | Form always belongs to one subscriber. Cross-subscriber access is denied. |
| `RestaurantId` | No | `null` = subscriber-level (shared across that subscriber's restaurants). Set = restaurant-specific and isolated from other restaurants. |

Access rules:

- A subscriber cannot access forms belonging to another subscriber.
- When `restaurantId` is supplied on read/write APIs, results include that restaurant's forms **plus** subscriber-level shared forms (`RestaurantId = null`).
- Restaurant-specific forms are not visible to other restaurants of the same subscriber.
- Omitting `restaurantId` is a subscriber-wide scope (can manage all forms for that subscriber).

Tenant scope is passed as:

- **Create**: `subscriberId` / `restaurantId` in the request body (ownership)
- **All other endpoints**: required query param `subscriberId`, optional `restaurantId`

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
| `GET` | `/api/forms?subscriberId=` | List forms (page, search, status, optional `restaurantId`) |
| `GET` | `/api/forms/{id}?subscriberId=` | Get form with fields |
| `POST` | `/api/forms` | Create draft form (`subscriberId` required in body) |
| `PUT` | `/api/forms/{id}?subscriberId=` | Update draft form |
| `POST` | `/api/forms/{id}/publish?subscriberId=` | Publish form |
| `POST` | `/api/forms/{id}/archive?subscriberId=` | Archive form |
| `POST` | `/api/forms/{id}/versions?subscriberId=` | Create next draft version |
| `DELETE` | `/api/forms/{id}?subscriberId=` | Soft-delete form |
| `POST` | `/api/forms/{id}/fields?subscriberId=` | Add field |
| `PUT` | `/api/forms/{id}/fields/{fieldId}?subscriberId=` | Update field |
| `DELETE` | `/api/forms/{id}/fields/{fieldId}?subscriberId=` | Soft-delete field |
| `PUT` | `/api/forms/{id}/fields/reorder?subscriberId=` | Reorder fields |
| `POST` | `/api/forms/{id}/responses?subscriberId=` | Submit response |
| `GET` | `/api/forms/{id}/responses?subscriberId=` | List responses |
| `GET` | `/api/responses/{id}?subscriberId=` | Get response by id |
| `GET` | `/api/health` | Health check |

### Example: Create form → add field → publish → submit

```bash
# 1. Create subscriber-level form (shared across restaurants)
curl -X POST http://localhost:8080/api/forms \
  -H "Content-Type: application/json" \
  -d '{"subscriberId":1,"name":"Contact Us","description":"Website contact","slug":"contact-us","createdBy":"admin"}'

# Restaurant-specific form (optional restaurantId):
# -d '{"subscriberId":1,"restaurantId":10,"name":"Local Survey","slug":"local-survey"}'

# 2. Add field (replace FORM_ID and SUBSCRIBER_ID)
curl -X POST "http://localhost:8080/api/forms/FORM_ID/fields?subscriberId=SUBSCRIBER_ID" \
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
curl -X POST "http://localhost:8080/api/forms/FORM_ID/publish?subscriberId=SUBSCRIBER_ID" \
  -H "Content-Type: application/json" \
  -d '{"actor":"admin"}'

# 4. Submit response (replace FIELD_ID)
curl -X POST "http://localhost:8080/api/forms/FORM_ID/responses?subscriberId=SUBSCRIBER_ID" \
  -H "Content-Type: application/json" \
  -d '{
    "submittedBy":"user@example.com",
    "values":[{"fieldId":"FIELD_ID","value":"user@example.com"}]
  }'
```

## Design Notes

- **Multi-tenant ownership**: `SubscriberId` is required; `RestaurantId` is optional (`null` = shared at subscriber level).
- **Draft-only edits**: Published/archived forms are immutable; create a new version to change structure.
- **Optimistic concurrency**: Send `rowVersion` (Base64) on update endpoints.
- **Soft delete**: Entities are marked deleted and excluded via EF global query filters.
- **Response validation**: Required rules, type checks, option membership, min/max, and regex are enforced at submission time.
- **Slug uniqueness**: Scoped per `(SubscriberId, RestaurantId, Slug, Version)`.

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
