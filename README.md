# Visit Management API

.NET 10 HTTP API for customs visit records. Clean Architecture (Domain, Application, Infrastructure, Api), MySQL via EF Core + Pomelo, JWT Bearer with scopes `visits:read` and `visits:write`.

## Prerequisites

- .NET 10 SDK
- Docker
- [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) **9.0.19** (matches EF Core / Pomelo 9; there is no official Pomelo for EF 10)

```bash
dotnet tool install --global dotnet-ef --version 9.0.19
```

`jq` is optional; used below to capture the access token.

## Run MySQL

Compose publishes MySQL on host port **3308** (container 3306). Copy `.env.example` to `.env` if you want to override the default passwords.

```bash
cp .env.example .env   # optional; defaults are fine for local use
docker compose up -d
```

The API reads `ConnectionStrings:Visits` from `src/VisitManagement.Api/appsettings.Development.json` (`Server=localhost;Port=3308;...`). Do not use `sudo mysql` on the host socket; that is not the app database.

## Migrate

The `InitialVisits` migration already exists. Apply it; do not add it again.

```bash
dotnet ef database update \
  --project src/VisitManagement.Infrastructure \
  --startup-project src/VisitManagement.Api
```

## Run the API

```bash
dotnet run --project src/VisitManagement.Api --launch-profile http
```

Listens on `http://localhost:5192`. HTTPS profile: `https://localhost:7277` (HTTP still on 5192). OpenAPI is mapped in Development only.

## Authentication

Visit routes require `Authorization: Bearer <jwt>`.

Local Development issues tokens at `POST /api/v1/auth/token` for client `dev-client` / secret `dev-secret` (SHA-256 hash in `appsettings.Development.json`). The JWT `sub` is written to `createdBy` / `updatedBy`. Scopes: `visits:read`, `visits:write`.

```bash
TOKEN=$(curl -sS -X POST http://localhost:5192/api/v1/auth/token \
  -H "Content-Type: application/json" \
  -d '{"clientId":"dev-client","clientSecret":"dev-secret"}' \
  | jq -r .accessToken)
```

Without `jq`, copy `accessToken` from the JSON body into `TOKEN`. Invalid credentials return **401** ProblemDetails. Missing or invalid Bearer on visit routes returns **401**; wrong scope returns **403**.

## Sample requests

Licence numbers are trimmed and uppercased (`ab12 xyz` → `AB12 XYZ`). Activity JSON uses `startAt` / `endAt`.

### Create — `POST /api/v1/visits` (`visits:write`) — 201

```bash
curl -sS -X POST http://localhost:5192/api/v1/visits \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Active",
    "vehicleLicenceNumber": "ab12 xyz",
    "visitor": { "id": "P123456", "firstName": "Jane", "lastName": "Doe" },
    "activities": [{
      "type": "Business",
      "travellerNumber": "TN-001",
      "startAt": "2026-09-10T00:00:00Z",
      "endAt": "2026-09-20T00:00:00Z"
    }]
  }'
```

### List — `GET /api/v1/visits` (`visits:read`) — 200

Query: `page` (default 1, must be ≥ 1), `pageSize` (default 50, must be 1–100). Response: `items`, `page`, `pageSize`, `totalCount`. `pageSize` over 100 returns **400** ProblemDetails.

```bash
curl -sS "http://localhost:5192/api/v1/visits?page=1&pageSize=50" \
  -H "Authorization: Bearer $TOKEN"
```

### Get by id — `GET /api/v1/visits/{id}` (`visits:read`) — 200 or 404

```bash
curl -sS "http://localhost:5192/api/v1/visits/<id>" \
  -H "Authorization: Bearer $TOKEN"
```

### Update — `PUT /api/v1/visits/{id}` (`visits:write`) — 200 or 404

Same body as create. Replaces status, licence, visitor, and the full activities list. `createdAt` / `createdBy` stay; `updatedAt` / `updatedBy` refresh from the token.

```bash
curl -sS -X PUT "http://localhost:5192/api/v1/visits/<id>" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Completed",
    "vehicleLicenceNumber": "xy99 zzz",
    "visitor": { "id": "P123456", "firstName": "Jane", "lastName": "Doe" },
    "activities": [{
      "type": "Pleasure",
      "travellerNumber": "TN-002",
      "startAt": "2026-09-10T00:00:00Z",
      "endAt": "2026-09-20T00:00:00Z"
    }]
  }'
```

Errors use RFC 9457 ProblemDetails (400 validation, 401/403 auth, 404 missing visit, 429 rate limit).

## Configuration

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:Visits` | MySQL. Required outside the `Testing` environment. |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:LifetimeMinutes` | Token validation and local issuance. |
| `Jwt:SigningKey` | HMAC key; **at least 32 characters**. Empty in `appsettings.json`; set in Development settings or user-secrets. |
| `AuthClients` | `clientId`, SHA-256 hex `secretHash`, `scopes`. Empty in base config. |
| `Cors:AllowedOrigins` | Explicit origins. Empty array **denies all** origins. |
| `RateLimiting:PermitLimit` / `WindowSeconds` | Fixed window per authenticated `sub` or client IP (default 60 / 60). |

Hash a client secret (UTF-8 SHA-256, lowercase hex):

```bash
printf '%s' 'your-secret' | sha256sum
```

User-secrets for anything that is not Development:

```bash
dotnet user-secrets set "Jwt:SigningKey" "<at-least-32-chars>" \
  --project src/VisitManagement.Api
dotnet user-secrets set "ConnectionStrings:Visits" "<mysql-connection-string>" \
  --project src/VisitManagement.Api
```

Do not commit production signing keys, client secrets, or connection passwords. `.env` is gitignored.

## Production

Replace `POST /api/v1/auth/token` with an IdP (Entra ID, IdentityServer, or similar) using the client-credentials flow. Keep the same JWT validation: issuer, audience, HMAC or the IdP signing keys, and scopes `visits:read` / `visits:write`. Enable HSTS (already on outside Development/Testing). Point `Cors:AllowedOrigins` at real front-end origins.

## Tests

API tests use an in-memory repository (`Testing` environment) and do not need MySQL.

```bash
dotnet test VisitManagement.slnx --nologo
```

## OpenAPI

Development: [http://localhost:5192/openapi/v1.json](http://localhost:5192/openapi/v1.json).

Visit operations declare Bearer (`Authorization: Bearer`). `POST /api/v1/auth/token` is anonymous.
