# Examples

This folder contains runnable examples for `qckdev.AspNetCore.Authentication.JwtBearer`.

Each example follows scenarios described in the package `README`.

## What You Will Find

- `JwtBearerExample.SingleScheme`: single Bearer scheme configured from `appsettings`, including token generation service.
- `JwtBearerExample.SingleSchemeWithRoles`: single scheme plus role-based authorization examples (`Admin`, `Support`).
- `JwtBearerExample.MultipleSchemes`: `Code` + `Bearer` schemes with raw Swagger bearer auth.
- `JwtBearerExample.Identity`: local ASP.NET Core Identity (InMemory) with JWT + cookie/UI and per-user roles.

## Common Endpoints

All examples expose:

- `GET /jwt/public` -> public endpoint.
- `GET /jwt/protected` -> protected endpoint.

Token endpoints:

- `JwtBearerExample.SingleScheme`: `POST /jwt/token`
- `JwtBearerExample.SingleSchemeWithRoles`: `POST /jwt/token`
- `JwtBearerExample.MultipleSchemes`:
  - `POST /jwt/token/code`
  - `POST /jwt/token/bearer`
- `JwtBearerExample.Identity`:
  - `POST /auth/token`

## 1) JwtBearerExample.SingleScheme

### What It Demonstrates

- `JwtTokenConfiguration` bound from `OAuth2:Token`.
- One Bearer scheme registration with `AddJwtBearer(..., moreOptions => ...)`.
- Validation settings synced with optional token lifetime.
- `IJwtGeneratorService` implementation using:
  - `IOptionsMonitor<JwtBearerOptions>`
  - `IOptionsMonitor<JwtBearerMoreOptions>`
- Runtime token creation with the same scheme/options used by validation.

### Request Example

```bash
curl -X POST http://localhost:5200/jwt/token ^
  -H "Content-Type: application/json" ^
  -d "{\"userName\":\"demo.user\",\"roles\":[\"Admin\"]}"
```

## 2) JwtBearerExample.SingleSchemeWithRoles

### What It Demonstrates

- Single Bearer scheme with token generation service.
- Role-protected endpoints:
  - `GET /jwt/roles/admin` -> requires `Admin`.
  - `GET /jwt/roles/support` -> requires `Support`.
  - `GET /jwt/roles/either` -> accepts `Admin` or `Support`.

### Request Example

```bash
curl -X POST http://localhost:5230/jwt/token ^
  -H "Content-Type: application/json" ^
  -d "{\"userName\":\"roles.user\",\"roles\":[\"Support\"]}"
```

## 3) JwtBearerExample.MultipleSchemes

### What It Demonstrates

- Two configured schemes from `OAuth2:Code` and `OAuth2:Token`.
- Explicit scheme authorization on endpoints.
- Raw Swagger bearer configuration (Swashbuckle security definition + requirement).

### Request Examples

```bash
curl -X POST http://localhost:5220/jwt/token/code -H "Content-Type: application/json" -d "{\"userName\":\"code.user\"}"
curl -X POST http://localhost:5220/jwt/token/bearer -H "Content-Type: application/json" -d "{\"userName\":\"token.user\"}"
```

### Docker

From `examples/JwtBearerExample.MultipleSchemes`:

```bash
docker compose up --build -d
```

This compose starts `api` + `nginx` reverse proxy. Use Swagger via `http://localhost:5220/api/swagger`.

## 4) JwtBearerExample.Identity

### What It Demonstrates

- Local ASP.NET Core Identity (`IdentityUser` + `IdentityRole`) without Entra ID.
- InMemory seed with demo users and different roles per user.
- JWT API flow plus cookie login/logout UI flow in the same backend.
- Role policies:
  - `GET /roles/admin` -> requires `Admin`.
  - `GET /roles/support` -> requires `Support`.
  - `GET /roles/either` -> accepts `Admin` or `Support`.

### Request Examples

```bash
curl -X POST http://localhost:5288/auth/token ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"admin@local.dev\",\"password\":\"Admin123!\"}"
```

## Run

From each example folder:

```bash
dotnet run -f net8.0
```

For Swagger (`JwtBearerExample.MultipleSchemes`):

```text
http://localhost:5220/swagger
```

## Suggested Learning Order

1. `JwtBearerExample.SingleScheme`
2. `JwtBearerExample.SingleSchemeWithRoles`
3. `JwtBearerExample.MultipleSchemes`
4. `JwtBearerExample.Identity`
