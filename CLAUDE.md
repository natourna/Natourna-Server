# CLAUDE.md

Guidance for Claude Code when working in **Natourna-Server**, the ASP.NET Core backend of the Natourna platform.

## What this is

REST API for managing residential compounds: buildings → apartments, recurring payment cycles, per-apartment payments split across shared balances, and bills drawn from those balances. JWT auth with two roles (`User`, `Admin`), PostgreSQL via EF Core, audited writes.

Sibling repos (checked out next to this one, separate git repos — do not edit from here):
- `../Natourna-Client` — React + TypeScript + Vite SPA, the only consumer of this API.
- `../Natourna-Infra` — docker-compose stacks (dev/prod), Caddy proxy, VPS setup.

## Stack

.NET 10 (`net10.0`) · ASP.NET Core controllers · EF Core 10 + Npgsql · Serilog · BCrypt.Net-Next · Swashbuckle · nullable + implicit usings enabled.

## Layout & commands

The solution lives one level down: **run all `dotnet` commands from `NatournaServer/`**, except `dotnet tool restore`, which uses the manifest at the repo root.

```bash
cd NatournaServer && dotnet build
```

```bash
cd NatournaServer && dotnet run
```

Dev runs on `http://localhost:8080` with Swagger UI at `/swagger` (Development only). Health: `GET /Health`. Version: `GET /api/Version`.

There is **no test project**. Verify changes by building and exercising endpoints (Swagger, `NatournaServer.http`, or the client).

```
NatournaServer/
  Controllers/      thin HTTP layer — routing, auth attributes, status codes
  Services/Api/     *ApiManager — business rules, DTO mapping, audit logging
  Services/Context/ *ContextManager — EF Core data access, the only place touching DbContext
  Services/Audit/   AuditService
  Interfaces/       Api/, Context/, Authentication/, Audit/ — one interface per manager
  Models/
    Entities/       *Entity, EF-mapped, all extend BaseEntity
    Api/Requests/   inbound DTOs with DataAnnotations
    Api/Response/   outbound DTOs
    Configurations/ options bound from appsettings
    Validation/     custom ValidationAttributes
  Data/             NatournaServerContext (all relationships, indexes, decimal precision)
  Extensions/       one static class per startup concern, called from Program.cs
  Constants/        ErrorCodes, RoleNames, LogAction, ApartmentStatus, PaymentCycle
  Exceptions/       ApiException, ContextException, CustomException, ErrorMessageBuilder
  Migrations/       EF Core migrations
  Docs/README.md    deployment manual
```

## Architecture: the three-layer rule

Every feature flows **Controller → ApiManager → ContextManager → DbContext**. Never skip a layer.

- **Controller** — inject the `I*ApiManager` only. No EF, no business logic. Translate `null` to `NotFound()`, return `Ok`/`CreatedAtAction`/`NoContent`. Carries `[Authorize]` attributes and an XML doc comment stating who may call it (e.g. `/// Create bill - Admin only`).
- **ApiManager** — business rules, cross-entity orchestration, `Entity → Response` mapping via a `private static MapToResponse`, and audit logging. Throws `ApiException` with an explicit `statusCode`.
- **ContextManager** — all EF Core queries. Filtering is done with optional nullable parameters on a single `GetAllAsync(...)` that composes `IQueryable` predicates; `GetByIdAsync` usually delegates to it. Wraps everything in try/catch and throws `ContextException`.

Register new managers in `Extensions/ApiManagerExtension.cs` / `Extensions/ContextManagerExtension.cs` (all `AddScoped`).

## Error handling

Two exception types, distinguished by layer, both carrying `(errorCode, userMessage, technicalDetails)`:

| Exception | Thrown by | HTTP result |
|---|---|---|
| `ApiException` | ApiManagers | its own `statusCode` (default 400) |
| `ContextException` | ContextManagers | 500 |
| `CustomException` | startup/config | 500 |

`Extensions/ExceptionHandlingExtension.cs` is the single catch point and serializes `ErrorResponse { ErrorCode, Message }`. **Only `userMessage` reaches the client** — never put internals in it; `technicalDetails` is for logs.

Every code is a `const string` in `Constants/Error/ErrorCodes.cs`, namespaced by area and layer: `BILL-001` (context) vs `BILL-API-001` (api). Add new codes there, never inline literals.

Message text lives in `Exceptions/ErrorMessageBuilder.cs`, one nested static class per entity, each method returning `(string userMessage, string technicalDetails)`. Add messages there rather than building strings at the throw site.

Canonical throw shape:

```csharp
(string userMessage, string technicalDetails) = ErrorMessageBuilder.Bill.BillNotFound(billId);
_logger.LogWarning("[{ErrorCode}] {ErrorMessage}", ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage);
throw new ApiException(ErrorCodes.BILL_NOT_FOUND_ERROR, userMessage, technicalDetails, statusCode: 404);
```

