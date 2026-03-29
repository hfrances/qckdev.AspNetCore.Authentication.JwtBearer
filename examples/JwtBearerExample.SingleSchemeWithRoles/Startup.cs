using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace JwtBearerExample.SingleSchemeWithRoles
{
    public class Startup
    {
        public const string PolicyAdminOnly = "AdminOnly";
        public const string PolicySupportOnly = "SupportOnly";
        public const string PolicyAdminOrSupport = "AdminOrSupport";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var jwtConfiguration = Configuration.GetSection("OAuth2:Token").Get<JwtTokenConfiguration>() ?? new JwtTokenConfiguration();
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtConfiguration.Key));

            services.AddScoped<IJwtGeneratorService, JwtGeneratorService>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
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
                        moreOptions.TokenLifeTimespan = jwtConfiguration.AccessExpireSeconds.HasValue
                            ? TimeSpan.FromSeconds(jwtConfiguration.AccessExpireSeconds.Value)
                            : (TimeSpan?)null;
                    });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyAdminOnly, policy => policy.RequireRole("Admin"));
                options.AddPolicy(PolicySupportOnly, policy => policy.RequireRole("Support"));
                options.AddPolicy(PolicyAdminOrSupport, policy => policy.RequireRole("Admin", "Support"));
            });
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
