# Project Rules

## API Conventions

- All GET endpoints that return lists must accept `[FromQuery] PageRequest pageRequest` and return `PagedResult<T>`.
- `PageRequest` is defined in `Application/Common/Pagination/PageRequest.cs` (Page, PageSize, Skip).
- `PagedResult<T>` is defined in `Application/Common/Pagination/PagedResult.cs` (Items, TotalCount, Page, PageSize, TotalPages).
- Repository methods that return lists should accept `PageRequest` and return `PagedResult<T>` with proper `.Skip()` / `.Take()` / `.CountAsync()`.
- GET endpoints returning a single item by ID, name, or route do NOT need pagination.

## Database

- PostgreSQL via Npgsql + EF Core 8.
- Do NOT use `EnableDynamicJson()` — the app must remain DB-agnostic. Use EF Core value conversions (`HasConversion`) for JSON columns.
- JSON columns use `HasColumnType("jsonb")` with string-based value conversions and `ValueComparer`.

## Architecture

- Clean architecture: Core (entities/enums) → Application (DTOs/interfaces/services) → Infrastructure (EF/repos) → WebAPI (controllers).
- Entities extend `BaseEntity`, `AuditableEntity`, or `TenantEntity` depending on needs.
- `ActionObject` is the universal object for features, folders, URLs, files, etc. Use `ObjectType` enum for discrimination — do NOT create separate entities for each type.
- Soft delete via `IsDeleted` / `DeletedAt` — never hard delete.
- Multi-tenancy via `OrganizationId` with global query filters in `AppDbContext`.