ApiManager methods with multi-step logic wrap the body in `try { ... } catch (ApiException) { throw; } catch (Exception ex) { ... }` so deliberate failures pass through untouched.

## Domain model

`Compound` → `Building` → `Apartment` (cascade), and `Compound` → `Balance` (cascade). `Balance` → `Bill` and `Balance` → `PaymentAllocation` are **restrict**, as are `Apartment` → `Payment` and `Cycle` → `Payment`.

Money flow — get this right, it is the core of the app:
- A **Cycle** defines a recurring charge (amount, `PaymentCycle` enum, date range, target apartments) plus `BalanceAllocations` whose percentages **must sum to exactly 100**. Creating a cycle expands it into one `Payment` per apartment per occurrence, each with `PaymentAllocation` rows.
- Marking a payment **paid** *credits* each allocated balance by `AllocatedAmount`; unpaid *debits* it back.
- Marking a bill **paid** *debits* its balance, and is rejected with 422 if `balance.CurrentAmount < bill.Amount`; unpaid credits it back.
- Paid/unpaid transitions are idempotency-guarded and return **409** if already in the target state.

These multi-write operations are **not wrapped in a database transaction** — they compensate manually (`RollbackBalanceUpdates`, or re-adding the deducted amount) when a later step fails. Preserve that compensation logic when editing; if you introduce a new multi-write path, either follow the same pattern or introduce a real transaction deliberately.

Entities: `[Key]` + identity `Id`, `[ForeignKey]` + `[JsonIgnore]` navigation properties, a constructor taking required fields that sets `CreatedAt`/`UpdatedAt`. `BaseEntity` supplies `CreatedAt`/`UpdatedAt` as `DateTimeOffset`. **Always set timestamps in UTC** (`DateTime.UtcNow`) — Npgsql rejects non-UTC `timestamptz` values.

All relationships, indexes and decimal precision are configured in `Data/NatournaServerContext.cs` `OnModelCreating`, not with attributes. Money is `HasPrecision(18, 2)`; `Percentage` is `(5, 2)`.

## API conventions

- Routes: `[Route("api/[controller]")]`, so `BillController` → `/api/Bill`. Controllers are `[Authorize]` at class level; relax with `[AllowAnonymous]` and tighten with `[Authorize(Roles = RoleNames.Admin)]` — always the constant, never `"Admin"`.
- Typical shape: read = any authenticated user, write = Admin. `UserController` additionally lets a non-admin act on their own record only, comparing `User.FindFirst(ClaimTypes.Name)` to the target's email and returning `Forbid()` otherwise.
- State transitions are `PATCH {id}/mark-as-paid` style, not PUT.
- Requests are dedicated DTOs in `Models/Api/Requests/` with DataAnnotations; there are separate `XRequest` (create) and `XUpdateRequest` types where update accepts more fields. Never bind an entity directly from the body.
- Responses are DTOs in `Models/Api/Response/`. A few older endpoints (`Balance`, `Cycle`) still return entities — prefer adding a response DTO when touching them.
- List endpoints return `PagedResponse<T>` and take `[FromQuery] PagedQuery` (`Page` ≥ 1, `PageSize` 1–100, default 20) plus optional filter params. Some list endpoints are not yet paginated; follow the paged pattern for new ones.
- Custom validators live in `Models/Validation/` (`RequiredInt`, `RequiredDecimal`, `RequiredEnum`, `PaymentAllocations`) because `[Required]` is meaningless on non-nullable value types.

## Multi-tenancy (SaaS)

The paying customer is an **Organization** (`OrganizationEntity`); billing is per building (`SubscriptionEntity`, one per org, monthly cost = `PricePerBuilding × building count`, always computed, never stored). A single-building customer is an organization whose compound contains exactly one building — **the compound never disappears from the data model, only from the UI**.

