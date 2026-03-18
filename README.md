[![NuGet Version](https://img.shields.io/nuget/v/qckdev.AspNetCore.Authentication.JwtBearer.svg)](https://www.nuget.org/packages/qckdev.AspNetCore.Authentication.JwtBearer)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=qckdev.AspNetCore.Authentication.JwtBearer&metric=alert_status)](https://sonarcloud.io/dashboard?id=qckdev.AspNetCore.Authentication.JwtBearer)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=qckdev.AspNetCore.Authentication.JwtBearer&metric=coverage)](https://sonarcloud.io/dashboard?id=qckdev.AspNetCore.Authentication.JwtBearer)
![Azure Pipelines Status](https://hfrances.visualstudio.com/qckdev/_apis/build/status/qckdev.AspNetCore.Authentication.JwtBearer?branchName=master)


# qckdev.AspNetCore.Authentication.JwtBearer

Provides helpers to register JWT bearer schemes and keep token generation in sync with validation settings.

## 📦 Packages

- `qckdev.AspNetCore.Authentication.JwtBearer`: validation helpers plus generator wiring for the API schemes your services expose.
- `qckdev.AspNetCore.Authentication.JwtBearer.Swagger`: a Swagger UI helper that captures your API-issued tokens and pre-authorizes the matching bearer scheme.

## 🛠️ Installation

```bash
dotnet add package qckdev.AspNetCore.Authentication.JwtBearer
```

## ⚡ Quick Start

### 1. Model the JWT settings you expect to read from configuration

```json
{
  "OAuth2": {
    "Code": {
      "Key": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
      "AccessExpireSeconds": 60
    },
    "Token": {
      "Key": "yyyyyyyyyyyyyyyyyyyyyyyyyyyyyy",
      "AccessExpireSeconds": 86400
    }
  }
}
```

```csharp
sealed class JwtTokenConfiguration
{
    public string Key { get; set; } = string.Empty;
    public double? AccessExpireSeconds { get; set; }
}
```

Bind each section in `Program.cs` or `Startup.cs` using `IConfiguration.GetSection(...).Get<JwtTokenConfiguration>()` so the next steps share the same symmetric key plus optional lifetime.

### 2. Register the extension that wires validation + generator services

```csharp
public static AuthenticationBuilder AddJwtBearer(this AuthenticationBuilder builder, string authenticationScheme, JwtTokenConfiguration configuration)
{
    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.Key));

    builder.Services.AddScoped<IJwtGeneratorService, JwtGeneratorService>();
    return builder.AddJwtBearer(authenticationScheme,
        options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        },
        moreOptions =>
        {
            moreOptions.TokenLifeTimespan = configuration.AccessExpireSeconds.HasValue
                ? TimeSpan.FromSeconds(configuration.AccessExpireSeconds.Value)
                : (TimeSpan?)null;
        }
    );
}
```

This keeps the `IJwtGeneratorService` that depends on `JwtBearerOptions` in sync with what the middleware expects; the same `TokenLifeTimespan` used to validate tokens is applied when generating them.

### 3. Generate tokens from the configured options

```csharp
sealed class JwtGeneratorService : IJwtGeneratorService
{
    IOptionsMonitor<JwtBearerOptions> JwtOptionsMonitor { get; }
    IOptionsMonitor<JwtBearerMoreOptions> JwtMoreOptionsMonitor { get; }

    public JwtGeneratorService(IOptionsMonitor<JwtBearerOptions> jwtOptionsMonitor, IOptionsMonitor<JwtBearerMoreOptions> jwtMoreOptionsMonitor)
    {
        JwtOptionsMonitor = jwtOptionsMonitor;
        JwtMoreOptionsMonitor = jwtMoreOptionsMonitor;
    }

    public Task<JwtToken> CreateTokenAsync(string scheme, string userName, IEnumerable<string> roles, IEnumerable<Claim> claims)
    {
        var jwtOptions = JwtOptionsMonitor.Get(scheme);
        var jwtMoreOptions = JwtMoreOptionsMonitor.Get(scheme);

        return Task.FromResult(
            JwtGenerator.CreateToken(
                jwtOptions.TokenValidationParameters.IssuerSigningKey,
                userName,
                roles,
                claims,
                jwtMoreOptions.TokenLifeTimespan)
        );
    }
}
```

Inject the service wherever you build tokens for clients, passing the same `scheme` string used to register the handler so the correct validation/key pair is selected.

### 4. Add both authentication schemes to the DI container

```csharp
const string AUTHENTICATIONSCHEME_CODE = "Code";
const string AUTHENTICATIONSCHEME_TOKEN = "Bearer";

var jwtCodeConfiguration = Configuration.GetSection("OAuth2:Code").Get<JwtTokenConfiguration>();
var jwtTokenConfiguration = Configuration.GetSection("OAuth2:Token").Get<JwtTokenConfiguration>();

services.AddAuthentication(AUTHENTICATIONSCHEME_CODE)
    .AddJwtBearer(AUTHENTICATIONSCHEME_CODE, jwtCodeConfiguration);

services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(AUTHENTICATIONSCHEME_TOKEN, jwtTokenConfiguration);
```

You can use `[Authorize(AuthenticationSchemes = "...")]` on controllers or actions if you mix multiple schemes in the same API.

## 🧪 Testing

This library includes comprehensive integration tests covering token validation, signature verification, and expiration handling.

**8 integration tests** validate the complete JWT Bearer authentication pipeline:
- Public endpoint access
- Challenge response (401 Unauthorized)
- Valid and expired tokens
- Token format validation
- Bearer scheme and claims handling

For detailed testing documentation, see [Integration Testing Guide](docs/TESTING.md).

## 🤝 Contributing
Issues and pull requests are welcome! See the contribution guidelines (coming soon).

## 📜 License
This project is licensed under the terms of the [MIT License](LICENSE).
