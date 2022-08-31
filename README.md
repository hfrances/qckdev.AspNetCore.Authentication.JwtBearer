<a href="https://www.nuget.org/packages/qckdev.AspNetCore.Authentication.JwtBearer"><img src="https://img.shields.io/nuget/v/qckdev.AspNetCore.Authentication.JwtBearer.svg" alt="NuGet Version"/></a>
<a href="https://sonarcloud.io/dashboard?id=qckdev.AspNetCore.Authentication.JwtBearer"><img src="https://sonarcloud.io/api/project_badges/measure?project=qckdev.AspNetCore.Authentication.JwtBearer&metric=alert_status" alt="Quality Gate"/></a>
<a href="https://sonarcloud.io/dashboard?id=qckdev.AspNetCore.Authentication.JwtBearer"><img src="https://sonarcloud.io/api/project_badges/measure?project=qckdev.AspNetCore.Authentication.JwtBearer&metric=coverage" alt="Code Coverage"/></a>
<a><img src="https://hfrances.visualstudio.com/qckdev/_apis/build/status/qckdev.AspNetCore.Authentication.JwtBearer?branchName=master" alt="Azure Pipelines Status"/></a>


# qckdev.AspNetCore.Authentication.JwtBearer

Provides tools to configure JWT bearer authentication.

``` json

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

``` cs

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using System.Text;

    sealed class JwtTokenConfiguration
    {
        public string Key { get; set; } = string.Empty;
        public double? AccessExpireSeconds { get; set; }
    }

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
                moreOptions.TokenLifeTimespan = (configuration.AccessExpireSeconds.HasValue ? TimeSpan.FromSeconds(configuration.AccessExpireSeconds.Value) : (TimeSpan?)null);
            }
        );
    }
    
```

``` cs

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Options;
    using qckdev.AspNetCore.Authentication.JwtBearer;
    using qckdev.Authentication.JwtBearer;
    using System.Security.Claims;

    sealed class JwtGeneratorService : IJwtGeneratorService
    {

        IOptionsMonitor<JwtBearerOptions> JwtOptionsMonitor { get; }
        IOptionsMonitor<JwtBearerMoreOptions> JwtMoreOptionsMonitor { get; }


        public JwtGeneratorService(IOptionsMonitor<JwtBearerOptions> jwtOptionsMonitor, IOptionsMonitor<JwtBearerMoreOptions> jwtMoreOptionsMonitor)
        {
            this.JwtOptionsMonitor = jwtOptionsMonitor;
            this.JwtMoreOptionsMonitor = jwtMoreOptionsMonitor;
        }

        public Task<JwtToken> CreateTokenAsync(string scheme, string userName, IEnumerable<string> roles, IEnumerable<Claim> claims)
        {
            var jwtOptions = JwtOptionsMonitor.Get(scheme);
            var jwtMoreOptions = JwtMoreOptionsMonitor.Get(scheme);

            return Task.FromResult(
                JwtGenerator.CreateToken(jwtOptions.TokenValidationParameters.IssuerSigningKey, userName, roles, claims, jwtMoreOptions.TokenLifeTimespan)
            );
        }

    }

```

``` cs

    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.Configuration;

    const string AUTHENTICATIONSCHEME_CODE = "Code";
    const string AUTHENTICATIONSCHEME_TOKEN = "Bearer";

    var jwtCodeConfiguration = Configuration.GetSection("OAuth2:Code").Get<JwtTokenConfiguration>();
    var jwtTokenConfiguration = Configuration.GetSection("OAuth2:Token").Get<JwtTokenConfiguration>();

    services.AddAuthentication(AUTHENTICATIONSCHEME_CODE)
        .AddJwtBearer(AUTHENTICATIONSCHEME_CODE, jwtCodeConfiguration);
    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(AUTHENTICATIONSCHEME_TOKEN, jwtTokenConfiguration);

```