- Every tenant entity implements `ITenantEntity` (`Models/Entities/ITenantEntity.cs`) and carries an `OrganizationId`: Compound, Building, Apartment, Balance, Bill, Payment, PaymentAllocation, Cycle, User. `Role` is global; `Audit.OrganizationId` is nullable and stamped best-effort in `AuditService`.
- **Reads** are scoped by global query filters in `NatournaServerContext.OnModelCreating`, driven by `_tenantOrganizationId` (from `ITenantContext`, i.e. the JWT's `orgId` claim — see `CustomClaimTypes`). Filters are permissive when no tenant is in scope (login lookup, startup seeding). **Never add manual org filtering to ContextManagers** — the filters do it.
- **Writes** are stamped in the context's `SaveChangesAsync` override: an added `ITenantEntity` with `OrganizationId == 0` gets the current tenant; if there is no tenant either, it **throws** (`TENANT-01`). Code running outside a request (seeding, registration) must set `OrganizationId` explicitly.
- The DbContext constructor takes `ITenantContext`; `TenancyExtension.AddTenancy` must be registered before `AddPostgreSqlService` in `Program.cs`.
- `POST /api/Organization/register` (anonymous, gated by `Registration:Enabled`, default off) creates org + trial subscription + admin + compound + buildings in one call. `GET /api/Organization/me` and `PUT /api/Organization/settings` (name + `LbpExchangeRate`, the LBP-per-USD display rate) serve the client.
- Error code areas: `ORG-xxx` / `ORG-API-xxx` / `SUB-xxx`.

## Auth

`JwtAuthenticationService` issues HS256 tokens with `ClaimTypes.Name` = email, `ClaimTypes.NameIdentifier` = user id, `ClaimTypes.Role`, `orgId` = organization (tenant scope), plus `jti`/`iat`. `ClockSkew` is zero; expired tokens come back with a `Token-Expired` response header. Passwords are BCrypt via `IPasswordHashingService` (singleton) and `UserEntity.Password` is `[JsonIgnore]`.

At startup `SeedRolesAsync` guarantees the `User`/`Admin` rows exist, then `SeedBootstrapAdminAsync` creates an admin from `Bootstrap:AdminEmail`/`Bootstrap:AdminPassword` **only when the users table is empty**.

Security middleware in `SecurityExtension.cs`: CORS restricted to `Cors:AllowedOrigins` (empty list = nothing allowed), a global 100 req/10 s per-IP rate limit, a 5 req/min `auth` policy on `POST /api/Auth/login`, and `X-Content-Type-Options` / `X-Frame-Options` / `Referrer-Policy` headers.

## Logging & auditing

Serilog writes to console and a daily rolling file (`/app/logs` in Docker, `Logs/` beside the binary locally, 30-day retention). Use structured templates — `_logger.LogInformation("Marking bill {BillId} as paid", billId)` — never interpolated strings.

`LoggingExtension` logs every request with its body, **skipping `/api/Auth` and `/api/User`**. Add any new route carrying secrets to `SensitiveBodyPaths`.

Mutating ApiManager operations call `_auditService.LogAsync(LogAction.X, "EntityType", id, oldValues, newValues)` with anonymous objects for the before/after snapshots. New create/update/delete paths should do the same. `AuditService` swallows its own failures so auditing never breaks a request.

## Configuration

`appsettings.json` holds the keys with empty values; real values come from environment variables using the `__` separator (see `../Natourna-Infra/*/docker-compose.yml`):

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__SecretKey` (≥ 32 chars), `__Issuer`, `__Audience`, `__ExpirationMinutes` (default 60)
- `Bootstrap__AdminEmail`, `Bootstrap__AdminPassword`
- `Cors__AllowedOrigins__0`, …
- `NatournaServer__Port` (default 8080; Kestrel binds this explicitly and the server header is suppressed)

Never commit real secrets; local overrides go in user-secrets or gitignored `appsettings.local.json`.

## Database & migrations

```bash
cd NatournaServer && dotnet ef migrations add "1.5.0_YourChange"
```

Migration names are prefixed with the app version (`1.2.0_InitialPostgreSql`, `1.4.0_RolesTable`); keep that convention and bump `<Version>` in `NatournaServer.csproj` alongside schema changes.

**Development uses `EnsureCreatedAsync()`, production uses `MigrateAsync()`** (`ContextManagerExtension.AddContextService`). A dev database created that way has no migration history — drop and recreate it after adding a migration rather than expecting it to apply.

## Deployment

Push to `develop` or `main` triggers `.github/workflows/deploy.yml`: builds `NatournaServer/Dockerfile`, pushes `ghcr.io/natourna/natourna-server:{dev|prod}`, then SSHes to the VPS and runs `docker compose pull server && docker compose up -d` in `/srv/natourna/{dev|prod}`. `Docs/README.md` is the full deployment manual.

## Git rules

- **Branches follow git flow**: new work happens on `feature/<topic>` branches cut from `develop` (e.g. `feature/ri-organizations`); `hotfix/*` from `main`; features merge back into `develop`; `main` is production and only receives merges from `develop`/release.
- **Claude must NOT push** to any remote unless the user explicitly says so in that conversation. **Never push to `main` under any circumstances** — pushing to `develop`/`main` triggers the deploy workflow.
- Commit subjects are lowercase past tense describing the effect, e.g. `paginated the bill list endpoint`.

## Conventions

- Namespaces mirror folders, with the notable exception that `Interfaces/Audit/IAuditService.cs` declares `NatournaServer.Interfaces.Services` — match the existing `using` when touching it.
- `ILogContextManager` is implemented by `AuditContextManager` (the audit trail was formerly called "log"); the mismatch is intentional in the current code.
- Prefer explicit types over `var` for the result of manager calls, matching the surrounding code. Braces on their own line, 4-space indent, `_camelCase` private fields, constructor injection only.
- File-scoped namespaces in `Extensions/`; block-scoped elsewhere. Follow whichever the file already uses.
- When you change a request/response DTO or a route, the client in `../Natourna-Client` needs the matching update — flag it even though it lives in another repo.
