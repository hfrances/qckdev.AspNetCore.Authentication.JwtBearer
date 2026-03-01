# Integration Testing

## Overview

This project includes comprehensive integration tests that validate the complete JWT Bearer authentication pipeline in a realistic ASP.NET Core context.

## Test Architecture

The tests spin up a minimal ASP.NET Core host for isolation and validation:

1. **LocalTestServiceManager** - Hosts ASP.NET Core application on dynamic port (27000 + PID)
2. **TestAssemblySetup** - Manages host lifecycle (startup/shutdown)
3. **JwtBearerAuthenticationTestController** - Provides test endpoints
4. **JwtBearerAuthenticationIntegrationTests** - Validates HTTP behavior and token handling

### Why Integration Tests?

Integration tests validate the **complete JWT authentication pipeline** including:
- ✅ Token generation and signing
- ✅ Bearer scheme parsing
- ✅ Signature verification
- ✅ Token expiration validation
- ✅ Claims extraction
- ✅ HTTP status codes and error responses

This catches issues that unit tests cannot: header parsing, token format validation, timestamp handling, etc.

## Test Coverage

### Integration Tests (8 tests)

#### Public Endpoint (Unauthenticated Access)

**PublicEndpoint_WithoutToken_ReturnsOk**
```csharp
GET /jwt/public
// No Authorization header

Expected: HTTP 200 OK
Response: "public-ok"
```
Validates unrestricted endpoint access without authentication.

#### Challenge Response (401 Unauthorized)

**ProtectedEndpoint_WithoutToken_ReturnsUnauthorized**
```csharp
GET /jwt/protected
// No Authorization header

Expected: HTTP 401 Unauthorized
```
Validates proper challenge response when token is missing.

#### Valid Token Authentication

**ProtectedEndpoint_WithValidToken_ReturnsOk**
```csharp
GET /jwt/protected
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

Expected: HTTP 200 OK
Response: "protected-ok-testuser"
Identity: ClaimTypes.Name = "testuser"
```
Validates successful authentication with valid JWT token.

#### Custom Subject Claims

**ProtectedEndpoint_WithValidTokenAndCustomSubject_ReturnsOk**
```csharp
GET /jwt/protected
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
// Token with aud claim = "customuser"

Expected: HTTP 200 OK
Response: "protected-ok-customuser"
Identity: ClaimTypes.Name = "customuser"
```
Validates custom subject claim extraction from token.

#### Token Expiration

**ProtectedEndpoint_WithExpiredToken_ReturnsUnauthorized**
```csharp
GET /jwt/protected
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
// Token with exp claim in the past (issued 2h ago, expired 1h ago)

Expected: HTTP 401 Unauthorized
```
Validates strict token expiration enforcement with zero clock skew.

#### Malformed Token Format

**ProtectedEndpoint_WithMalformedToken_ReturnsUnauthorized**
```csharp
GET /jwt/protected
Authorization: Bearer invalid.token.here

Expected: HTTP 401 Unauthorized
```
Validates error handling for non-JWT format in Authorization header.

#### Missing Bearer Scheme

**ProtectedEndpoint_WithMissingBearerScheme_ReturnsUnauthorized**
```csharp
GET /jwt/protected
Authorization: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
// Token without "Bearer " prefix

Expected: HTTP 401 Unauthorized
```
Validates that Bearer scheme is required.

#### Wrong Authentication Scheme

**ProtectedEndpoint_WithWrongScheme_ReturnsUnauthorized**
```csharp
GET /jwt/protected
Authorization: Basic eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
// Wrong scheme (Basic instead of Bearer)

Expected: HTTP 401 Unauthorized
```
Validates rejection of non-Bearer schemes.

## Test Configuration

### Test Environment

| Setting | Value |
|---------|-------|
| **Service Port** | Dynamic (27000 + Process ID) |
| **Signing Algorithm** | HMAC-SHA256 |
| **Default Token Lifetime** | 1 hour |
| **Clock Skew** | 0 (strict expiration) |
| **Subject Claim** | `ClaimTypes.NameIdentifier` and `ClaimTypes.Name` |

### Token Generation Helper

Tests use `LocalTestServiceManager.GenerateValidToken()` to create tokens:

```csharp
// Basic valid token (expires in 1 hour)
var token = LocalTestServiceManager.GenerateValidToken();
// Returns: JWT with subject "testuser"

// Custom subject
var token = LocalTestServiceManager.GenerateValidToken("customuser");
// Returns: JWT with subject "customuser"

// Custom lifetime
var token = LocalTestServiceManager.GenerateValidToken(
    lifetime: TimeSpan.FromSeconds(30)
);
// Returns: JWT that expires in 30 seconds

// Expired token
var token = LocalTestServiceManager.GenerateValidToken(expired: true);
// Returns: JWT with IssuedAt 2h ago, expires 1h ago
```

## Running Tests

### Run All Tests

```bash
cd qckdev.AspNetCore.Authentication.JwtBearer
dotnet test qckdev.AspNetCore.Authentication.JwtBearer.sln -v minimal
```

