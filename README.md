# ConferenceHallBooking API

REST API for managing conference hall bookings — .NET 8, Clean Architecture, CQRS.

## Features

- **JWT authentication** — register/login, 60-minute tokens; each client IP gets
  its own fixed window of **10 requests per 15 minutes** (429 on exceed).
- **Hall management** — CRUD for Admins; availability search by time slot and capacity
  for any authenticated user.
- **Race-safe bookings** — a PostgreSQL `EXCLUDE` constraint rejects overlaps
  (`tstzrange` with `[)` bounds, so back-to-back bookings are allowed); each booking
  is visible only to its owner or an Admin.
- **Reports** — revenue, utilization, and booking summary for a period (Admin only).
- **Operational basics** — `/health` endpoint, Swagger UI, RFC 7807
  `application/problem+json` errors.

## Architecture

| Project | Contents |
|---|---|
| `src/ConferenceHallBooking.Api` | controllers, `Program` composition root, Swagger, rate limiting, health checks |
| `src/ConferenceHallBooking.Application` | commands/queries and handlers (MediatR), FluentValidation, DTOs, mappers, pricing |
| `src/ConferenceHallBooking.Domain` | entities, invariants, domain exceptions |
| `src/ConferenceHallBooking.Infrastructure` | EF Core 8 + Npgsql, repositories, unit of work, JWT/BCrypt services, migrations, seed |
| `tests/ConferenceHallBooking.UnitTests` | hermetic xUnit suite (InMemory, NSubstitute) — 266 tests |
| `tests/ConferenceHallBooking.PostgresTests` | live-PostgreSQL overlap and concurrency tests (3 tests, gated by `POSTGRES_TEST_CONNECTION`) |

## Getting Started

### Prerequisites

Docker with Compose, **or** .NET SDK 10.x (the solution is a `.slnx`; projects target `net8.0`).

### Run with Docker

```bash
git clone https://github.com/TasminskyiRuslan/conference-hall-booking.git
cd conference-hall-booking
cp .env.example .env
# edit .env: set JWT_SECRET_KEY (required, e.g. `openssl rand -base64 48`),
# optionally ADMIN_EMAIL + ADMIN_PASSWORD to seed an admin user
docker compose up -d --build
docker compose run --rm api --migrate    # apply migrations + seed, then exits
```

Open `http://localhost:8080` - health at `/health` (`Healthy` once migrated).
Swagger UI at `/swagger` is served only in the `Development` environment
(the local run profile; the Docker image runs `Production` without it).
(`Healthy` once migrated).

Migrations are never applied automatically; `--migrate` is the single migration entry
point (Docker and local runs alike).

```bash
docker compose down        # stop, keep the database volume
docker compose down -v     # stop and delete the database volume
```

### Run locally

```bash
docker compose up -d db                                        # PostgreSQL on localhost:5433
dotnet run --project src/ConferenceHallBooking.Api -- --migrate  # schema + seed, then exits
dotnet run --project src/ConferenceHallBooking.Api               # http://localhost:5133
```

The `Development` profile provides the JWT secret and admin credentials
(`appsettings.Development.json`) and enables Swagger at `/swagger`.

### Run tests

```bash
# full suite without a database (the 3 live tests no-op)
dotnet test ConferenceHallBooking.slnx
```

```bash
# full suite including the live PostgreSQL tests (requires the db container)
docker compose up -d db
POSTGRES_TEST_CONNECTION='Host=localhost;Port=5433;Database=conference_booking;Username=postgres;Password=postgres' \
  dotnet test ConferenceHallBooking.slnx
```

```powershell
# PowerShell
$env:POSTGRES_TEST_CONNECTION = 'Host=localhost;Port=5433;Database=conference_booking;Username=postgres;Password=postgres'
dotnet test ConferenceHallBooking.slnx
```

Totals: **269 tests** = 266 unit + 3 live.

## Configuration

