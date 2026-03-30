# JwtBearerExample.MultipleSchemes (Docker)

This example can be started with Docker Compose, like other examples that include containerized setup.

## Run

From this folder:

```bash
docker compose up --build -d
```

## Swagger

Open:

```text
http://localhost:5220/api/swagger
http://localhost:5220/api-proxy/swagger
```

Direct API (without proxy):

```text
http://localhost:5221/swagger
```

## Quick test

Generate tokens:

```bash
curl -X POST http://localhost:5221/jwt/token/code -H "Content-Type: application/json" -d "{\"userName\":\"code.user\"}"
curl -X POST http://localhost:5221/jwt/token/bearer -H "Content-Type: application/json" -d "{\"userName\":\"token.user\"}"
```

Through nginx reverse proxy:

```bash
curl -X POST http://localhost:5220/api/jwt/token/code -H "Content-Type: application/json" -d "{\"userName\":\"code.user\"}"
curl -X POST http://localhost:5220/api/jwt/token/bearer -H "Content-Type: application/json" -d "{\"userName\":\"token.user\"}"
curl -X POST http://localhost:5220/api-proxy/jwt/token/code -H "Content-Type: application/json" -d "{\"userName\":\"code.user\"}"
curl -X POST http://localhost:5220/api-proxy/jwt/token/bearer -H "Content-Type: application/json" -d "{\"userName\":\"token.user\"}"
```

Protected endpoints:

```bash
curl -X GET http://localhost:5221/jwt/protected/code -H "Authorization: Bearer <CODE_TOKEN>"
curl -X GET http://localhost:5221/jwt/protected/bearer -H "Authorization: Bearer <BEARER_TOKEN>"
curl -X GET http://localhost:5221/jwt/protected/either -H "Authorization: Bearer <CODE_OR_BEARER_TOKEN>"
```

Through nginx reverse proxy:

```bash
curl -X GET http://localhost:5220/api/jwt/protected/code -H "Authorization: Bearer <CODE_TOKEN>"
curl -X GET http://localhost:5220/api/jwt/protected/bearer -H "Authorization: Bearer <BEARER_TOKEN>"
curl -X GET http://localhost:5220/api/jwt/protected/either -H "Authorization: Bearer <CODE_OR_BEARER_TOKEN>"
curl -X GET http://localhost:5220/api-proxy/jwt/protected/code -H "Authorization: Bearer <CODE_TOKEN>"
curl -X GET http://localhost:5220/api-proxy/jwt/protected/bearer -H "Authorization: Bearer <BEARER_TOKEN>"
curl -X GET http://localhost:5220/api-proxy/jwt/protected/either -H "Authorization: Bearer <CODE_OR_BEARER_TOKEN>"
```

## Stop

```bash
docker compose down
```
