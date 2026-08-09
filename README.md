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
| `RestaurantId` | No | `null` or `0` = subscriber-level (shared across that subscriber's restaurants). Positive int = restaurant-specific and isolated from other restaurants. |

Access rules:

- A subscriber cannot access forms belonging to another subscriber.
- When `restaurantId` is supplied on read/write APIs, results include that restaurant's forms **plus** subscriber-level shared forms (`RestaurantId = null`).
- Restaurant-specific forms are not visible to other restaurants of the same subscriber.
- Omitting `restaurantId` is a subscriber-wide scope (can manage all forms for that subscriber).

Tenant scope is passed as:

- **Admin form-builder APIs** (`/api/forms`, `/api/forms/{id}/fields`): `x-subscriber-id` header; optional `restaurantId` remains query/body
- **Data-entry APIs** (`/api/forms/{id}/responses`, `/api/responses/...`): required query param `subscriberId`, optional `restaurantId`

### Admin form-builder token headers

All admin form-builder endpoints take tenant/identity **only from headers** (not query):

| Header | Purpose |
|--------|---------|
| `x-user-id` | Calling user id |
| `x-role-id` | Role id; `1013` is treated as admin and bypasses subscriber membership |
| `x-subscriber-id` | Target subscriber |
| `x-subscriber-ids` | JSON array of subscriber ids the caller may access, e.g. `[1,2,3]` |

Applies to list/get/create/update/publish/archive/version/delete forms and all field APIs.

Access is granted when the caller is admin (`x-role-id = 1013`) **or** `x-subscriber-id` is present in `x-subscriber-ids`. Otherwise the API returns `401 Unauthorized` with `"Invalid token"`.

Data-entry endpoints (`/api/forms/{formId}/responses`, `/api/responses/...`) do **not** use these headers.

## Out of Scope (v1)

The initial version intentionally does **not** include:

- Full Authentication / Authorization / User management (gateway header checks apply to admin form-builder APIs)
- Workflow engine
- Notifications
- Frontend UI
- Reporting

- Optional `createdBy` / `updatedBy` / `submittedBy` strings are audit metadata only — not a user system.
- Form and field identifiers (`FormId` / `FieldId`) are `bigint` (`long` in .NET). Response/option/rule ids remain Guid.

## Supported Field Types

`Text`, `MultilineText`, `Number`, `Decimal`, `Date`, `Time`, `DateTime`, `Email`, `Phone`, `Url`, `Checkbox`, `RadioButton`, `Dropdown`, `MultiSelect`, `Password`, `FileUpload`, `ImageUpload`, `Province`, `City`

### Province and City lookups

`Province` and `City` are lookup field types: they carry no per-field options. A client renders them
by loading the shared reference data, then submits the selected **id** as the response value.

| Endpoint | Returns |
|----------|---------|
| `GET /api/provinces` | `[{ "id": 1, "name": "…", "orderIndex": 1 }]` |
| `GET /api/provinces/{provinceId}/cities` | `[{ "id": 101, "provinceId": 1, "name": "…", "orderIndex": 1 }]` |

Both lists are ordered by `orderIndex`. Unknown `provinceId` returns `404`; a non-positive one returns `400`.

The data is reference data shared by every subscriber and is seeded by the
`AddProvinceAndCityLookups` migration, so there are no create/update/delete endpoints.

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

### Troubleshooting: `PUT` / `DELETE` return 405 on IIS

IIS enables WebDAV by default, and it answers `405 Method Not Allowed` for `PUT` and `DELETE`
before the request reaches ASP.NET Core (`GET` and `POST` keep working). `src/Vendo.FormBuilder.Api/web.config`
removes the WebDAV module and handler; restart the site or app pool after deploying it.
Kestrel and Docker are unaffected.

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
| `PUT` | `/api/responses/{id}?subscriberId=` | Update response |
| `DELETE` | `/api/responses/{id}?subscriberId=` | Soft-delete response |
| `GET` | `/api/provinces` | List provinces ordered by `orderIndex` |
| `GET` | `/api/provinces/{provinceId}/cities` | List a province's cities ordered by `orderIndex` |
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