| Key | Environment variable | Default |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` | `Host=localhost;Port=5433;Database=conference_booking;Username=postgres;Password=postgres` (compose sets `Host=db`) |
| `JwtSettings:SecretKey` | `JWT_SECRET_KEY` (fallback) | empty in `appsettings.json`; `Development` provides a dev key; compose requires the variable (startup fails without it) |
| `JwtSettings:Issuer` / `Audience` | — | `ConferenceHallBooking` / `ConferenceHallBookingApp` |
| `JwtSettings:ExpirationInMinutes` | — | `60` |
| `AdminSeed:Email` / `Password` | `ADMIN_EMAIL` / `ADMIN_PASSWORD` (compose) | empty in `appsettings.json`; `Development` defaults to `admin@conference.local` / `Admin@12345`; compose seeds only when both are set |
| `RateLimiting:Auth:PermitLimit` / `WindowMinutes` | — | `10` / `15` (per client IP) |
| `RateLimiting:Enabled` | — | `true` |
| `Cors:AllowedOrigins` | — | empty — cross-origin requests are denied; list origins to allow them |
| `PricingSettings:Rules` | — | `×0.90` 06:00–09:00, `×1.15` 12:00–14:00, `×0.80` 18:00–23:00 |
| `PricingSettings:TimeZoneId` | вЂ” | `Europe/Kyiv` вЂ” tariff windows and multipliers are evaluated in this timezone |
| — | `POSTGRES_PORT` / `API_PORT` | `5433` / `8080` (compose) |
| — | `ASPNETCORE_ENVIRONMENT` | `Production` (compose) |
| — | `POSTGRES_TEST_CONNECTION` | unset — live tests run as a no-op (they pass without a database) |

> **Reverse proxy:** ForwardedHeaders middleware is intentionally not enabled, so the
> app always sees the direct peer IP. Behind a reverse proxy the auth rate limiter
> partitions by the proxy IP (one shared bucket for all clients) - keep the API
> directly reachable, or account for shared-IP limiting when proxying.

## Roles

| Role | Capabilities |
|---|---|
| Customer | search and view halls, create bookings, view own bookings |
| Admin | all Customer capabilities, plus hall CRUD and all reports |

## Business Tasks & Technical Solutions

| Business task | Technical solution |
|---|---|
| Find a free hall for a time slot | `GET /api/hall/available` — SQL-side overlap check per hall, capacity filter |
| Prevent double-booking | PostgreSQL `EXCLUDE USING gist` on `tstzrange` `[)` — back-to-back allowed, overlap → 409 |
| Protect accounts | JWT (60 min) + BCrypt; register/login rate-limited per client IP → 429 + `Retry-After` |
| Grant capabilities by role | Admin-only endpoints guarded by role authorization → 403 |
| Keep bookings private | owner-or-Admin access; anyone else gets 404 so existence is not disclosed |
| Price bookings by time of day | `PricingSettings:Rules` window multipliers applied by `PricingService` in the `PricingSettings:TimeZoneId` time zone (Europe/Kyiv) |
| Report on hall usage | Admin reports: revenue, utilization, summary for a period |

## API Endpoints

| Method | Path | Access | Notes |
|---|---|---|---|
| GET | `/health` | — | returns `Healthy` |
| GET | `/swagger` | — | Swagger UI (Development only) |
| POST | `/api/auth/register` | — | → 201; rate-limited → 429 |
| POST | `/api/auth/login` | — | rate-limited → 429 |
| GET | `/api/hall/available?startTime&endTime&capacity` | Bearer | halls free for the slot |
| GET | `/api/hall/{id}` | Bearer | |
| POST | `/api/hall` | Admin | |
| PUT | `/api/hall/{id}` | Admin | 409 when removing options used by bookings |
| DELETE | `/api/hall/{id}` | Admin | 409 when the hall has bookings |
| GET | `/api/booking/{id}` | Bearer | owner or Admin; 404 otherwise |
| POST | `/api/booking` | Bearer | 409 on overlap; 400 on invalid input |
| GET | `/api/report/revenue?from&to` | Admin | revenue breakdown by hall |
| GET | `/api/report/utilization?from&to` | Admin | booked vs available hours |
| GET | `/api/report/summary?from&to` | Admin | totals and popular time slots |

Errors follow RFC 7807 (`application/problem+json`) with stable machine-readable codes
(`HALL_ALREADY_BOOKED`, `HALL_OPTION_IN_USE`, `UNIQUE_CONSTRAINT_VIOLATION`, …) and a
`traceId`.

## Seed Data

`--migrate` applies pending migrations and seeds each of the following (every step is
idempotent):

| Seeded data | Values |
|---|---|
| Halls | `Зал А` (50 / 2000), `Зал B` (100 / 3500), `Зал C` (30 / 1500) — capacity / hourly rate |
| Options, attached to every hall | `Проєктор` 500, `Wi-Fi` 300, `Звук` 700 |
| Pricing rules | from `PricingSettings:Rules` (see Configuration) |
| Admin user | from `AdminSeed` — `Development`: `admin@conference.local` / `Admin@12345`; compose: only when `ADMIN_EMAIL` + `ADMIN_PASSWORD` are set |

Customers are not seeded — register them via `POST /api/auth/register`.

## Tech Stack

| Area | Choice |
|---|---|
| Runtime | .NET 8, ASP.NET Core MVC |
| API style | REST with RFC 7807 problem details |
| Authentication | JWT Bearer, BCrypt password hashing |
| Persistence | EF Core 8 + Npgsql; race safety via `btree_gist` exclusion constraint |
| Application layer | MediatR + FluentValidation |
| Documentation | Swagger / OpenAPI (Development) |
| Health | EF-backed health checks (`/health`) |
| Tests | xUnit, FluentAssertions, NSubstitute, coverlet |
| Packaging | Docker multi-stage (`sdk:8.0` → `aspnet:8.0`), docker compose |