### Run Specific Test

```bash
dotnet test --filter "FullyQualifiedName~JwtBearerAuthenticationIntegrationTests.ProtectedEndpoint_WithValidToken"
```

### Run with Detailed Logging

```bash
dotnet test --logger "console;verbosity=detailed" --no-build
```

## Test Execution Flow

```
[AssemblyInitialize]
  ↓
  LocalTestServiceManager.StartIfNeeded()
    ├─ Create IHostBuilder
    ├─ Configure services (AddControllers, AddAuthentication with JwtBearer)
    ├─ Configure middleware (UseRouting, UseAuthentication, UseEndpoints)
    ├─ Start host on dynamic port
    └─ Wait for service ready (polling, 10s timeout)
  ↓
[Test Methods Execute]
  ├─ Create HttpClient with base address from service
  ├─ Generate JWT token (if needed) using test signing key
  ├─ Build request with Authorization header
  ├─ Send request to live endpoint
  └─ Assert response status, headers, content, identity
  ↓
[AssemblyCleanup]
  ↓
  LocalTestServiceManager.Stop()
    └─ Dispose host
```

## Multi-Framework Coverage

Tests execute against all supported frameworks:

- .NET Core 3.1 (EOL December 2022)
- .NET 5.0 (EOL May 2022)
- .NET 6.0 (LTS until November 2024)
- .NET 8.0 (LTS until November 2026)
- .NET 10.0 (Current, supported until November 2027)

Each framework runs **8 integration tests** independently.

## Key Validations

✅ **Bearer scheme parsing** - Case-insensitive scheme matching  
✅ **Token format** - Valid JWT structure (3 parts separated by `.`)  
✅ **Signature verification** - HMAC-SHA256 signature validation  
✅ **Token expiration** - Enforced with zero clock skew  
✅ **Claims extraction** - Subject claim mapped to user identity  
✅ **Error handling** - Graceful handling of malformed tokens  
✅ **Identity mapping** - Authenticated subject available via `User.Identity.Name`  
✅ **Scheme validation** - Bearer scheme required, other schemes rejected  

## Token Structure

Sample token payload (decoded):

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "testuser",
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "testuser",
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "testuser",
    "iat": 1740826923,
    "nbf": 1740826923,
    "exp": 1740830523
  }
}
```

## Troubleshooting

### Test Hangs on Service Ready

**Symptom**: Test hangs for 10 seconds then fails  
**Cause**: Service not responding on expected port  
**Solution**: 
- Verify port is not in use
- Check that .NET runtime is installed
- Review startup configuration in LocalTestServiceManager

### Token Generation Fails

**Symptom**: `ArgumentException: Expires must be after NotBefore`  
**Cause**: Clock skew or invalid timestamp calculation  
**Solution**:
- Token must have IssuedAt before Expires
- Token manager handles this automatically
- Check system clock is synchronized

### Invalid Signature Error

**Symptom**: "Invalid signature" error on valid-looking token  
**Cause**: Signing key mismatch  
**Solution**:
- Ensure token is generated with same key as validator uses
- Check key is 256+ bits for HS256
- LocalTestServiceManager handles key consistency automatically

### Token Not Recognized

**Symptom**: Custom tokens rejected with "Invalid token"  
**Cause**: Token claims or structure doesn't match expectations  
**Solution**:
- Verify token has required claims (sub, exp, nbf, iat)
- Ensure exp is in future (not expired)
- Check signature is valid

## Best Practices

1. **Always use HTTPS in production** - JWT transmitted in header is susceptible to interception
2. **Validate issuer and audience** - For multi-tenant scenarios
3. **Implement token refresh** - Shorter lived tokens reduce exposure
4. **Test expiration handling** - Ensure graceful degradation on token expiration
5. **Monitor token age** - Track how long tokens are held before refresh
6. **Use strong signing keys** - Minimum 256 bits for HS256
7. **Parallel execution** - Tests are thread-safe and work with parallel execution

## Security Considerations

### Signature Verification

This test suite uses:
- **Algorithm**: HMAC-SHA256
- **Key Size**: 256+ bits
- **Validation**: Enabled by default in tests

Production implementations should:
- Use RS256 or ES256 (asymmetric) for better key management
- Store secrets in secure vaults (Azure Key Vault, HashiCorp Vault)
- Rotate keys regularly
- Audit token generation and validation

### Token Lifetime

Test tokens:
- Valid for 1 hour by default
- Can be customized per test
- Strict expiration (no clock skew)

Production recommendations:
- Use shorter lifetimes (15-30 minutes)
- Refresh tokens for long-lived sessions
- Consider token revocation lists for logout scenarios

## Related Documentation

- [Test Coverage Guide](../../TEST_COVERAGE.md) - Coverage across all projects
- [Framework Compatibility](COMPATIBILITY.md) - Supported frameworks and versions
- [Main README](../README.md) - Usage examples and quickstart

---

Last updated: March 1, 2026
