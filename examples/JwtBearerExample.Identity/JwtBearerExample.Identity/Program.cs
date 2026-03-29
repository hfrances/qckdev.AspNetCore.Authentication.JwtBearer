using JwtBearerExample.Identity.Data;
using JwtBearerExample.Identity.Security;
using JwtBearerExample.Identity.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace JwtBearerExample.Identity;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Authentication:Jwt"));

        builder.Services.AddDbContext<AppIdentityDbContext>(options =>
            options.UseInMemoryDatabase("JwtBearerIdentity"));

        builder.Services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>()
            .AddDefaultTokenProviders();

        var jwtOptions = builder.Configuration.GetSection("Authentication:Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var keyBytes = Encoding.UTF8.GetBytes(jwtOptions.Key);
        if (keyBytes.Length <= 64)
        {
            throw new InvalidOperationException("Authentication:Jwt:Key must be at least 65 bytes when using HS512.");
        }

        var key = new SymmetricSecurityKey(keyBytes);

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.AdminOnly, policy =>
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireRole("Admin"));
            options.AddPolicy(AuthPolicies.SupportOnly, policy =>
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireRole("Support"));
            options.AddPolicy(AuthPolicies.AdminOrSupport, policy =>
                policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireRole("Admin", "Support"));
        });

        builder.Services.AddScoped<JwtTokenService>();
        builder.Services.AddControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwagger();

        var app = builder.Build();
        app.MapDefaultEndpoints();

        await IdentitySeeder.SeedAsync(app.Services);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapRazorPages();

        await app.RunAsync();
    }
}
