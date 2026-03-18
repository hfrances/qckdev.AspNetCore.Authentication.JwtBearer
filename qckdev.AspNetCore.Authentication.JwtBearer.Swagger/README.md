[![NuGet Version](https://img.shields.io/nuget/v/qckdev.AspNetCore.Authentication.JwtBearer.Swagger.svg)](https://www.nuget.org/packages/qckdev.AspNetCore.Authentication.JwtBearer.Swagger)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=qckdev.AspNetCore.Authentication.JwtBearer.Swagger&metric=alert_status)](https://sonarcloud.io/dashboard?id=qckdev.AspNetCore.Authentication.JwtBearer.Swagger)
[![Code Coverage](https://sonarcloud.io/api/project_badges/measure?project=qckdev.AspNetCore.Authentication.JwtBearer.Swagger&metric=coverage)](https://sonarcloud.io/dashboard?id=qckdev.AspNetCore.Authentication.JwtBearer.Swagger)
![Azure Pipelines Status](https://hfrances.visualstudio.com/qckdev-suite/_apis/build/status/azure-devops-pipelines?branchName=main)


# qckdev.AspNetCore.Authentication.JwtBearer.Swagger

Helper that feeds the tokens your API issues into Swagger UI so developers never need to copy/paste Authorization headers manually.

## 📦 Packages

- `qckdev.AspNetCore.Authentication.JwtBearer`: validation helpers plus generator wiring for the API schemes your services expose.
- `qckdev.AspNetCore.Authentication.JwtBearer.Swagger`: a Swagger UI helper that captures your API-issued tokens and pre-authorizes the matching bearer scheme.

## 🛠️ Installation

```bash
dotnet add package qckdev.AspNetCore.Authentication.JwtBearer.Swagger
```

## ⚡ Quick Start

1. **Register JWT bearer authentication the traditional way.**  
   Begin by wiring `AddJwtBearer` for every scheme your API enforces so that middleware can authenticate incoming requests and Swagger exposes the correct authorization entries. This is the same code you would add without the helper.

   ```csharp
   builder.Services.AddAuthentication()
       .AddJwtBearer("Bearer", options =>
       {
           options.Authority = builder.Configuration["Jwt:Authority"];
           options.Audience = "api-demo";
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateIssuerSigningKey = true,
               ValidateLifetime = true,
               ValidateIssuer = true,
               ValidateAudience = true
           };
       })
       .AddJwtBearer("BearerInstant", options =>
       {
           options.TokenValidationParameters = new TokenValidationParameters
           {
               RequireExpirationTime = true,
               ClockSkew = TimeSpan.FromSeconds(5)
           };
       });

   builder.Services.AddAuthorization();
   ```

2. **Add Swagger authentication the typical way (baseline experience).**  
   Configure `AddSwaggerGen` with a bearer security definition and requirement so the “Authorize” button appears, then enable Swagger UI so users can work with protected endpoints just by pasting `Bearer <token>` into that dialog. This full setup is what most teams ship before adding any helper logic.

    ```csharp
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            BearerFormat = "JWT",
            Name = "JWT Authentication",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Description = "JWT Authorization header using the Bearer scheme.<br>" +
                          "Enter <b>ONLY</b> your token in the text input (without the prefix 'Bearer').",
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    });
    ```

    ```csharp
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo API v1");
    });
    ```

3. **Enable Swagger UI and mount the helper on top of the default flow.**  
    Use `UseJwtBearerSwaggerUi` inside `UseSwaggerUI` so the helper can emit its embedded JavaScript that watches every request, extracts tokens, and pre-authorizes the matching scheme. Previously developers had to copy/paste Authorization headers after logging in; this helper avoids that manual step while still running after the traditional setup.

   ```csharp
   app.UseSwagger();
   app.UseSwaggerUI(c =>
   {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo API v1");
        c.UseJwtBearerSwaggerUi(cfg =>
        {
            cfg.AddRule("Bearer", "/security/jwt/token", rule =>
            {
                rule.TokenJsonPath = "accessToken";
                rule.Log = true; // enable console tracing during development
            });

            cfg.AddRule("BearerInstant", "/security/jwt/token-instant", rule =>
            {
                rule.TokenJsonPath = "accessToken";
            });
        });
   });
   ```

4. **Describe what each rule looks like.**  
   The helper serializes every rule into the script tag, so keep the schema consistent:

   ```json
   {
     "SchemeName": "Bearer",
     "EndpointPath": "/security/jwt/token",
     "HttpMethod": "POST",
     "TokenJsonPath": "accessToken",
     "StripBearerPrefix": true,
     "Log": false
   }
   ```

   | Property | Purpose |
   | --- | --- |
   | `SchemeName` | Matches the name used in `AddJwtBearer` and in Swagger's authorization dialog. |
   | `EndpointPath` | Endpoint that returns the token JSON (adjust for path base if Swagger is deployed under `/api`). |
   | `HttpMethod` | Defaults to `POST`; the helper uses both method and path to identify the correct response. |
   | `TokenJsonPath` | JSON path that leads to the token string inside the response. |
   | `StripBearerPrefix` | Set to `false` if the response already includes the `Bearer <token>` prefix. |
   | `Log` | Enables DevTools tracing; keep it `true` when debugging and `false` in production. |

   Add one rule per scheme so the helper can map each token response back into Swagger's `Authorize` button automatically.

## 🧰 How the helper works

1. `UseJwtBearerSwaggerUiStaticFiles()` serves the embedded `jwt-bearer-auth.js`.  
2. `UseJwtBearerSwaggerUi(cfg => …)` adds a `<script>` tag with a query string that describes every rule.  
3. The browser script reads each rule (case-insensitive keys), builds a map keyed by `METHOD + normalized path`, and patches `window.fetch`.  
4. When a response matches, it follows `TokenJsonPath`, optionally removes the `Bearer ` prefix, and calls `window.ui.preauthorizeApiKey`/`authActions.authorize` to populate the scheme.  
5. Only rules with `Log = true` emit console messages, so the helper stays silent in production unless you deliberately trace it.

## 🤝 Contributing
Issues and pull requests are welcome! See the contribution guidelines (coming soon).

## 📜 License
This project is licensed under the terms of the [MIT License](LICENSE).
