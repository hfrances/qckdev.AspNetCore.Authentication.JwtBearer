# JwtBearerExample.Identity

Ejemplo completo de autenticación/autorización local con:

- ASP.NET Core Identity (`IdentityUser` + `IdentityRole`)
- JWT para API
- Cookie para UI de login/logout
- Seed InMemory con usuarios y roles demo

## Proyectos

- `JwtBearerExample.Identity`: API principal con endpoints de auth/roles/UI.
- `JwtBearerExample.Identity.AppHost`: host Aspire para levantar el ejemplo.
- `JwtBearerExample.Identity.ServiceDefaults`: defaults de observabilidad/salud.

## Configuración JWT local

La configuración JWT vive en `appsettings.json` del proyecto API (`JwtBearerExample.Identity`), en:

- `Authentication:Jwt:Issuer`
- `Authentication:Jwt:Audience`
- `Authentication:Jwt:Key`
- `Authentication:Jwt:AccessExpireMinutes`

Si necesitas sobrescribir valores, puedes hacerlo con variables de entorno.

## Seed InMemory

Roles:

- `Admin`
- `Support`
- `Viewer`

Usuarios:

- `admin@local.dev` / `Admin123!` -> `Admin`, `Viewer`
- `support@local.dev` / `Support123!` -> `Support`, `Viewer`
- `viewer@local.dev` / `Viewer123!` -> `Viewer`
- `ops@local.dev` / `Ops12345!` -> `Admin`, `Support`

## Endpoints API (JWT)

- `GET /auth/public` (anónimo)
- `POST /auth/token` (email/password -> JWT)
- `GET /auth/whoami` (autenticado JWT)
- `GET /roles/admin` (rol `Admin`)
- `GET /roles/support` (rol `Support`)
- `GET /roles/either` (rol `Admin` o `Support`)
- `GET /weatherforecast` (autenticado JWT)

## Endpoints UI (cookie)

- `GET /ui/login`
- `POST /ui/login`
- `GET /ui/me` (requiere cookie)
- `POST /ui/logout`

## Smoke test rápido

1. Ejecutar `JwtBearerExample.Identity.AppHost` o el proyecto API.
2. Llamar `POST /auth/token` con uno de los usuarios demo.
3. Usar el JWT en `Authorization: Bearer <token>`.
4. Probar restricciones de rol:
   - token `Admin` accede a `/roles/admin` y falla en `/roles/support`.
   - token `Support` accede a `/roles/support` y falla en `/roles/admin`.
